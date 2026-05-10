using UnityEngine;
using JulyArch;

namespace IsleWorks.Economy
{
    public class InventoryData
    {
        public int Gold;
        public int TotalProductionValue;
    }

    /// <summary>
    /// 库存存储，管理玩家金币与累计产值。
    /// </summary>
    public class InventoryStore : StoreBase<InventoryData>, IInventoryQueries
    {
        public int Gold => Data.Gold;
        public int TotalProductionValue => Data.TotalProductionValue;

        public void UpdateGold(int amount)
        {
            Data.Gold += amount;
            Debug.Log($"Gold updated by {amount}. Current: {Data.Gold}");
        }

        public void UpdateTotalProductionValue(int amount)
        {
            Data.TotalProductionValue += amount;
        }
    }
}
