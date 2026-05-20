namespace CozyYard
{
    public struct InventoryChangedEvent { }

    public struct BuildingPlacedEvent
    {
        public int BuildingId;
        public int GridX;
        public int GridY;
    }

    public struct BuildingRemovedEvent
    {
        public int GridX;
        public int GridY;
    }

    public struct CropHarvestedEvent
    {
        public int CropId;
        public int Quantity;
    }

    public struct OrderCompletedEvent
    {
        public int VisitorId;
        public int RewardCoins;
    }

    public struct MilestoneAchievedEvent
    {
        public int MilestoneId;
    }

    public struct GridCellChangedEvent
    {
        public int GridX;
        public int GridY;
        public CellState NewState;
    }

    public struct WeatherChangedEvent
    {
        public WeatherType NewWeather;
        public WeatherType PreviousWeather;
    }
}
