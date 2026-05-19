using System;
using cfg;
using JulyCore;
using JulyCore.Data.Save;

namespace SpiritHealer
{
    [Serializable]
    public class TimeData : ISaveData
    {
        public int Day = 1;
        public int MinuteOfDay;
        public ESeason CurrentSeason;

        public SaveImportance Importance => SaveImportance.Normal;
    }

    public class TimeStore : SavableStoreBase<TimeData>
    {
        protected override string SaveKey => SaveKeys.TimeDataKey;

        public int Day => Data.Day;
        public int MinuteOfDay => Data.MinuteOfDay;
        public ESeason CurrentSeason => Data.CurrentSeason;

        public ETimePhase CurrentPhase
        {
            get
            {
                var tbTime = GF.Config.GetTable<TbTime>();
                var result = ETimePhase.Night;
                if (MinuteOfDay < tbTime.Noon)
                {
                    result = ETimePhase.Morning;
                }else if (MinuteOfDay < tbTime.Afternoon)
                {
                    result = ETimePhase.Noon;
                }else if (MinuteOfDay < tbTime.Evening)
                {
                    result = ETimePhase.Afternoon;
                }else if (MinuteOfDay < tbTime.Night)
                {
                    result = ETimePhase.Evening;
                }
                return result;
            }
        }

        public bool IsOpen
        {
            get
            {
                var tbTime = GF.Config.GetTable<TbTime>();
                return MinuteOfDay >= tbTime.DayStart
                    && MinuteOfDay < tbTime.CloseTime;
            }
        }

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

        public void SetSeason(ESeason s)
        {
            Data.CurrentSeason = s;
            MarkDirty();
        }
    }
}