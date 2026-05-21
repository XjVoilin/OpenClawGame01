using System.Collections.Generic;
using cfg;
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
        [SerializeField] private PhoneItemEntry _itemEntryPrefab;
        [SerializeField] private UISmartButtonGray _askBtn;
        [SerializeField] private UISmartButton _closeBtn;

        private readonly List<PhoneItemEntry> _itemEntries = new();
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
            var nameKey = GF.Config.GetTable<TbRecipe>()?.GetOrDefault(e.RecipeId)?.NameKey ?? $"#{e.RecipeId}";
            if (_resultText) _resultText.text = string.Format(GF.Localization.Get("mom_new_recipe"), GF.Localization.Get(nameKey));
            Refresh();
        }

        private void Refresh()
        {
            if (_hintText) _hintText.text = GF.Localization.Get("mom_hint");

            var craftQueries = GetStore<CraftStore>();
            int remaining = MomAskLimit - craftQueries.MomAsksToday;
            if (_asksRemainingText) _asksRemainingText.text = string.Format(GF.Localization.Get("asks_remaining"), remaining, MomAskLimit);

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
                var entry = Object.Instantiate(_itemEntryPrefab, _itemsContainer);
                entry.gameObject.SetActive(true);

                var nameKey = GF.Config.GetTable<TbItem>()?.GetOrDefault(stack.ItemId)?.NameKey ?? $"#{stack.ItemId}";
                int itemId = stack.ItemId;
                entry.Setup(
                    $"{GF.Localization.Get(nameKey)} ×{stack.Quantity}",
                    () => OnSelectItem(itemId)
                );

                _itemEntries.Add(entry);
            }
        }

        private void OnSelectItem(int itemId)
        {
            _selectedItemId = itemId;
            var nameKey = GF.Config.GetTable<TbItem>()?.GetOrDefault(itemId)?.NameKey ?? $"#{itemId}";
            if (_resultText) _resultText.text = string.Format(GF.Localization.Get("selected"), GF.Localization.Get(nameKey));
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
                _resultText.text = GF.Localization.Get("mom_unknown");

            Refresh();
        }

        private void ClearItemEntries()
        {
            foreach (var entry in _itemEntries)
            {
                entry.Cleanup();
                Object.Destroy(entry.gameObject);
            }
            _itemEntries.Clear();
        }

        private void OnClose() => CloseWindow();
    }
}
