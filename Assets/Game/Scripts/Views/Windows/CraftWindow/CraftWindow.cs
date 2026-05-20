using System.Collections.Generic;
using cfg;
using JulyCore;
using TMPro;
using UnityEngine;

namespace CozyYard
{
    public class CraftWindow : GameUIView
    {
        [SerializeField] private Transform _listContainer;
        [SerializeField] private GameObject _entryPrefab;
        [SerializeField] private UISmartButton _closeBtn;

        private readonly List<GameObject> _entries = new();

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
                var go = Object.Instantiate(_entryPrefab, _listContainer);
                go.SetActive(true);

                var texts = go.GetComponentsInChildren<TextMeshProUGUI>();
                if (texts.Length > 0)
                {
                    var recipe = GF.Config.GetTable<TbRecipe>()?.GetOrDefault(recipeId);
                    string name = recipe?.Name ?? $"#{recipeId}";
                    texts[0].text = $"{name}";
                }

                var craftBtn = go.GetComponentInChildren<UISmartButton>();
                if (craftBtn)
                {
                    bool canCraft = craftSystem.CanCraft(recipeId);
                    craftBtn.SetInteractable(canCraft);
                    int id = recipeId;
                    craftBtn.onClick.AddListener(() => OnCraft(craftSystem, id));
                }

                _entries.Add(go);
            }
        }

        private void OnCraft(CraftSystem craftSystem, int recipeId)
        {
            craftSystem.StartCraft(recipeId);
            Refresh();
        }

        private void ClearEntries()
        {
            foreach (var go in _entries)
            {
                var btn = go.GetComponentInChildren<UISmartButton>();
                if (btn) btn.onClick.RemoveAllListeners();
                Object.Destroy(go);
            }
            _entries.Clear();
        }

        private void OnClose() => CloseWindow();
    }
}
