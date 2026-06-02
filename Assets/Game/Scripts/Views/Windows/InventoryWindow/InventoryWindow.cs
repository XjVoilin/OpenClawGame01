using System.Collections.Generic;
using System.Linq;
using cfg;
using Cysharp.Threading.Tasks;
using JulyCore;
using JulyToolkit;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CozyYard
{
    public class InventoryWindow : GameUIView
    {
        [SerializeField] private Transform _itemsContainer;
        [SerializeField] private InventorySlotEntry _itemSlotPrefab;
        [SerializeField] private TextMeshProUGUI _capacityText;
        [SerializeField] private TextMeshProUGUI _coinsText;
        [SerializeField] private UISmartButton _closeBtn;

        [Header("Category Tabs")] [SerializeField]
        private UIToggleGroup _categoryTabs;

        [Header("Detail Panel")] [SerializeField]
        private GameObject _detailPanel;

        [SerializeField] private Image _detailIcon;
        [SerializeField] private TextMeshProUGUI _detailName;
        [SerializeField] private TextMeshProUGUI _detailDesc;
        [SerializeField] private UISmartButton _useBtn;
        [SerializeField] private UISmartButton _discardBtn;

        private readonly List<InventorySlotEntry> _slotInstances = new();
        private int _selectedSlotIndex = -1;
        private int _currentCategory; // 0=All,1=Material,2=Seed,3=Product

        private static readonly string[] CategoryTypes = { "", "Material", "Seed", "Product" };

        private static readonly Dictionary<string, Color> TypeColors = new()
        {
            { "Material", new Color(0.5f, 0.6f, 0.8f) },
            { "Seed", new Color(0.4f, 0.75f, 0.4f) },
            { "Product", new Color(0.9f, 0.7f, 0.3f) },
        };

        protected override void OnViewEnable()
        {
            Subscribe<InventoryChangedEvent>(OnInventoryChanged);
            if (_closeBtn) _closeBtn.onClick.AddListener(OnClose);
            if (_categoryTabs) _categoryTabs.OnValueChanged += OnCategoryChanged;
            if (_useBtn) _useBtn.onClick.AddListener(OnUse);
            if (_discardBtn) _discardBtn.onClick.AddListener(OnDiscard);

            _selectedSlotIndex = -1;
            _currentCategory = 0;
            if (_categoryTabs) _categoryTabs.SetWithoutNotify(0);

            RefreshAsync().Forget();
            HideDetail();
        }

        protected override void OnViewDisable()
        {
            if (_closeBtn) _closeBtn.onClick.RemoveAllListeners();
            if (_categoryTabs) _categoryTabs.OnValueChanged -= OnCategoryChanged;
            if (_useBtn) _useBtn.onClick.RemoveAllListeners();
            if (_discardBtn) _discardBtn.onClick.RemoveAllListeners();
            ClearSlots();
        }

        private void OnInventoryChanged(InventoryChangedEvent e) => RefreshAsync().Forget();

        private void OnCategoryChanged(int index)
        {
            _currentCategory = index;
            _selectedSlotIndex = -1;
            HideDetail();
            RefreshAsync().Forget();
        }

        private async UniTaskVoid RefreshAsync()
        {
            var store = GetStore<InventoryStore>();
            var itemTable = GF.Config.GetTable<TbItem>();

            if (_capacityText) _capacityText.text = $"{store.UsedSlots}/{store.Capacity}";
            if (_coinsText) _coinsText.text = store.Coins.ToString();

            var filteredItems = GetFilteredItems(store, itemTable);

            EnsureSlotCount(store.Capacity);

            for (int i = 0; i < _slotInstances.Count; i++)
            {
                var slot = _slotInstances[i];
                if (i < filteredItems.Count)
                {
                    var stack = filteredItems[i];
                    var cfg = itemTable.GetOrDefault(stack.ItemId);
                    slot.Setup(stack.ItemId, stack.Quantity, cfg.IconSprite, GetItemColor(cfg.Type));
                    slot.SetSelected(i == _selectedSlotIndex);
                }
                else
                {
                    slot.SetEmpty();
                }
            }
        }

        private List<ItemStack> GetFilteredItems(InventoryStore store, TbItem itemTable)
        {
            if (_currentCategory == 0)
                return store.Items.ToList();

            string typeFilter = CategoryTypes[_currentCategory];
            return store.Items
                .Where(s =>
                {
                    var cfg = itemTable?.GetOrDefault(s.ItemId);
                    return cfg != null && cfg.Type == typeFilter;
                })
                .ToList();
        }

        private void EnsureSlotCount(int count)
        {
            while (_slotInstances.Count < count)
            {
                if (_itemSlotPrefab == null || _itemsContainer == null) break;
                var slot = Object.Instantiate(_itemSlotPrefab, _itemsContainer);
                slot.gameObject.SetActive(true);
                int idx = _slotInstances.Count;
                slot.Slot.SetIndex(idx);
                slot.Slot.OnClicked += OnSlotClicked;
                _slotInstances.Add(slot);
            }

            while (_slotInstances.Count > count)
            {
                int last = _slotInstances.Count - 1;
                var slot = _slotInstances[last];
                slot.Slot.OnClicked -= OnSlotClicked;
                Object.Destroy(slot.gameObject);
                _slotInstances.RemoveAt(last);
            }
        }

        private void OnSlotClicked(UIItemSlot slot)
        {
            int idx = slot.Index;
            if (idx < 0 || idx >= _slotInstances.Count) return;

            var entry = _slotInstances[idx];
            if (entry.ItemId < 0)
            {
                _selectedSlotIndex = -1;
                HideDetail();
                UpdateSelection();
                return;
            }

            _selectedSlotIndex = idx;
            UpdateSelection();
            ShowDetail(entry.ItemId);
        }

        private void UpdateSelection()
        {
            for (int i = 0; i < _slotInstances.Count; i++)
                _slotInstances[i].SetSelected(i == _selectedSlotIndex);
        }

        private void ShowDetail(int itemId)
        {
            var itemTable = GF.Config.GetTable<TbItem>();
            var cfg = itemTable?.GetOrDefault(itemId);
            if (cfg == null)
            {
                HideDetail();
                return;
            }

            if (_detailPanel) _detailPanel.SetActive(true);
            if (_detailIcon)
            {
                _detailIcon.LoadSprite(cfg.IconSprite);
                _detailIcon.color = Color.white;
                _detailIcon.enabled = true;
            }

            if (_detailName) _detailName.text = GF.Localization.Get(cfg.NameKey);
            if (_detailDesc) _detailDesc.text = GF.Localization.Get(cfg.DescKey);
        }

        private void HideDetail()
        {
            if (_detailPanel) _detailPanel.SetActive(false);
        }

        private void OnUse()
        {
            if (_selectedSlotIndex < 0 || _selectedSlotIndex >= _slotInstances.Count) return;
            int itemId = _slotInstances[_selectedSlotIndex].ItemId;
            if (itemId < 0) return;

            var itemCfg = GF.Config.GetTable<TbItem>()?.GetOrDefault(itemId);
            bool isSeed = itemCfg != null && itemCfg.Type == "Seed";

            Publish(new UseItemEvent { ItemId = itemId });

            if (isSeed)
                CloseWindow();
        }

        private void OnDiscard()
        {
            if (_selectedSlotIndex < 0 || _selectedSlotIndex >= _slotInstances.Count) return;
            int itemId = _slotInstances[_selectedSlotIndex].ItemId;
            if (itemId < 0) return;
            Publish(new DiscardItemEvent { ItemId = itemId, Quantity = 1 });
        }

        private static Color GetItemColor(string type)
        {
            if (type != null && TypeColors.TryGetValue(type, out var c))
                return c;
            return new Color(0.6f, 0.6f, 0.6f);
        }

        private void ClearSlots()
        {
            foreach (var slot in _slotInstances)
            {
                slot.Slot.OnClicked -= OnSlotClicked;
                Object.Destroy(slot.gameObject);
            }

            _slotInstances.Clear();
        }

        private void OnClose() => CloseWindow();
    }
}