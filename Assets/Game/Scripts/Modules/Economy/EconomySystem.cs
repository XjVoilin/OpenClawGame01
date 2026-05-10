using UnityEngine;
using IsleWorks.Production;
using JulyArch;

namespace IsleWorks.Economy
{
    /// <summary>
    /// 经济系统，负责产品销售和金币管理。
    /// </summary>
    public class EconomySystem : GameSystemBase
    {
        public void SellAtPort(ResourceType[] products)
        {
            int totalRevenue = 0;
            int totalCost = 0;

            foreach (var product in products)
            {
                var config = ResourceConfigLoader.GetConfig((int)product);
                if (config != null)
                {
                    totalRevenue += config.SellPrice;
                    totalCost += CalculateProductionCost(product);
                }
            }

            this.Mutate<InventoryStore>(store =>
            {
                store.UpdateGold(totalRevenue);
                store.UpdateTotalProductionValue(totalRevenue);
            });

            int profit = totalRevenue - totalCost;
            Debug.Log($"Sold products at port for {totalRevenue} gold. Production cost: {totalCost}, Profit: {profit}");
        }

        private int CalculateProductionCost(ResourceType product)
        {
            // TODO: 根据产品的输入资源计算生产成本
            return 10;
        }
    }
}
