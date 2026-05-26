using System;
using System.Collections.Generic;
using JulyCore.Data.Save;

namespace CozyYard
{
    [Serializable]
    public class ItemStack
    {
        public int ItemId;
        public int Quantity;
    }

    [Serializable]
    public class InventoryData : ISaveData
    {
        public int Capacity;
        public List<ItemStack> Items = new();
        public int Coins;

        public SaveImportance Importance => SaveImportance.Normal;
    }
}
