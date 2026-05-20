using JulyArch;

namespace CozyYard
{
    public class WeatherSystem : GameSystemBase
    {
        private WeatherStore _store;
        private TimeStore _timeStore;

        // Season weather probabilities [sunny, cloudy, lightRain, heavyRain, windy]
        // Will be replaced by Luban TbWeather when available
        private static readonly int[][] SeasonWeights = {
            new[] { 30, 30, 25, 10, 5 },  // Spring
            new[] { 45, 20, 15, 10, 10 }, // Summer
            new[] { 20, 25, 30, 15, 10 }, // Autumn
            new[] { 25, 35, 15, 5, 20 },  // Winter
        };

        protected override void OnInitialize()
        {
            _store = GetStore<WeatherStore>();
            _timeStore = GetStore<TimeStore>();

            this.Subscribe<DayChangedEvent>(OnDayChanged);
        }

        public WeatherType GetCurrentWeather() => _store.CurrentWeather;
        public bool IsRaining() => _store.IsRaining;

        /// <summary>
        /// Roll new weather for the day based on current season.
        /// Called at start of each new day.
        /// </summary>
        public void RollDailyWeather()
        {
            var previousWeather = _store.CurrentWeather;
            var season = _timeStore.CurrentSeason;
            var weights = SeasonWeights[(int)season];

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
