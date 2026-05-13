namespace OffTrail.World
{
    public struct DayChanged
    {
        public int NewDay;
    }

    public struct SeasonChanged
    {
        public int NewSeason;
        public string SeasonName;
    }

    public struct NightFell
    {
    }

    public struct DawnBroke
    {
    }

    public struct RegionDiscovered
    {
        public int RegionId;
        public string RegionName;
    }

    public struct StationBuilt
    {
        public int StationId;
        public string StationName;
    }
}
