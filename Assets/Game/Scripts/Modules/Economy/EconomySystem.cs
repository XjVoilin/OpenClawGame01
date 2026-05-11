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
                var config = ResourceConfigLoader.GetConfig((int)products[i]);
                if (config != null)
                {
                    totalRevenue += config.SellPrice;
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

            // Check milestone after selling
            this.GetSystem<TechSystem>().CheckMilestone();
        }
    }
}
