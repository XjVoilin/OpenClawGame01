using System.Collections.Generic;
using cfg;
using JulyCore;
using TMPro;
using UnityEngine;

namespace CozyYard
{
    public class InventoryWindow : GameUIView
    {
        [SerializeField] private Transform _itemsContainer;
        [SerializeField] private InventorySlotEntry _itemSlotPrefab;
        [SerializeField] private TextMeshProUGUI _capacityText;
        [SerializeField] private TextMeshProUGUI _coinsText;
        [SerializeField] private UISmartButton _closeBtn;

        private readonly List<InventorySlotEntry> _slotInstances = new();

        protected override void OnViewEnable()
        {
            Subscribe<InventoryChangedEvent>(OnInventoryChanged);
            if (_closeBtn) _closeBtn.onClick.AddListener(OnClose);
            Refresh();
        }

        protected override void OnViewDisable()
        {
            if (_closeBtn) _closeBtn.onClick.RemoveAllListeners();
            ClearSlots();
        }

        private void OnInventoryChanged(InventoryChangedEvent e) => Refresh();

        private void Refresh()
        {
            var q = GetStore<InventoryStore>();

            if (_capacityText) _capacityText.text = $"{q.UsedSlots}/{q.Capacity}";
            if (_coinsText) _coinsText.text = q.Coins.ToString();

            ClearSlots();

            foreach (var stack in q.Items)
            {
                if (_itemSlotPrefab == null || _itemsContainer == null) break;

                var slot = Object.Instantiate(_itemSlotPrefab, _itemsContainer);
                slot.gameObject.SetActive(true);

                var nameKey = GF.Config.GetTable<TbItem>()?.GetOrDefault(stack.ItemId)?.NameKey ?? $"#{stack.ItemId}";
                slot.Setup($"{GF.Localization.Get(nameKey)} ×{stack.Quantity}");

                _slotInstances.Add(slot);
            }
        }

        private void ClearSlots()
        {
            foreach (var slot in _slotInstances)
                Object.Destroy(slot.gameObject);
            _slotInstances.Clear();
        }

        private void OnClose() => CloseWindow();
    }
}
