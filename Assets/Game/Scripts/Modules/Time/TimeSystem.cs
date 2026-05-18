using System;
using JulyArch;

namespace SpiritHealer
{
    /// <summary>
    /// 时间系统 —— 行为驱动时间模型。
    /// 时间不自动流逝，由玩家行为（看诊、种植等）消耗游戏内分钟推进。
    /// 时段由当天时刻对照 TimeConfig 阈值自动推算。
    /// 每 DaysPerSeason 天轮转季节（春→夏→秋→冬）。
    /// </summary>
    public class TimeSystem : GameSystemBase
    {
        private TimeStore _store;

        public bool IsOpen => _store.IsOpen;
        public int MinuteOfDay => _store.MinuteOfDay;
        public TimePhase CurrentPhase => _store.CurrentPhase;

        protected override void OnInitialize()
        {
            _store = GetStore<TimeStore>();
        }

        /// <summary>
        /// 消耗游戏内时间。自动检测并逐个触发跨越的时段边界事件。
        /// </summary>
        public void ConsumeTime(int minutes)
        {
            var remaining = minutes;
            while (remaining > 0)
            {
                var oldPhase = _store.CurrentPhase;
                var toNext = GetMinutesToNextPhase();
                var step = Math.Min(remaining, toNext);
                _store.AddMinutes(step);
                remaining -= step;

                var newPhase = _store.CurrentPhase;
                if (oldPhase != newPhase)
                {
                    Publish(new PhaseChangedEvent { OldPhase = oldPhase, NewPhase = newPhase });
                }
            }
        }

        /// <summary>结束当天：触发夜间结算，然后推进到下一天早晨。</summary>
        public void EndDay()
        {
            var oldPhase = _store.CurrentPhase;
            if (oldPhase != TimePhase.Night)
            {
                _store.SetMinuteOfDay(TimeConfig.Night);
                this.Publish(new PhaseChangedEvent { OldPhase = oldPhase, NewPhase = TimePhase.Night });
            }

            _store.AdvanceDay();
            CheckSeasonTransition();
            _store.SetMinuteOfDay(TimeConfig.DayStart);

            this.Publish(new DayChangedEvent
            {
                NewDay = _store.Day,
                CurrentSeason = _store.CurrentSeason
            });

            this.Publish(new PhaseChangedEvent { OldPhase = TimePhase.Night, NewPhase = TimePhase.Morning });
        }

        private int GetMinutesToNextPhase()
        {
            var m = _store.MinuteOfDay;
            if (m < TimeConfig.Noon)      return TimeConfig.Noon - m;
            if (m < TimeConfig.Afternoon) return TimeConfig.Afternoon - m;
            if (m < TimeConfig.Evening)   return TimeConfig.Evening - m;
            if (m < TimeConfig.Night)     return TimeConfig.Night - m;
            return int.MaxValue;
        }

        private void CheckSeasonTransition()
        {
            if ((_store.Day - 1) % TimeConfig.DaysPerSeason == 0 && _store.Day > 1)
            {
                var next = (Season)(((int)_store.CurrentSeason + 1) % 4);
                _store.SetSeason(next);
            }
        }
    }
}
