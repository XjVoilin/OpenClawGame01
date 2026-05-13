namespace OffTrail.Inventory
{
    public struct ItemPickedUp
    {
        public int ItemId;
        public int Count;
    }

    public struct ItemRemoved
    {
        public int ItemId;
        public int Count;
    }

    public struct ItemUsed
    {
        public int ItemId;
    }

    public struct InventoryChanged
    {
    }
}
