using System.Collections.Generic;
using cfg;
using JulyCore;
using UnityEngine;

namespace CozyYard
{
    public class CraftWindow : GameUIView
    {
        [SerializeField] private Transform _listContainer;
        [SerializeField] private CraftEntry _entryPrefab;
        [SerializeField] private UISmartButton _closeBtn;

        private readonly List<CraftEntry> _entries = new();

        protected override void OnViewEnable()
        {
            Subscribe<CraftCompletedEvent>(OnCraftChanged);
            Subscribe<InventoryChangedEvent>(OnInventoryChanged);
            if (_closeBtn) _closeBtn.onClick.AddListener(OnClose);
            Refresh();
        }

        protected override void OnViewDisable()
        {
            if (_closeBtn) _closeBtn.onClick.RemoveAllListeners();
            ClearEntries();
        }

        private void OnCraftChanged(CraftCompletedEvent e) => Refresh();
        private void OnInventoryChanged(InventoryChangedEvent e) => Refresh();

        private void Refresh()
        {
            ClearEntries();
            if (_entryPrefab == null || _listContainer == null) return;

            var craftStore = GetStore<CraftStore>();
            var craftSystem = GetSystem<CraftSystem>();

            foreach (int recipeId in craftStore.UnlockedRecipeIds)
            {
                var entry = Object.Instantiate(_entryPrefab, _listContainer);
                entry.gameObject.SetActive(true);

                var recipe = GF.Config.GetTable<TbRecipe>()?.GetOrDefault(recipeId);
                string nameKey = recipe?.NameKey ?? $"#{recipeId}";
                bool canCraft = craftSystem.CanCraft(recipeId);
                int id = recipeId;

                entry.Setup(
                    GF.Localization.Get(nameKey),
                    canCraft,
                    () => OnCraft(craftSystem, id)
                );

                _entries.Add(entry);
            }
        }

        private void OnCraft(CraftSystem craftSystem, int recipeId)
        {
            craftSystem.StartCraft(recipeId);
            Refresh();
        }

        private void ClearEntries()
        {
            foreach (var entry in _entries)
            {
                entry.Cleanup();
                Object.Destroy(entry.gameObject);
            }
            _entries.Clear();
        }

        private void OnClose() => CloseWindow();
    }
}
