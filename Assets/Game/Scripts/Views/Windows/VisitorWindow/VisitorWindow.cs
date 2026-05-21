using System.Collections.Generic;
using cfg;
using JulyCore;
using TMPro;
using UnityEngine;

namespace CozyYard
{
    public class VisitorWindow : GameUIView
    {
        [SerializeField] private Transform _listContainer;
        [SerializeField] private VisitorEntry _entryPrefab;
        [SerializeField] private UISmartButton _gateToggleBtn;
        [SerializeField] private TextMeshProUGUI _gateText;
        [SerializeField] private UISmartButton _closeBtn;

        private readonly List<VisitorEntry> _entries = new();

        protected override void OnViewEnable()
        {
            Subscribe<VisitorArrivedEvent>(OnVisitorChanged);
            Subscribe<VisitorLeftEvent>(OnVisitorLeft);
            Subscribe<OrderCompletedEvent>(OnOrderCompleted);
            if (_gateToggleBtn) _gateToggleBtn.onClick.AddListener(OnGateToggle);
            if (_closeBtn) _closeBtn.onClick.AddListener(OnClose);
            Refresh();
        }

        protected override void OnViewDisable()
        {
            if (_gateToggleBtn) _gateToggleBtn.onClick.RemoveAllListeners();
            if (_closeBtn) _closeBtn.onClick.RemoveAllListeners();
            ClearEntries();
        }

        private void OnVisitorChanged(VisitorArrivedEvent e) => Refresh();
        private void OnVisitorLeft(VisitorLeftEvent e) => Refresh();
        private void OnOrderCompleted(OrderCompletedEvent e) => Refresh();

        private void Refresh()
        {
            ClearEntries();
            RefreshGate();

            if (_entryPrefab == null || _listContainer == null) return;

            var visitorStore = GetStore<VisitorStore>();
            var visitorSystem = GetSystem<VisitorSystem>();

            foreach (var order in visitorStore.TodayOrders)
            {
                var entry = Object.Instantiate(_entryPrefab, _listContainer);
                entry.gameObject.SetActive(true);

                string visitorNameKey = GF.Config.GetTable<TbVisitor>()?.GetOrDefault(order.VisitorId)?.NameKey ?? $"#{order.VisitorId}";
                string itemNameKey = GF.Config.GetTable<TbItem>()?.GetOrDefault(order.RequestedItemId)?.NameKey ?? $"#{order.RequestedItemId}";
                string reward = FormatReward(order);

                string info = $"{GF.Localization.Get(visitorNameKey)}\n{GF.Localization.Get("need")} {GF.Localization.Get(itemNameKey)}×{order.RequestedQuantity}\n{GF.Localization.Get("reward")} {reward}";

                var orderCopy = order;
                entry.Setup(
                    info,
                    () => visitorSystem.FulfillOrder(orderCopy),
                    () => visitorSystem.DismissVisitor(orderCopy)
                );

                _entries.Add(entry);
            }
        }

        private static string FormatReward(ActiveOrder order)
        {
            var parts = new List<string>();
            if (order.RewardCoins > 0) parts.Add($"{order.RewardCoins} {GF.Localization.Get("coins")}");
            if (order.RewardItemId > 0 && order.RewardItemQty > 0)
            {
                var config = GF.Config.GetTable<TbItem>()?.GetOrDefault(order.RewardItemId);
                if (config != null)
                {
                    parts.Add($"{GF.Localization.Get(config.NameKey)}×{order.RewardItemQty}");
                }
            }

            return parts.Count > 0 ? string.Join(", ", parts) : GF.Localization.Get("none");
        }

        private void OnGateToggle()
        {
            GetSystem<VisitorSystem>().ToggleGate();
            RefreshGate();
        }

        private void RefreshGate()
        {
            var q = GetStore<VisitorStore>();
            if (_gateText) _gateText.text = q.IsGateOpen ? GF.Localization.Get("gate_open") : GF.Localization.Get("gate_close");
        }

        private void ClearEntries()
        {
            foreach (var entry in _entries)
            {
                entry.Cleanup();
                Object.Destroy(entry.gameObject);
            }
            _entries.Clear();
        }

        private void OnClose() => CloseWindow();
    }
}
