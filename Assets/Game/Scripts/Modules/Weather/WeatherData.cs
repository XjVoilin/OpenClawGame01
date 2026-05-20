using System;
using JulyCore.Data.Save;

namespace CozyYard
{
    [Serializable]
    public class WeatherData : ISaveData
    {
        public int CurrentWeather; // WeatherType as int
        public int ConsecutiveRainDays;

        public SaveImportance Importance => SaveImportance.Normal;
    }
}
