using JulyArch;
using TMPro;
using UnityEngine;

namespace CozyYard
{
    public class WeatherHUDView : GameView
    {
        [SerializeField] private TextMeshProUGUI _weatherText;
        [SerializeField] private TextMeshProUGUI _weatherIcon;

        public override IGameContext GetArchitecture() => AppArch.Context;

        protected override void OnViewEnable()
        {
            this.Subscribe<WeatherChangedEvent>(OnWeatherChanged);
            Refresh();
        }

        private void OnWeatherChanged(WeatherChangedEvent e) => Refresh();

        private void Refresh()
        {
            var weather = this.GetSystem<WeatherSystem>().GetCurrentWeather();
            if (_weatherText) _weatherText.text = GetWeatherName(weather);
            if (_weatherIcon) _weatherIcon.text = GetWeatherIcon(weather);
        }

        private static string GetWeatherName(WeatherType w) => w switch
        {
            WeatherType.Sunny => "晴天",
            WeatherType.Cloudy => "多云",
            WeatherType.LightRain => "小雨",
            WeatherType.HeavyRain => "大雨",
            WeatherType.Windy => "大风",
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
