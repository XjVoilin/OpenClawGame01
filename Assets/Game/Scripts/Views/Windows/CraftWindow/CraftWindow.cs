using System.Collections.Generic;
using cfg;
using Cysharp.Threading.Tasks;
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
            RefreshAsync();
        }

        protected override void OnViewDisable()
        {
            if (_closeBtn) _closeBtn.onClick.RemoveAllListeners();
            ClearEntries();
        }

        private void OnCraftChanged(CraftCompletedEvent e) => RefreshAsync();
        private void OnInventoryChanged(InventoryChangedEvent e) => RefreshAsync();

        private void RefreshAsync()
        {
            ClearEntries();
            if (_entryPrefab == null || _listContainer == null) return;

            var craftStore = GetStore<CraftStore>();
            var craftSystem = GetSystem<CraftSystem>();
            var tbItem = GF.Config.GetTable<TbItem>();
            var tbRecipe = GF.Config.GetTable<TbRecipe>();

            foreach (var recipeId in craftStore.UnlockedRecipeIds)
            {
                var entry = Instantiate(_entryPrefab, _listContainer);
                entry.gameObject.SetActive(true);

                var recipe = tbRecipe.GetOrDefault(recipeId);
                var nameKey = recipe.NameKey;
                var canCraft = craftSystem.CanCraft(recipeId);
                var id = recipeId;
                var outputItem = tbItem.GetOrDefault(recipe.OutputItemId);
                entry.Setup(
                    GF.Localization.Get(nameKey),
                    canCraft,
                    () => OnCraft(craftSystem, id),
                    outputItem.IconSprite
                );

                _entries.Add(entry);
            }
        }

        private void OnCraft(CraftSystem craftSystem, int recipeId)
        {
            craftSystem.StartCraft(recipeId);
            RefreshAsync();
        }

        private void ClearEntries()
        {
            foreach (var entry in _entries)
            {
                entry.Cleanup();
                Destroy(entry.gameObject);
            }
            _entries.Clear();
        }

        protected override void OnClose() => CloseWindow();
    }
}
