using System.Collections.Generic;
using cfg;
using JulyCore;
using UnityEngine;

namespace CozyYard
{
    public class RecipeBookWindow : GameUIView
    {
        [SerializeField] private Transform _listContainer;
        [SerializeField] private RecipeBookEntry _entryPrefab;
        [SerializeField] private UISmartButton _closeBtn;

        private readonly List<RecipeBookEntry> _entries = new();

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
                var entry = Object.Instantiate(_entryPrefab, _listContainer);
                entry.gameObject.SetActive(true);

                var recipe = GF.Config.GetTable<TbRecipe>()?.GetOrDefault(recipeId);
                string nameKey = recipe?.NameKey ?? $"#{recipeId}";
                entry.Setup(GF.Localization.Get(nameKey));

                _entries.Add(entry);
            }
        }

        private void ClearEntries()
        {
            foreach (var entry in _entries)
                Object.Destroy(entry.gameObject);
            _entries.Clear();
        }

        private void OnClose() => CloseWindow();
    }
}
