using JulyArch;
using JulyCore;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CozyYard
{
    public class GameHUD : GameView
    {
        [SerializeField] private Button _inventoryBtn;
        [SerializeField] private Button _buildBtn;
        [SerializeField] private Button _craftBtn;
        [SerializeField] private Button _visitorBtn;
        [SerializeField] private Button _milestoneBtn;
        [SerializeField] private Button _recipeBookBtn;
        [SerializeField] private Button _phoneBtn;
        [SerializeField] private Button _gateToggleBtn;
        [SerializeField] private TextMeshProUGUI _gateText;
        [SerializeField] private TextMeshProUGUI _visitorBadgeText;

        public override IGameContext GetArchitecture() => AppArch.Context;

        protected override void OnViewEnable()
        {
            this.Subscribe<VisitorArrivedEvent>(OnVisitorChanged);
            this.Subscribe<VisitorLeftEvent>(OnVisitorLeft);
            this.Subscribe<OrderCompletedEvent>(OnOrderCompleted);

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
            this.GetSystem<VisitorSystem>().ToggleGate();
            RefreshGate();
        }

        private void RefreshGate()
        {
            var q = this.Query<IVisitorQueries>();
            if (_gateText) _gateText.text = q.IsGateOpen ? "大门: 开" : "大门: 关";
        }

        private void RefreshVisitorBadge()
        {
            if (_visitorBadgeText == null) return;
            var q = this.Query<IVisitorQueries>();
            int count = q.TodayOrders.Count;
            _visitorBadgeText.text = count > 0 ? count.ToString() : "";
            _visitorBadgeText.gameObject.SetActive(count > 0);
        }
    }
}
