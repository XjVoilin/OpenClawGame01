using System.Collections.Generic;
using JulyArch;

namespace CozyYard
{
    public interface IInventoryQueries : IStoreQueries
    {
        int Capacity { get; }
        int UsedSlots { get; }
        int FreeSlots { get; }
        int Coins { get; }
        IReadOnlyList<ItemStack> Items { get; }
        int GetItemCount(int itemId);
        bool HasItem(int itemId, int quantity = 1);
        bool HasSpace(int itemId, int quantity = 1);
    }
}
