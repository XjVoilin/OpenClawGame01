using System.Collections.Generic;
using cfg;
using JulyCore;
using TMPro;
using UnityEngine;

namespace CozyYard
{
    public class RecipeBookWindow : GameUIView
    {
        [SerializeField] private Transform _listContainer;
        [SerializeField] private GameObject _entryPrefab;
        [SerializeField] private UISmartButton _closeBtn;

        private readonly List<GameObject> _entries = new();

        protected override void OnViewEnable()
        {
            Subscribe<RecipeUnlockedEvent>(OnRecipeUnlocked);
            if (_closeBtn) _closeBtn.onClick.AddListener(OnClose);
            Refresh();
        }

        protected override void OnViewDisable()
        {
            if (_closeBtn) _closeBtn.onClick.RemoveAllListeners();
            ClearEntries();
        }

        private void OnRecipeUnlocked(RecipeUnlockedEvent e) => Refresh();

        private void Refresh()
        {
            ClearEntries();
            if (_entryPrefab == null || _listContainer == null) return;

            var craftStore = GetStore<CraftStore>();

            foreach (int recipeId in craftStore.UnlockedRecipeIds)
            {
                var go = Object.Instantiate(_entryPrefab, _listContainer);
                go.SetActive(true);

                var text = go.GetComponentInChildren<TextMeshProUGUI>();
                if (text)
                {
                    var recipe = GF.Config.GetTable<TbRecipe>()?.GetOrDefault(recipeId);
                    string name = recipe?.Name ?? $"#{recipeId}";
                    text.text = $"{name}";
                }

                _entries.Add(go);
            }
        }

        private void ClearEntries()
        {
            foreach (var go in _entries)
                Object.Destroy(go);
            _entries.Clear();
        }

        private void OnClose() => CloseWindow();
    }
}
