using cfg;
using JulyArch;
using JulyCore;

namespace CozyYard
{
    /// <summary>天气系统：每日根据季节概率随机天气，影响作物浇水和来客到访概率。</summary>
    public class WeatherSystem : GameSystemBase
    {
        private WeatherStore _store;
        private TimeStore _timeStore;

        protected override void OnInitialize()
        {
            _store = GetStore<WeatherStore>();
            _timeStore = GetStore<TimeStore>();

            this.Subscribe<DayChangedEvent>(OnDayChanged);
        }

        protected override void OnStart()
        {
            if (_store.CurrentWeather == WeatherType.Sunny && _store.ConsecutiveRainDays == 0)
                RollDailyWeather();
        }

        public WeatherType GetCurrentWeather() => _store.CurrentWeather;
        public bool IsRaining() => _store.IsRaining;

        public void RollDailyWeather()
        {
            var previousWeather = _store.CurrentWeather;
            var season = _timeStore.CurrentSeason;

            int[] weights = GetSeasonWeights(season);

            var rng = new System.Random();
            int total = 0;
            for (int i = 0; i < weights.Length; i++) total += weights[i];

            int roll = rng.Next(total);
            int cumulative = 0;
            WeatherType newWeather = WeatherType.Sunny;

            for (int i = 0; i < weights.Length; i++)
            {
                cumulative += weights[i];
                if (roll < cumulative)
                {
                    newWeather = (WeatherType)i;
                    break;
                }
            }

            _store.SetWeather(newWeather);
            Publish(new WeatherChangedEvent { NewWeather = newWeather, PreviousWeather = previousWeather });
        }

        private static int[] GetSeasonWeights(Season season)
        {
            var seasonId = (int)season + 1;
            var cfg = GF.Config.GetTable<TbWeather>()?.GetOrDefault(seasonId);
            if (cfg != null)
                return new[] { cfg.Sunny, cfg.Cloudy, cfg.LightRain, cfg.HeavyRain, cfg.Windy };

            return new[] { 30, 25, 25, 10, 10 };
        }

        /// <summary>
        /// Get visitor chance modifier based on weather.
        /// Returns a multiplier (e.g., 0.5 for heavy rain).
        /// </summary>
        public float GetVisitorChanceModifier()
        {
            return _store.CurrentWeather switch
            {
                WeatherType.HeavyRain => 0.5f,
                WeatherType.Windy => 0.8f,
                _ => 1.0f
            };
        }

        private void OnDayChanged(DayChangedEvent e)
        {
            RollDailyWeather();
        }
    }
}
