using System.Collections.Generic;

namespace CozyYard
{
    public class InventoryStore : SavableStoreBase<InventoryData>, IInventoryQueries
    {
        protected override string SaveKey => SaveKeys.InventoryData;

        public int Capacity => Data.Capacity;
        public int UsedSlots => Data.Items.Count;
        public int FreeSlots => Data.Capacity - Data.Items.Count;
        public int Coins => Data.Coins;
        public IReadOnlyList<ItemStack> Items => Data.Items;

        public int GetItemCount(int itemId)
        {
            var stack = FindStack(itemId);
            return stack?.Quantity ?? 0;
        }

        public bool HasItem(int itemId, int quantity = 1)
        {
            return GetItemCount(itemId) >= quantity;
        }

        public bool HasSpace(int itemId, int quantity = 1)
        {
            var existing = FindStack(itemId);
            if (existing != null) return true;
            return FreeSlots > 0;
        }

        public bool AddItem(int itemId, int quantity)
        {
            if (quantity <= 0) return false;

            var existing = FindStack(itemId);
            if (existing != null)
            {
                existing.Quantity += quantity;
                MarkDirty();
                return true;
            }

            if (FreeSlots <= 0) return false;

            Data.Items.Add(new ItemStack { ItemId = itemId, Quantity = quantity });
            MarkDirty();
            return true;
        }

        public bool RemoveItem(int itemId, int quantity)
        {
            if (quantity <= 0) return false;

            var existing = FindStack(itemId);
            if (existing == null || existing.Quantity < quantity) return false;

            existing.Quantity -= quantity;
            if (existing.Quantity <= 0)
            {
                Data.Items.Remove(existing);
            }
            MarkDirty();
            return true;
        }

        public void AddCoins(int amount)
        {
            Data.Coins += amount;
            MarkDirty();
        }

        public bool SpendCoins(int amount)
        {
            if (Data.Coins < amount) return false;
            Data.Coins -= amount;
            MarkDirty();
            return true;
        }

        public void SetCapacity(int capacity)
        {
            Data.Capacity = capacity;
            MarkDirty();
        }

        private ItemStack FindStack(int itemId)
        {
            for (int i = 0; i < Data.Items.Count; i++)
            {
                if (Data.Items[i].ItemId == itemId) return Data.Items[i];
            }
            return null;
        }
    }
}
