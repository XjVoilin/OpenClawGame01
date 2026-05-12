using IsleWorks.Production;
using IsleWorks.Tech;
using JulyArch;
using JulyCore;

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

            for (int i = 0; i < products.Length; i++)
            {
                var res = CfgTable.Resource.GetOrDefault((int)products[i]);
                if (res != null)
                {
                    totalRevenue += res.SellPrice;
                }
            }

            this.Mutate<InventoryStore>(store =>
            {
                store.UpdateGold(totalRevenue);
                store.UpdateTotalProductionValue(totalRevenue);
            });

            var inv = this.Query<IInventoryQueries>();
            this.Publish(new GoldChangedEvent(inv.Gold));

            GF.Log($"Sold {products.Length} products at port for {totalRevenue} gold. Total value: {inv.TotalProductionValue}");

            this.GetSystem<TechSystem>().CheckMilestone();
        }

        /// <summary>
        /// 卖出港口中所有产品并清空港口库存。
        /// </summary>
        public void SellAllPortProducts()
        {
            var inv = this.Query<IInventoryQueries>();
            if (inv.PortProducts.Count == 0) return;

            var products = new ResourceType[inv.PortProducts.Count];
            for (int i = 0; i < inv.PortProducts.Count; i++)
                products[i] = inv.PortProducts[i];

            SellAtPort(products);

            this.Mutate<InventoryStore>(store => store.ClearPortProducts());
        }
    }
}
