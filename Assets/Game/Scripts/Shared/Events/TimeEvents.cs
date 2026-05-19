using cfg;

namespace SpiritHealer
{
    public struct PhaseChangedEvent
    {
        public ETimePhase OldPhase;
        public ETimePhase NewPhase;
    }

    public struct DayChangedEvent
    {
        public int NewDay;
        public ESeason CurrentSeason;
    }
}
