using cfg;
using JulyArch;
using JulyCore;

namespace CozyYard
{
    public struct ShopPurchaseEvent
    {
        public int ShopItemId;
        public int ItemId;
        public int Price;
    }

    public struct ShopPurchaseFailedEvent
    {
        public int ShopItemId;
        public PurchaseFailReason Reason;
    }

    public enum PurchaseFailReason
    {
        NotEnoughCoins,
        InventoryFull
    }

    /// <summary>商店系统：处理购买逻辑，扣钱发物品。</summary>
    public class ShopSystem : GameSystemBase
    {
        private InventorySystem _inventorySystem;

        protected override void OnInitialize()
        {
            _inventorySystem = GetSystem<InventorySystem>();
        }

        public bool TryPurchase(int shopItemId)
        {
            var tbShop = GF.Config.GetTable<TbShop>();
            var shopItem = tbShop?.GetOrDefault(shopItemId);
            if (shopItem == null) return false;

            if (!_inventorySystem.SpendCoins(shopItem.Price))
            {
                Publish(new ShopPurchaseFailedEvent { ShopItemId = shopItemId, Reason = PurchaseFailReason.NotEnoughCoins });
                return false;
            }

            if (!_inventorySystem.AddItem(shopItem.ItemId, 1))
            {
                _inventorySystem.AddCoins(shopItem.Price);
                Publish(new ShopPurchaseFailedEvent { ShopItemId = shopItemId, Reason = PurchaseFailReason.InventoryFull });
                return false;
            }

            Publish(new ShopPurchaseEvent { ShopItemId = shopItemId, ItemId = shopItem.ItemId, Price = shopItem.Price });
            return true;
        }
    }
}
