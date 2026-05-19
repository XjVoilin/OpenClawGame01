using System.Collections.Generic;
using cfg;
using JulyArch;
using JulyCore;
using TMPro;
using UnityEngine;

namespace SpiritHealer
{
    /// <summary>
    /// 处方面板：君臣佐使四个药材槽 + 药材列表 + 确认开方。
    /// 由 VisitorWindow 的"开方"按钮打开。
    /// </summary>
    public class PrescriptionWindow : GameUIView
    {
        [Header("处方槽位")]
        [SerializeField] private PrescriptionSlotUI _junSlot;
        [SerializeField] private PrescriptionSlotUI _chenSlot;
        [SerializeField] private PrescriptionSlotUI _zuoSlot;
        [SerializeField] private PrescriptionSlotUI _shiSlot;

        [Header("药材列表")]
        [SerializeField] private Transform _herbListRoot;
        [SerializeField] private GameObject _herbItemPrefab;

        [Header("操作")]
        [SerializeField] private UISmartButton _confirmBtn;
        [SerializeField] private UISmartButton _clearBtn;
        [SerializeField] private UISmartButton _closeBtn;

        private GameLoopSystem _gameLoop;
        private InventoryStore _inventoryStore;
        private PrescriptionStore _prescriptionStore;

        private HerbRole? _selectedRole;
        private readonly List<GameObject> _herbItems = new();
        private TreatmentResultData _pendingResult;

        protected override void OnBeforeOpen()
        {
            _gameLoop = this.GetSystem<GameLoopSystem>();
            _inventoryStore = this.GetStore<InventoryStore>();
            _prescriptionStore = this.GetStore<PrescriptionStore>();

            _confirmBtn.onClick.AddListener(OnConfirm);
            _clearBtn.onClick.AddListener(OnClear);
            _closeBtn.onClick.AddListener(OnCloseClicked);

            this.Subscribe<TreatmentCompletedEvent>(OnTreatmentCompleted);

            InitSlots();
            RefreshSlots();
            BuildHerbList();
        }

        protected override void OnClose()
        {
            _confirmBtn.onClick.RemoveAllListeners();
            _clearBtn.onClick.RemoveAllListeners();
            _closeBtn.onClick.RemoveAllListeners();
            this.UnsubscribeAll();
            ClearHerbItems();
        }

        private void OnTreatmentCompleted(TreatmentCompletedEvent e)
        {
            _pendingResult = new TreatmentResultData
            {
                Score = e.EfficacyScore,
                ReputationGained = e.ReputationGained,
                CoinsGained = e.CoinsGained
            };
        }

        private void InitSlots()
        {
            if (_junSlot) _junSlot.Init(HerbRole.Jun, "君", OnSlotClicked, OnSlotClear);
            if (_chenSlot) _chenSlot.Init(HerbRole.Chen, "臣", OnSlotClicked, OnSlotClear);
            if (_zuoSlot) _zuoSlot.Init(HerbRole.Zuo, "佐", OnSlotClicked, OnSlotClear);
            if (_shiSlot) _shiSlot.Init(HerbRole.Shi, "使", OnSlotClicked, OnSlotClear);
        }

        private void OnSlotClicked(HerbRole role)
        {
            _selectedRole = role;
            HighlightSelectedSlot();
        }

        private void OnSlotClear(HerbRole role)
        {
            _gameLoop.SetPrescriptionSlot(role, 0, 0);
            RefreshSlots();
        }

        private void HighlightSelectedSlot()
        {
            if (_junSlot) _junSlot.SetSelected(_selectedRole == HerbRole.Jun);
            if (_chenSlot) _chenSlot.SetSelected(_selectedRole == HerbRole.Chen);
            if (_zuoSlot) _zuoSlot.SetSelected(_selectedRole == HerbRole.Zuo);
            if (_shiSlot) _shiSlot.SetSelected(_selectedRole == HerbRole.Shi);
        }

