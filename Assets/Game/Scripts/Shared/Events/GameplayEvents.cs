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
        public int OrderId;
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
}
