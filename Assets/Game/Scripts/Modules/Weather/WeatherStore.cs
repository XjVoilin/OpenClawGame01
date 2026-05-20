namespace CozyYard
{
    public class WeatherStore : SavableStoreBase<WeatherData>
    {
        protected override string SaveKey => SaveKeys.WeatherData;

        public WeatherType CurrentWeather => (WeatherType)Data.CurrentWeather;
        public bool IsRaining => CurrentWeather == WeatherType.LightRain || CurrentWeather == WeatherType.HeavyRain;
        public int ConsecutiveRainDays => Data.ConsecutiveRainDays;

        public void SetWeather(WeatherType weather)
        {
            Data.CurrentWeather = (int)weather;
            if (IsRaining)
                Data.ConsecutiveRainDays++;
            else
                Data.ConsecutiveRainDays = 0;
            MarkDirty();
        }
    }
}
