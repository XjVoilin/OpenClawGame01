using System;
using cfg;
using JulyArch;
using JulyCore;

namespace SpiritHealer
{
    /// <summary>
    /// 时间系统 —— 行为驱动时间模型。
    /// 时间不自动流逝，由玩家行为（看诊、种植等）消耗游戏内分钟推进。
    /// 时段由当天时刻对照 TbTime 配表阈值自动推算。
    /// 每 DaysPerSeason 天轮转季节（春→夏→秋→冬）。
    /// </summary>
    public class TimeSystem : GameSystemBase
    {
        private TimeStore _store;

        public bool IsOpen => _store.IsOpen;
        public int MinuteOfDay => _store.MinuteOfDay;
        public ETimePhase CurrentPhase => _store.CurrentPhase;

        protected override void OnInitialize()
        {
            _store = GetStore<TimeStore>();
        }

        /// <summary>
        /// 如果当前时刻在 DayStart 之前（如新游戏 MinuteOfDay=0），
        /// 自动设置为当天开门时间并触发 Morning 事件。
        /// </summary>
        public void EnsureDayStarted()
        {
            var tbTime = GF.Config.GetTable<TbTime>();
            if (_store.MinuteOfDay < tbTime.DayStart)
            {
                _store.SetMinuteOfDay(tbTime.DayStart);
            }
        }

        /// <summary>
        /// 消耗游戏内时间。自动检测并逐个触发跨越的时段边界事件。
        /// </summary>
        public void ConsumeTime(int minutes)
        {
            var remaining = minutes;
            var oldPhase = _store.CurrentPhase;
            while (remaining > 0)
            {
                var toNext = GetMinutesToNextPhase();
                var step = Math.Min(remaining, toNext);
                _store.AddMinutes(step);
                remaining -= step;
            }
            
            var newPhase = _store.CurrentPhase;
            if (oldPhase != newPhase)
            {
                Publish(new PhaseChangedEvent { OldPhase = oldPhase, NewPhase = newPhase });
            }
        }

        /// <summary>结束当天：触发夜间结算，然后推进到下一天早晨。</summary>
        public void EndDay()
        {
            var tbTime = GF.Config.GetTable<TbTime>();
            var oldPhase = _store.CurrentPhase;
            if (oldPhase != ETimePhase.Night)
            {
                _store.SetMinuteOfDay(tbTime.Night);
                Publish(new PhaseChangedEvent { OldPhase = oldPhase, NewPhase = ETimePhase.Night });
            }

            _store.AdvanceDay();
            CheckSeasonTransition();
            _store.SetMinuteOfDay(tbTime.DayStart);

            Publish(new DayChangedEvent
            {
                NewDay = _store.Day,
                CurrentSeason = _store.CurrentSeason
            });

            Publish(new PhaseChangedEvent { OldPhase = ETimePhase.Night, NewPhase = ETimePhase.Morning });
        }

        private int GetMinutesToNextPhase()
        {
            var tbTime = GF.Config.GetTable<TbTime>();
            var m = _store.MinuteOfDay;
            if (m < tbTime.Noon)      return tbTime.Noon - m;
            if (m < tbTime.Afternoon) return tbTime.Afternoon - m;
            if (m < tbTime.Evening)   return tbTime.Evening - m;
            if (m < tbTime.Night)     return tbTime.Night - m;
            return int.MaxValue;
        }

        private void CheckSeasonTransition()
        {
            var tbTime = GF.Config.GetTable<TbTime>();
            if ((_store.Day - 1) % tbTime.DaysPerSeason == 0 && _store.Day > 1)
            {
                var next = (ESeason)(((int)_store.CurrentSeason + 1) % 4);
                _store.SetSeason(next);
            }
        }
    }
}
