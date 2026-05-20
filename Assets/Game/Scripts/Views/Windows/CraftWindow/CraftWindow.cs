using System.Collections.Generic;
using JulyArch;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CozyYard
{
    public class CraftWindow : GameUIView
    {
        private static readonly Dictionary<int, string> RecipeNames = new()
        {
            { 1, "桂花干" }, { 2, "糯米粉" }, { 3, "桂花糕" }, { 4, "辣炒蛋" },
            { 5, "清炒白菜" }, { 6, "萝卜干" }, { 7, "菊花干" }, { 8, "菊花茶" }, { 9, "柿饼" },
        };

        private static readonly Dictionary<int, string> RecipeInputs = new()
        {
            { 1, "#3006×3" }, { 2, "#3003×2" }, { 3, "#4001×2, #4002×2" },
            { 4, "#3101×1, #3005×1" }, { 5, "#3001×2" }, { 6, "#3002×2" },
            { 7, "#3004×3" }, { 8, "#4004×2" }, { 9, "#3007×3" },
        };

        private static readonly Dictionary<int, string> RecipeOutputs = new()
        {
            { 1, "#4001×2" }, { 2, "#4002×2" }, { 3, "#5001×1" }, { 4, "#5002×1" },
            { 5, "#5003×1" }, { 6, "#4003×2" }, { 7, "#4004×2" }, { 8, "#5004×1" }, { 9, "#5005×2" },
        };

        [SerializeField] private Transform _listContainer;
        [SerializeField] private GameObject _entryPrefab;
        [SerializeField] private Button _closeBtn;

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

            var craftQueries = this.Query<ICraftQueries>();
            var craftSystem = GetSystem<CraftSystem>();

            foreach (int recipeId in craftQueries.UnlockedRecipeIds)
            {
                var go = Object.Instantiate(_entryPrefab, _listContainer);
                go.SetActive(true);

                var texts = go.GetComponentsInChildren<TextMeshProUGUI>();
                if (texts.Length > 0)
                {
                    string name = RecipeNames.TryGetValue(recipeId, out var n) ? n : $"#{recipeId}";
                    string inputs = RecipeInputs.TryGetValue(recipeId, out var inp) ? inp : "?";
                    string output = RecipeOutputs.TryGetValue(recipeId, out var outp) ? outp : "?";
                    texts[0].text = $"{name}\n材料: {inputs}\n产出: {output}";
                }

                var craftBtn = go.GetComponentInChildren<Button>();
                if (craftBtn)
                {
                    bool canCraft = craftSystem.CanCraft(recipeId);
                    craftBtn.interactable = canCraft;
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
                var btn = go.GetComponentInChildren<Button>();
                if (btn) btn.onClick.RemoveAllListeners();
                Object.Destroy(go);
            }
            _entries.Clear();
        }

        private void OnClose() => CloseWindow();
    }
}
