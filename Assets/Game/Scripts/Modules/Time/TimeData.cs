using System;
using JulyCore.Data.Save;

namespace CozyYard
{
    [Serializable]
    public class TimeData : ISaveData
    {
        public int Day;
        public int MinuteOfDay;
        public int SeasonIndex;
        public int Year;
        public int DayInSeason;

        public SaveImportance Importance => SaveImportance.Normal;
    }
}
