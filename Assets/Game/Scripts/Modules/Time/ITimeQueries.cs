using JulyArch;

namespace CozyYard
{
    public interface ITimeQueries : IStoreQueries
    {
        int Day { get; }
        int MinuteOfDay { get; }
        int Hour { get; }
        int Minute { get; }
        Season CurrentSeason { get; }
        TimePhase CurrentPhase { get; }
        int Year { get; }
        int DayInSeason { get; }
        bool IsNight { get; }
    }
}
