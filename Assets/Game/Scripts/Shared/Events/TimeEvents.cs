namespace SpiritHealer
{
    public struct PhaseChangedEvent
    {
        public TimePhase OldPhase;
        public TimePhase NewPhase;
    }

    public struct DayChangedEvent
    {
        public int NewDay;
        public Season CurrentSeason;
    }
}
