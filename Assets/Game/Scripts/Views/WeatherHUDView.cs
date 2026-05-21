using JulyCore;
using TMPro;
using UnityEngine;

namespace CozyYard
{
    public class WeatherHUDView : GameUIView
    {
        [SerializeField] private TextMeshProUGUI _weatherText;
        [SerializeField] private TextMeshProUGUI _weatherIcon;

        protected override void OnViewEnable()
        {
            Subscribe<WeatherChangedEvent>(OnWeatherChanged);
            Refresh();
        }

        private void OnWeatherChanged(WeatherChangedEvent e) => Refresh();

        private void Refresh()
        {
            var weather = GetSystem<WeatherSystem>().GetCurrentWeather();
            if (_weatherText) _weatherText.text = GetWeatherName(weather);
            if (_weatherIcon) _weatherIcon.text = GetWeatherIcon(weather);
        }

        private static string GetWeatherName(WeatherType w) => w switch
        {
            WeatherType.Sunny => GF.Localization.Get("weather_sunny"),
            WeatherType.Cloudy => GF.Localization.Get("weather_cloudy"),
            WeatherType.LightRain => GF.Localization.Get("weather_light_rain"),
            WeatherType.HeavyRain => GF.Localization.Get("weather_heavy_rain"),
            WeatherType.Windy => GF.Localization.Get("weather_windy"),
            _ => "?"
        };

        private static string GetWeatherIcon(WeatherType w) => w switch
        {
            WeatherType.Sunny => "☀",
            WeatherType.Cloudy => "☁",
            WeatherType.LightRain => "🌧",
            WeatherType.HeavyRain => "⛈",
            WeatherType.Windy => "💨",
            _ => "?"
        };
    }
}
