using JulyArch;
using OffTrail.Inventory;
using TMPro;
using UnityEngine;

namespace OffTrail
{
    public class InventoryWindow : GameUIView
    {
        [SerializeField] private Transform _slotContainer;
        [SerializeField] private TextMeshProUGUI _titleText;

        protected override void OnBeforeOpen()
        {
            this.Subscribe<InventoryChanged>(_ => RefreshSlots());

            if (_titleText) _titleText.text = "背包";
            RefreshSlots();
            base.OnBeforeOpen();
        }

        private void RefreshSlots()
        {
            // Placeholder: will be populated when UI prefabs are built
        }
    }
}
