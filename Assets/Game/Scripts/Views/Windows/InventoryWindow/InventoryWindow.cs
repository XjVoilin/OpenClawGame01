using System.Collections.Generic;
using JulyArch;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CozyYard
{
    public class InventoryWindow : GameUIView
    {
        [SerializeField] private Transform _itemsContainer;
        [SerializeField] private GameObject _itemSlotPrefab;
        [SerializeField] private TextMeshProUGUI _capacityText;
        [SerializeField] private TextMeshProUGUI _coinsText;
        [SerializeField] private Button _closeBtn;

        private readonly List<GameObject> _slotInstances = new();

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
            var q = this.Query<IInventoryQueries>();

            if (_capacityText) _capacityText.text = $"{q.UsedSlots}/{q.Capacity}";
            if (_coinsText) _coinsText.text = q.Coins.ToString();

            ClearSlots();

            foreach (var stack in q.Items)
            {
                if (_itemSlotPrefab == null || _itemsContainer == null) break;

                var go = Object.Instantiate(_itemSlotPrefab, _itemsContainer);
                go.SetActive(true);

                var nameText = go.GetComponentInChildren<TextMeshProUGUI>();
                if (nameText) nameText.text = $"#{stack.ItemId} x{stack.Quantity}";

                _slotInstances.Add(go);
            }
        }

        private void ClearSlots()
        {
            foreach (var go in _slotInstances)
            {
                Object.Destroy(go);
            }
            _slotInstances.Clear();
        }

        private void OnClose()
        {
            CloseWindow();
        }
    }
}
