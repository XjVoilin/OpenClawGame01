using cfg;
using JulyArch;
using UnityEngine;

namespace CozyYard
{
    public class TimeSystem : GameSystemBase, IUpdatableSystem
    {
        private TimeStore _store;

        private float _timeScale = 1f;
        private float _accumulatedRealTime;
        private bool _paused;

        private const float BaseGameMinutesPerRealSecond = 0.8f;
        private const int DayStartMinute = 360;
        private const int DayEndMinute = 1440;

        public float TimeScale
        {
            get => _timeScale;
            set => _timeScale = Mathf.Clamp(value, 0f, 3f);
        }

        public bool IsPaused
        {
            get => _paused;
            set => _paused = value;
        }

        protected override void OnInitialize()
        {
            _store = GetStore<TimeStore>();
        }

        public void OnUpdate(float deltaTime)
        {
            if (_paused) return;

            _accumulatedRealTime += deltaTime * _timeScale;

            float minutesToAdd = _accumulatedRealTime * BaseGameMinutesPerRealSecond;
            if (minutesToAdd >= 1f)
            {
                int wholeMinutes = Mathf.FloorToInt(minutesToAdd);
                _accumulatedRealTime -= wholeMinutes / BaseGameMinutesPerRealSecond;
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
            TimeScale = Mathf.Clamp(multiplier, 1, 3);
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
            if (_store.MinuteOfDay < DayStartMinute)
            {
                _store.SetMinuteOfDay(DayStartMinute);
            }
        }

        private void AdvanceTime(int minutes)
        {
            var oldPhase = _store.CurrentPhase;
            _store.AddMinutes(minutes);

            if (_store.MinuteOfDay >= DayEndMinute)
            {
                _store.SetMinuteOfDay(DayEndMinute);
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
            _store.SetMinuteOfDay(DayStartMinute);
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
        }

        private int GetSeasonDays(Season season)
        {
            var cfg = CfgTable.Tables?.TbSeason.GetOrDefault((int)season);
            return cfg?.Days ?? 15;
        }
    }
}
