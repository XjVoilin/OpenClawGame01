using System.Collections.Generic;
using JulyArch;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
        [SerializeField] private Button _askBtn;
        [SerializeField] private Button _closeBtn;

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
            if (_resultText) _resultText.text = $"妈妈教了你新配方! (#{e.RecipeId})";
            Refresh();
        }

        private void Refresh()
        {
            if (_hintText) _hintText.text = "告诉妈妈你有什么材料，她可能知道配方";

            var craftQueries = this.Query<ICraftQueries>();
            int remaining = MomAskLimit - craftQueries.MomAsksToday;
            if (_asksRemainingText) _asksRemainingText.text = $"今日剩余询问: {remaining}/{MomAskLimit}";

            if (_askBtn) _askBtn.interactable = remaining > 0 && _selectedItemId > 0;

            RefreshItemList();
        }

        private void RefreshItemList()
        {
            ClearItemEntries();
            if (_itemEntryPrefab == null || _itemsContainer == null) return;

            var inventoryQueries = this.Query<IInventoryQueries>();
            _selectedItemId = 0;

            foreach (var stack in inventoryQueries.Items)
            {
                var go = Object.Instantiate(_itemEntryPrefab, _itemsContainer);
                go.SetActive(true);

                var text = go.GetComponentInChildren<TextMeshProUGUI>();
                if (text) text.text = $"#{stack.ItemId} ×{stack.Quantity}";

                var btn = go.GetComponentInChildren<Button>();
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
            if (_resultText) _resultText.text = $"已选择: #{itemId}";
            RefreshAskButton();
        }

        private void RefreshAskButton()
        {
            var craftQueries = this.Query<ICraftQueries>();
            int remaining = MomAskLimit - craftQueries.MomAsksToday;
            if (_askBtn) _askBtn.interactable = remaining > 0 && _selectedItemId > 0;
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
                var btn = go.GetComponentInChildren<Button>();
                if (btn) btn.onClick.RemoveAllListeners();
                Object.Destroy(go);
            }
            _itemEntries.Clear();
        }

        private void OnClose() => CloseWindow();
    }
}
