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
        [SerializeField] private GameObject _entryPrefab;
        [SerializeField] private UISmartButton _gateToggleBtn;
        [SerializeField] private TextMeshProUGUI _gateText;
        [SerializeField] private UISmartButton _closeBtn;

        private readonly List<GameObject> _entries = new();

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
                var go = Object.Instantiate(_entryPrefab, _listContainer);
                go.SetActive(true);

                string visitorName = GF.Config.GetTable<TbVisitor>()?.GetOrDefault(order.VisitorId)?.Name ?? $"#{order.VisitorId}";
                string itemName = GF.Config.GetTable<TbItem>()?.GetOrDefault(order.RequestedItemId)?.Name ?? $"#{order.RequestedItemId}";
                string reward = FormatReward(order);

                var texts = go.GetComponentsInChildren<TextMeshProUGUI>();
                if (texts.Length > 0)
                {
                    texts[0].text = $"{visitorName}\n需要: {itemName}×{order.RequestedQuantity}\n奖励: {reward}";
                }

                var buttons = go.GetComponentsInChildren<UISmartButton>();
                if (buttons.Length >= 2)
                {
                    var orderCopy = order;
                    buttons[0].onClick.AddListener(() => visitorSystem.FulfillOrder(orderCopy));
                    buttons[1].onClick.AddListener(() => visitorSystem.DismissVisitor(orderCopy));
                }
                else if (buttons.Length == 1)
                {
                    var orderCopy = order;
                    buttons[0].onClick.AddListener(() => visitorSystem.FulfillOrder(orderCopy));
                }

                _entries.Add(go);
            }
        }

        private static string FormatReward(ActiveOrder order)
        {
            var parts = new List<string>();
            if (order.RewardCoins > 0) parts.Add($"{order.RewardCoins} 金币");
            if (order.RewardItemId > 0 && order.RewardItemQty > 0)
            {
                var config =  GF.Config.GetTable<TbItem>()?.GetOrDefault(order.RewardItemId);
                if (config != null)
                {
                    parts.Add($"{config.Name}×{order.RewardItemQty}");
                }
            }

            return parts.Count > 0 ? string.Join(", ", parts) : "无";
        }

        private void OnGateToggle()
        {
            GetSystem<VisitorSystem>().ToggleGate();
            RefreshGate();
        }

        private void RefreshGate()
        {
            var q = GetStore<VisitorStore>();
            if (_gateText) _gateText.text = q.IsGateOpen ? "大门: 开" : "大门: 关";
        }

        private void ClearEntries()
        {
            foreach (var go in _entries)
            {
                foreach (var btn in go.GetComponentsInChildren<UISmartButton>())
                    btn.onClick.RemoveAllListeners();
                Object.Destroy(go);
            }
            _entries.Clear();
        }

        private void OnClose() => CloseWindow();
    }
}
