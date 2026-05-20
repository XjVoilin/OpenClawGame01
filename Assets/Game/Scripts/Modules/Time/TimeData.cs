using System;
using JulyCore.Data.Save;

namespace CozyYard
{
    [Serializable]
    public class TimeData : ISaveData
    {
        public int Day = 1;
        public int MinuteOfDay = 360;
        public int SeasonIndex = 2;
        public int Year = 1;
        public int DayInSeason = 1;

        public SaveImportance Importance => SaveImportance.Normal;
    }
}
