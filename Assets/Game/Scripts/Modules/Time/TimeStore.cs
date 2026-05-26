namespace CozyYard
{
    public class TimeStore : SavableStoreBase<TimeData>
    {
        protected override string SaveKey => SaveKeys.TimeData;

        public int Day => Data.Day;
        public int MinuteOfDay => Data.MinuteOfDay;
        public int Hour => Data.MinuteOfDay / 60;
        public int Minute => Data.MinuteOfDay % 60;
        public Season CurrentSeason => (Season)Data.SeasonIndex;
        public int Year => Data.Year;
        public int DayInSeason => Data.DayInSeason;
        public bool IsNight => CurrentPhase == TimePhase.Night;

        public TimePhase CurrentPhase => GetPhaseForMinute(Data.MinuteOfDay);

        public void AddMinutes(int minutes)
        {
            Data.MinuteOfDay += minutes;
            MarkDirty();
        }

        public void SetMinuteOfDay(int minute)
        {
            Data.MinuteOfDay = minute;
            MarkDirty();
        }

        public void AdvanceDay()
        {
            Data.Day++;
            MarkDirty();
        }

        public void SetSeason(Season season)
        {
            Data.SeasonIndex = (int)season;
            MarkDirty();
        }

        public void SetDayInSeason(int day)
        {
            Data.DayInSeason = day;
            MarkDirty();
        }

        public void SetInitialTime(int seasonIndex, int minuteOfDay, int year, int dayInSeason)
        {
            Data.SeasonIndex = seasonIndex;
            Data.MinuteOfDay = minuteOfDay;
            Data.Year = year;
            Data.Day = 1;
            Data.DayInSeason = dayInSeason;
            MarkDirty();
        }

        public void AdvanceYear()
        {
            Data.Year++;
            MarkDirty();
        }

        private static TimePhase GetPhaseForMinute(int minute)
        {
            if (minute < 360) return TimePhase.Night;
            if (minute < 480) return TimePhase.Dawn;
            if (minute < 720) return TimePhase.Morning;
            if (minute < 840) return TimePhase.Noon;
            if (minute < 1080) return TimePhase.Afternoon;
            if (minute < 1260) return TimePhase.Evening;
            return TimePhase.Night;
        }
    }
}
