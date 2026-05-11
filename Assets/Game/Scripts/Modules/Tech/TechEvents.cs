namespace IsleWorks.Tech
{
    public readonly struct EraChangedEvent
    {
        public readonly int NewEra;

        public EraChangedEvent(int newEra)
        {
            NewEra = newEra;
        }
    }
}
