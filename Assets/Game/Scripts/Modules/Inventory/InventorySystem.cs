using cfg;
using JulyArch;
using JulyCore;

namespace CozyYard
{
    /// <summary>背包系统：管理玩家物品的增删、堆叠、金币收支和容量扩展。</summary>
    public class InventorySystem : GameSystemBase
    {
        private InventoryStore _store;

        protected override void OnInitialize()
        {
            _store = GetStore<InventoryStore>();
            this.Subscribe<DiscardItemEvent>(OnDiscardItem);
            this.Subscribe<UseItemEvent>(OnUseItem);
        }

        private void OnDiscardItem(DiscardItemEvent e)
        {
            RemoveItem(e.ItemId, e.Quantity);
        }

        private void OnUseItem(UseItemEvent e)
        {
            var itemCfg = GF.Config.GetTable<TbItem>()?.GetOrDefault(e.ItemId);
            if (itemCfg == null) return;

            if (itemCfg.Type == "Seed")
            {
                var tbCrop = GF.Config.GetTable<TbCrop>();
                if (tbCrop == null) return;
                foreach (var crop in tbCrop.DataList)
                {
                    if (crop.SeedItemId == e.ItemId)
                    {
                        Publish(new EnterPlantingModeEvent { SeedItemId = e.ItemId, CropId = crop.Id });
                        return;
                    }
                }
            }
        }

        protected override void OnStart() { }

        public bool AddItem(int itemId, int quantity = 1)
        {
            if (!_store.HasSpace(itemId, quantity)) return false;

            bool success = _store.AddItem(itemId, quantity);
            if (success)
            {
                Publish(new InventoryChangedEvent());
            }
            return success;
        }

        public bool RemoveItem(int itemId, int quantity = 1)
        {
            bool success = _store.RemoveItem(itemId, quantity);
            if (success)
            {
                Publish(new InventoryChangedEvent());
            }
            return success;
        }

        public bool HasItem(int itemId, int quantity = 1)
        {
            return _store.HasItem(itemId, quantity);
        }

        public int GetItemCount(int itemId)
        {
            return _store.GetItemCount(itemId);
        }

        public void AddCoins(int amount)
        {
            _store.AddCoins(amount);
            Publish(new InventoryChangedEvent());
        }

        public bool SpendCoins(int amount)
        {
            bool success = _store.SpendCoins(amount);
            if (success)
            {
                Publish(new InventoryChangedEvent());
            }
            return success;
        }

        public void ExpandCapacity(int additionalSlots)
        {
            _store.SetCapacity(_store.Capacity + additionalSlots);
            Publish(new InventoryChangedEvent());
        }

        public bool ConsumeItems(int[] itemIds, int[] quantities)
        {
            if (itemIds.Length != quantities.Length) return false;

            for (int i = 0; i < itemIds.Length; i++)
            {
                if (!_store.HasItem(itemIds[i], quantities[i])) return false;
            }

            for (int i = 0; i < itemIds.Length; i++)
            {
                _store.RemoveItem(itemIds[i], quantities[i]);
            }

            Publish(new InventoryChangedEvent());
            return true;
        }
    }
}
