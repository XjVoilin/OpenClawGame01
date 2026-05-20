namespace CozyYard
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

    public struct SeasonChangedEvent
    {
        public Season OldSeason;
        public Season NewSeason;
    }
}
