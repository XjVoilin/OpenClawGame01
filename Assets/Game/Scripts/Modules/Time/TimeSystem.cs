using cfg;
using Cysharp.Threading.Tasks;
using JulyArch;
using JulyCore;
using JulyCore.Data.Save;
using UnityEngine;

namespace CozyYard
{
    /// <summary>时间系统：驱动游戏内时间流逝、日夜交替、季节轮换，支持加速和行动消耗时间。</summary>
    public class TimeSystem : GameSystemBase, IUpdatableSystem
    {
        private TimeStore _store;

        private float _timeScale = 1f;
        private float _accumulatedRealTime;
        private bool _paused;

        private float _gameMinutesPerRealSecond;
        private int _dayStartMinute;
        private int _dayEndMinute;
        private float _maxTimeScale;

        public float TimeScale
        {
            get => _timeScale;
            set => _timeScale = Mathf.Clamp(value, 0f, _maxTimeScale);
        }

        public bool IsPaused
        {
            get => _paused;
            set => _paused = value;
        }

        protected override void OnInitialize()
        {
            _store = GetStore<TimeStore>();

            var cfg = GF.Config.GetTable<TbGameConfig>();
            _gameMinutesPerRealSecond = cfg?.GameMinutesPerRealSecond ?? 0.8f;
            _dayStartMinute = cfg?.DayStartMinute ?? 360;
            _dayEndMinute = cfg?.DayEndMinute ?? 1440;
            _maxTimeScale = cfg?.MaxTimeScale ?? 3f;
        }

        protected override void OnStart()
        {
            EnsureDayStarted();
        }

        public void OnUpdate(float deltaTime)
        {
            if (_paused) return;

            _accumulatedRealTime += deltaTime * _timeScale;

            float minutesToAdd = _accumulatedRealTime * _gameMinutesPerRealSecond;
            if (minutesToAdd >= 1f)
            {
                int wholeMinutes = Mathf.FloorToInt(minutesToAdd);
                _accumulatedRealTime -= wholeMinutes / _gameMinutesPerRealSecond;
                AdvanceTime(wholeMinutes);
            }
        }

        public void ConsumeTime(int minutes)
        {
            if (minutes <= 0) return;
            AdvanceTime(minutes);
        }

        public void SetSpeed(int multiplier)
        {
            TimeScale = Mathf.Clamp(multiplier, 1, (int)_maxTimeScale);
        }

        public void EndDay()
        {
            var oldPhase = _store.CurrentPhase;
            if (oldPhase != TimePhase.Night)
            {
                _store.SetMinuteOfDay(1260);
                Publish(new PhaseChangedEvent { OldPhase = oldPhase, NewPhase = TimePhase.Night });
            }
            PerformDaySettlement();
        }

        public void EnsureDayStarted()
        {
            if (_store.MinuteOfDay < _dayStartMinute)
            {
                _store.SetMinuteOfDay(_dayStartMinute);
            }
        }

        private void AdvanceTime(int minutes)
        {
            var oldPhase = _store.CurrentPhase;
            _store.AddMinutes(minutes);

            if (_store.MinuteOfDay >= _dayEndMinute)
            {
                _store.SetMinuteOfDay(_dayEndMinute);
                var newPhase = _store.CurrentPhase;
                if (oldPhase != newPhase)
                {
                    Publish(new PhaseChangedEvent { OldPhase = oldPhase, NewPhase = newPhase });
                }
                PerformDaySettlement();
                return;
            }

            var currentPhase = _store.CurrentPhase;
            if (oldPhase != currentPhase)
            {
                Publish(new PhaseChangedEvent { OldPhase = oldPhase, NewPhase = currentPhase });
            }
        }

        private void PerformDaySettlement()
        {
            _store.AdvanceDay();

            int dayInSeason = _store.DayInSeason + 1;
            int currentSeasonDays = GetSeasonDays(_store.CurrentSeason);

            if (dayInSeason > currentSeasonDays)
            {
                dayInSeason = 1;
                var oldSeason = _store.CurrentSeason;
                int nextSeasonIndex = ((int)oldSeason + 1) % 4;
                _store.SetSeason((Season)nextSeasonIndex);

                if (nextSeasonIndex == 0)
                {
                    _store.AdvanceYear();
                }

                Publish(new SeasonChangedEvent
                {
                    OldSeason = oldSeason,
                    NewSeason = _store.CurrentSeason
                });
            }

            _store.SetDayInSeason(dayInSeason);
            _store.SetMinuteOfDay(_dayStartMinute);
            _accumulatedRealTime = 0f;

            Publish(new DayChangedEvent
            {
                NewDay = _store.Day,
                CurrentSeason = _store.CurrentSeason
            });

            Publish(new PhaseChangedEvent
            {
                OldPhase = TimePhase.Night,
                NewPhase = TimePhase.Dawn
            });

            GF.Save.TriggerSaveAsync(SaveSignal.High).Forget();
        }

        private int GetSeasonDays(Season season)
        {
            var cfg = GF.Config.GetTable<TbSeason>()?.GetOrDefault((int)season);
            return cfg?.Days ?? 15;
        }
    }
}
