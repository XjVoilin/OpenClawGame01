using JulyToolkit;
using UnityEngine;

namespace CozyYard
{
    public class InventorySlotEntry : MonoBehaviour
    {
        [SerializeField] private UIItemSlot _slot;

        public int ItemId { get; private set; } = -1;
        public UIItemSlot Slot => _slot;

        public void Setup(int itemId, int quantity, Sprite icon, Color tint)
        {
            ItemId = itemId;
            _slot.SetItem(icon, quantity, tint);
        }

        public void SetEmpty()
        {
            ItemId = -1;
            _slot.SetEmpty();
        }

        public void SetSelected(bool selected)
        {
            _slot.SetSelected(selected);
        }
    }
}
