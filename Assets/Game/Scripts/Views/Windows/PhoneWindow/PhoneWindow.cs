using System.Collections.Generic;
using cfg;
using JulyArch;
using JulyCore;
using TMPro;
using UnityEngine;

namespace CozyYard
{
    public class PhoneWindow : GameUIView
    {
        private const int MomAskLimit = 1;

        [SerializeField] private TextMeshProUGUI _hintText;
        [SerializeField] private TextMeshProUGUI _asksRemainingText;
        [SerializeField] private TextMeshProUGUI _resultText;
        [SerializeField] private Transform _itemsContainer;
        [SerializeField] private GameObject _itemEntryPrefab;
        [SerializeField] private UISmartButton _askBtn;
        [SerializeField] private UISmartButton _closeBtn;

        private readonly List<GameObject> _itemEntries = new();
        private int _selectedItemId;

        protected override void OnViewEnable()
        {
            Subscribe<RecipeUnlockedEvent>(OnRecipeUnlocked);
            if (_askBtn) _askBtn.onClick.AddListener(OnAsk);
            if (_closeBtn) _closeBtn.onClick.AddListener(OnClose);
            Refresh();
        }

        protected override void OnViewDisable()
        {
            if (_askBtn) _askBtn.onClick.RemoveAllListeners();
            if (_closeBtn) _closeBtn.onClick.RemoveAllListeners();
            ClearItemEntries();
        }

        private void OnRecipeUnlocked(RecipeUnlockedEvent e)
        {
            var recipeName = GF.Config.GetTable<TbRecipe>()?.GetOrDefault(e.RecipeId)?.Name ?? $"#{e.RecipeId}";
            if (_resultText) _resultText.text = $"妈妈教了你新配方! ({recipeName})";
            Refresh();
        }

        private void Refresh()
        {
            if (_hintText) _hintText.text = "告诉妈妈你有什么材料，她可能知道配方";

            var craftQueries = GetStore<CraftStore>();
            int remaining = MomAskLimit - craftQueries.MomAsksToday;
            if (_asksRemainingText) _asksRemainingText.text = $"今日剩余询问: {remaining}/{MomAskLimit}";

            if (_askBtn) _askBtn.SetInteractable(remaining > 0 && _selectedItemId > 0);

            RefreshItemList();
        }

        private void RefreshItemList()
        {
            ClearItemEntries();
            if (_itemEntryPrefab == null || _itemsContainer == null) return;

            var inventoryQueries = GetStore<InventoryStore>();
            _selectedItemId = 0;

            foreach (var stack in inventoryQueries.Items)
            {
                var go = Instantiate(_itemEntryPrefab, _itemsContainer);
                go.SetActive(true);

                var text = go.GetComponentInChildren<TextMeshProUGUI>();
                var itemName = GF.Config.GetTable<TbItem>()?.GetOrDefault(stack.ItemId)?.Name ?? $"#{stack.ItemId}";
                if (text) text.text = $"{itemName} ×{stack.Quantity}";

                var btn = go.GetComponentInChildren<UISmartButton>();
                if (btn)
                {
                    int itemId = stack.ItemId;
                    btn.onClick.AddListener(() => OnSelectItem(itemId));
                }

                _itemEntries.Add(go);
            }
        }

        private void OnSelectItem(int itemId)
        {
            _selectedItemId = itemId;
            var selectedName = GF.Config.GetTable<TbItem>()?.GetOrDefault(itemId)?.Name ?? $"#{itemId}";
            if (_resultText) _resultText.text = $"已选择: {selectedName}";
            RefreshAskButton();
        }

        private void RefreshAskButton()
        {
            var craftQueries = GetStore<CraftStore>();
            int remaining = MomAskLimit - craftQueries.MomAsksToday;
            if (_askBtn) _askBtn.SetInteractable(remaining > 0 && _selectedItemId > 0);
        }

        private void OnAsk()
        {
            if (_selectedItemId <= 0) return;

            var craftSystem = GetSystem<CraftSystem>();
            bool success = craftSystem.AskMom(_selectedItemId);

            if (!success && _resultText)
                _resultText.text = "妈妈也不知道这个能做什么…";

            Refresh();
        }

        private void ClearItemEntries()
        {
            foreach (var go in _itemEntries)
            {
                var btn = go.GetComponentInChildren<UISmartButton>();
                if (btn) btn.onClick.RemoveAllListeners();
                Object.Destroy(go);
            }
            _itemEntries.Clear();
        }

        private void OnClose() => CloseWindow();
    }
}
