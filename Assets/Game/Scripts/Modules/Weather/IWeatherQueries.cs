using JulyArch;

namespace CozyYard
{
    public interface IWeatherQueries : IStoreQueries
    {
        WeatherType CurrentWeather { get; }
        bool IsRaining { get; }
        int ConsecutiveRainDays { get; }
    }
}
