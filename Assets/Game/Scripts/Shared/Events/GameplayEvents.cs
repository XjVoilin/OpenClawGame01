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
        public int GridX;
        public int GridY;
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

    /// <summary>请求进入建筑放置模式</summary>
    public struct EnterPlacementModeEvent
    {
        public int BuildingId;
    }

    /// <summary>放置模式被取消</summary>
    public struct PlacementCancelledEvent { }

    /// <summary>请求使用物品</summary>
    public struct UseItemEvent
    {
        public int ItemId;
    }

    /// <summary>请求进入种植模式（选好种子后）</summary>
    public struct EnterPlantingModeEvent
    {
        public int SeedItemId;
        public int CropId;
    }

    /// <summary>请求丢弃物品</summary>
    public struct DiscardItemEvent
    {
        public int ItemId;
        public int Quantity;
    }
}
