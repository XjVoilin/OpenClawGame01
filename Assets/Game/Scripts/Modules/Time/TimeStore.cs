using System;
using JulyCore.Data.Save;

namespace SpiritHealer
{
    [Serializable]
    public class TimeData : ISaveData
    {
        public int Day = 1;
        public int MinuteOfDay = TimeConfig.DayStart;
        public Season CurrentSeason;

        public SaveImportance Importance => SaveImportance.Normal;
    }

    public class TimeStore : SavableStoreBase<TimeData>
    {
        protected override string SaveKey => SaveKeys.TimeDataKey;

        public int Day => Data.Day;
        public int MinuteOfDay => Data.MinuteOfDay;
        public Season CurrentSeason => Data.CurrentSeason;

        public TimePhase CurrentPhase => MinuteOfDay switch
        {
            < TimeConfig.Noon => TimePhase.Morning,
            < TimeConfig.Afternoon => TimePhase.Noon,
            < TimeConfig.Evening => TimePhase.Afternoon,
            < TimeConfig.Night => TimePhase.Evening,
            _ => TimePhase.Night
        };

        public bool IsOpen => MinuteOfDay >= TimeConfig.DayStart
                              && MinuteOfDay < TimeConfig.CloseTime;

        public int Hour => MinuteOfDay / 60;
        public int Minute => MinuteOfDay % 60;

        public void AddMinutes(int minutes)
        {
            Data.MinuteOfDay += minutes;
            MarkDirty();
        }

        public void SetMinuteOfDay(int m)
        {
            Data.MinuteOfDay = m;
            MarkDirty();
        }

        public void AdvanceDay()
        {
            Data.Day++;
            MarkDirty();
        }

        public void SetSeason(Season s)
        {
            Data.CurrentSeason = s;
            MarkDirty();
        }
    }
}