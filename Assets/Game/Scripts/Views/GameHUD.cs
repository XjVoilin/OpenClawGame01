using JulyCore;
using TMPro;
using UnityEngine;

namespace CozyYard
{
    public class GameHUD : GameUIView
    {
        [SerializeField] private UISmartButton _inventoryBtn;
        [SerializeField] private UISmartButton _buildBtn;
        [SerializeField] private UISmartButton _craftBtn;
        [SerializeField] private UISmartButton _visitorBtn;
        [SerializeField] private UISmartButton _milestoneBtn;
        [SerializeField] private UISmartButton _recipeBookBtn;
        [SerializeField] private UISmartButton _phoneBtn;
        [SerializeField] private UISmartButton _gateToggleBtn;
        [SerializeField] private TextMeshProUGUI _gateText;
        [SerializeField] private TextMeshProUGUI _visitorBadgeText;

        protected override void OnViewEnable()
        {
            Subscribe<VisitorArrivedEvent>(OnVisitorChanged);
            Subscribe<VisitorLeftEvent>(OnVisitorLeft);
            Subscribe<OrderCompletedEvent>(OnOrderCompleted);

            if (_inventoryBtn) _inventoryBtn.onClick.AddListener(() => GF.UI.Open(UIWindowId.InventoryWindow));
            if (_buildBtn) _buildBtn.onClick.AddListener(() => GF.UI.Open(UIWindowId.BuildWindow));
            if (_craftBtn) _craftBtn.onClick.AddListener(() => GF.UI.Open(UIWindowId.CraftWindow));
            if (_visitorBtn) _visitorBtn.onClick.AddListener(() => GF.UI.Open(UIWindowId.VisitorWindow));
            if (_milestoneBtn) _milestoneBtn.onClick.AddListener(() => GF.UI.Open(UIWindowId.MilestoneWindow));
            if (_recipeBookBtn) _recipeBookBtn.onClick.AddListener(() => GF.UI.Open(UIWindowId.RecipeBookWindow));
            if (_phoneBtn) _phoneBtn.onClick.AddListener(() => GF.UI.Open(UIWindowId.PhoneWindow));
            if (_gateToggleBtn) _gateToggleBtn.onClick.AddListener(OnGateToggle);

            RefreshGate();
            RefreshVisitorBadge();
        }

        protected override void OnViewDisable()
        {
            if (_inventoryBtn) _inventoryBtn.onClick.RemoveAllListeners();
            if (_buildBtn) _buildBtn.onClick.RemoveAllListeners();
            if (_craftBtn) _craftBtn.onClick.RemoveAllListeners();
            if (_visitorBtn) _visitorBtn.onClick.RemoveAllListeners();
            if (_milestoneBtn) _milestoneBtn.onClick.RemoveAllListeners();
            if (_recipeBookBtn) _recipeBookBtn.onClick.RemoveAllListeners();
            if (_phoneBtn) _phoneBtn.onClick.RemoveAllListeners();
            if (_gateToggleBtn) _gateToggleBtn.onClick.RemoveAllListeners();
        }

        private void OnVisitorChanged(VisitorArrivedEvent e) => RefreshVisitorBadge();
        private void OnVisitorLeft(VisitorLeftEvent e) => RefreshVisitorBadge();
        private void OnOrderCompleted(OrderCompletedEvent e) => RefreshVisitorBadge();

        private void OnGateToggle()
        {
            GetSystem<VisitorSystem>().ToggleGate();
            RefreshGate();
        }

        private void RefreshGate()
        {
            var q = this.GetStore<VisitorStore>();
            if (_gateText) _gateText.text = q.IsGateOpen ? GF.Localization.Get("gate_open") : GF.Localization.Get("gate_close");
        }

        private void RefreshVisitorBadge()
        {
            if (_visitorBadgeText == null) return;
            var q = this.GetStore<VisitorStore>();
            int count = q.TodayOrders.Count;
            _visitorBadgeText.text = count > 0 ? count.ToString() : "";
            _visitorBadgeText.gameObject.SetActive(count > 0);
        }
    }
}
