using UnityEngine;
using IsleWorks.Data;
using JulyArch;

namespace IsleWorks.Systems
{
    /// <summary>
    /// 经济系统，负责产品销售和金币管理。
    /// </summary>
    public class EconomySystem : GameSystemBase
    {
        [Inject] private InventoryStore _inventoryStore;

        /// <summary>
        /// 港口产品销售。
        /// </summary>
        public void SellAtPort(ResourceType[] products)
        {
            int totalRevenue = 0;

            foreach (var product in products)
            {
                totalRevenue += GetSellPrice(product);
            }

            // 更新玩家金币和累计产值
            _inventoryStore.UpdateGold(totalRevenue);
            _inventoryStore.UpdateTotalProductionValue(totalRevenue);

            Debug.Log($"Sold products at port for {totalRevenue} gold.");
        }

        /// <summary>
        /// 获取产品售价。
        /// </summary>
        private int GetSellPrice(ResourceType product)
        {
            // TODO: 查询资源表获取售价
            return 50; // 示例售价
        }
    }
}