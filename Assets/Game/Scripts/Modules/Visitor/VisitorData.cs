using System;
using System.Collections.Generic;
using JulyCore.Data.Save;

namespace CozyYard
{
    [Serializable]
    public class ActiveOrder
    {
        public int VisitorId;
        public int RequestedItemId;
        public int RequestedQuantity;
        public int RewardCoins;
        public int RewardItemId;
        public int RewardItemQty;
    }

    [Serializable]
    public class VisitorData : ISaveData
    {
        public List<ActiveOrder> TodayOrders = new();
        public bool GateOpen = true;

        public SaveImportance Importance => SaveImportance.Normal;
    }
}
