using System;
using System.Collections.Generic;
using JulyArch;

namespace OffTrail.Inventory
{
    public struct ItemSlot
    {
        public int ItemId;
        public int Count;
        public int Durability;
    }

    public sealed class InventoryData
    {
        public readonly List<ItemSlot> Slots = new();
    }

    public interface IInventoryQueries : IStoreQueries
    {
        int SlotCount { get; }
        int GetItemCount(int itemId);
        bool HasItems(int itemId, int count);
        List<int> GetAllUniqueItemIds();
    }

    public sealed class InventoryStore : StoreBase<InventoryData>, IInventoryQueries
    {
        public const int SlotCapacity = 20;

        public int SlotCount => Data.Slots.Count;

        protected override InventoryData LoadData()
        {
            var d = new InventoryData();
            for (var i = 0; i < SlotCapacity; i++)
                d.Slots.Add(default);
            return d;
        }

        public int GetItemCount(int itemId)
        {
            var total = 0;
            for (var i = 0; i < Data.Slots.Count; i++)
            {
                var s = Data.Slots[i];
                if (s.ItemId == itemId)
                    total += s.Count;
            }

            return total;
        }

        public bool HasItems(int itemId, int count)
        {
            if (count <= 0)
                return true;

            return GetItemCount(itemId) >= count;
        }

        public List<int> GetAllUniqueItemIds()
        {
            var set = new HashSet<int>();
            for (var i = 0; i < Data.Slots.Count; i++)
            {
                var s = Data.Slots[i];
                if (s.ItemId != 0 && s.Count > 0)
                    set.Add(s.ItemId);
            }

            var list = new List<int>(set);
            list.Sort();
            return list;
        }

        public bool AddItem(int itemId, int count = 1, int durability = 0)
        {
            if (count <= 0 || itemId == 0)
                return false;

            var pending = count;
            for (var i = 0; i < Data.Slots.Count && pending > 0; i++)
            {
                var slot = Data.Slots[i];
                if (slot.ItemId != itemId || slot.Durability != durability || slot.Count <= 0)
                    continue;

                slot.Count += pending;
                pending = 0;
                Data.Slots[i] = slot;
                break;
            }

            while (pending > 0)
            {
                var emptyIdx = FindEmpty(Data.Slots);
                if (emptyIdx < 0)
                    return false;

                var chunk = pending;
                Data.Slots[emptyIdx] = new ItemSlot
                {
                    ItemId = itemId,
                    Durability = durability,
                    Count = chunk
                };
                pending -= chunk;
            }

            this.Publish(new ItemPickedUp { ItemId = itemId, Count = count });
            this.Publish(new InventoryChanged());
            return true;
        }

        public bool RemoveItem(int itemId, int count = 1)
        {
            if (count <= 0 || itemId == 0)
                return false;

            if (!HasItems(itemId, count))
                return false;

            var remaining = count;
            for (var i = 0; i < Data.Slots.Count && remaining > 0; i++)
            {
                var slot = Data.Slots[i];
                if (slot.ItemId != itemId || slot.Count <= 0)
                    continue;

                var remove = Math.Min(slot.Count, remaining);
                slot.Count -= remove;
                remaining -= remove;
                if (slot.Count <= 0)
                {
                    slot.ItemId = 0;
                    slot.Durability = 0;
                }

                Data.Slots[i] = slot;
            }

            this.Publish(new ItemRemoved { ItemId = itemId, Count = count });
            this.Publish(new InventoryChanged());
            return true;
        }

        private static int FindEmpty(List<ItemSlot> slots)
        {
            for (var i = 0; i < slots.Count; i++)
            {
                var s = slots[i];
                if (s.ItemId == 0 || s.Count <= 0)
                    return i;
            }

            return -1;
        }
    }
}
