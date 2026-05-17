using JulyArch;

namespace SpiritHealer
{
    public class TimeData
    {
        public int Day;
        public Season CurrentSeason;
        public TimePhase CurrentPhase;
    }

    public class TimeStore : StoreBase<TimeData>
    {
        public int Day => Data.Day;
        public Season CurrentSeason => Data.CurrentSeason;
        public TimePhase CurrentPhase => Data.CurrentPhase;
        
        public void AdvanceDay() => Data.Day++;
        public void SetSeason(Season season) => Data.CurrentSeason = season;
        public void SetPhase(TimePhase phase) => Data.CurrentPhase = phase;
    }
}