        private void RefreshSlots()
        {
            RefreshSlot(_junSlot, HerbRole.Jun);
            RefreshSlot(_chenSlot, HerbRole.Chen);
            RefreshSlot(_zuoSlot, HerbRole.Zuo);
            RefreshSlot(_shiSlot, HerbRole.Shi);

            int filledCount = 0;
            foreach (var slot in _prescriptionStore.CurrentSlots)
            {
                if (slot.HerbId > 0) filledCount++;
            }
            _confirmBtn.SetInteractable(filledCount > 0);
        }

        private void RefreshSlot(PrescriptionSlotUI slotUI, HerbRole role)
        {
            if (!slotUI) return;
            var slot = _prescriptionStore.GetSlot(role);
            if (slot != null && slot.HerbId > 0)
            {
                var herb = CfgTable.Herb.GetOrDefault(slot.HerbId);
                var name = herb?.Name ?? $"#{slot.HerbId}";
                slotUI.SetHerb(name, slot.Quality);
            }
            else
            {
                slotUI.SetEmpty();
            }
        }

        private void BuildHerbList()
        {
            ClearHerbItems();
            if (!_herbItemPrefab || !_herbListRoot) return;

            foreach (var item in _inventoryStore.Herbs)
            {
                if (item.Count <= 0) continue;

                var herb = CfgTable.Herb.GetOrDefault(item.ConfigId);
                if (herb == null) continue;

                var go = Instantiate(_herbItemPrefab, _herbListRoot);
                go.SetActive(true);

                var herbUI = go.GetComponent<HerbItemUI>();
                if (herbUI)
                {
                    var knowledge = _prescriptionStore.GetKnowledge(item.ConfigId);
                    herbUI.Init(herb, item.Quality, item.Count, knowledge, OnHerbSelected);
                }

                _herbItems.Add(go);
            }
        }

        private void OnHerbSelected(int herbId, int quality)
        {
            if (_selectedRole == null)
            {
                _selectedRole = GetFirstEmptyRole();
                if (_selectedRole == null) return;
                HighlightSelectedSlot();
            }

            _gameLoop.SetPrescriptionSlot(_selectedRole.Value, herbId, quality);
            _selectedRole = GetNextRole(_selectedRole.Value);
            HighlightSelectedSlot();
            RefreshSlots();
        }

        private HerbRole? GetFirstEmptyRole()
        {
            if (IsSlotEmpty(HerbRole.Jun)) return HerbRole.Jun;
            if (IsSlotEmpty(HerbRole.Chen)) return HerbRole.Chen;
            if (IsSlotEmpty(HerbRole.Zuo)) return HerbRole.Zuo;
            if (IsSlotEmpty(HerbRole.Shi)) return HerbRole.Shi;
            return null;
        }

        private HerbRole? GetNextRole(HerbRole current)
        {
            var order = new[] { HerbRole.Jun, HerbRole.Chen, HerbRole.Zuo, HerbRole.Shi };
            int start = System.Array.IndexOf(order, current);
            for (int i = 1; i <= order.Length; i++)
            {
                var candidate = order[(start + i) % order.Length];
                if (IsSlotEmpty(candidate)) return candidate;
            }
            return null;
        }

        private bool IsSlotEmpty(HerbRole role)
        {
            var slot = _prescriptionStore.GetSlot(role);
            return slot == null || slot.HerbId <= 0;
        }

        private void OnConfirm()
        {
            _pendingResult = null;
            _gameLoop.PrescribeAndSettle();

            var resultData = _pendingResult ?? new TreatmentResultData();
            GF.UI.Open(UIWindowId.TreatmentResultWindow, resultData);
            CloseWindow();
        }

        private void OnClear()
        {
            _gameLoop.ClearPrescription();
            _selectedRole = HerbRole.Jun;
            RefreshSlots();
            HighlightSelectedSlot();
        }

        private void OnCloseClicked()
        {
            _gameLoop.ClearPrescription();
            CloseWindow();
        }

        private void ClearHerbItems()
        {
            foreach (var go in _herbItems) Destroy(go);
            _herbItems.Clear();
        }
    }
}
