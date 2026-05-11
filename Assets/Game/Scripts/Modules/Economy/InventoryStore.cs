using System.Collections.Generic;
using IsleWorks.Production;
using JulyArch;
using JulyCore;

namespace IsleWorks.Economy
{
    public class InventoryData
    {
        public int Gold;
        public int TotalProductionValue;
        public List<ResourceType> PortProducts;
    }

    /// <summary>
    /// 库存存储，管理玩家金币与累计产值。
    /// </summary>
    public class InventoryStore : StoreBase<InventoryData>, IInventoryQueries
    {
        public int Gold => Data.Gold;
        public int TotalProductionValue => Data.TotalProductionValue;
        public IReadOnlyList<ResourceType> PortProducts => Data.PortProducts;

        protected override InventoryData LoadData()
        {
            return new InventoryData
            {
                Gold = 500,
                TotalProductionValue = 0,
                PortProducts = new List<ResourceType>()
            };
        }

        public void UpdateGold(int amount)
        {
            Data.Gold += amount;
            GF.Log($"Gold updated by {amount}. Current: {Data.Gold}");
        }

        public void UpdateTotalProductionValue(int amount)
        {
            Data.TotalProductionValue += amount;
        }

        public void AddPortProduct(ResourceType item)
        {
            Data.PortProducts.Add(item);
        }

        public void ClearPortProducts()
        {
            Data.PortProducts.Clear();
        }
    }
}
