using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CozyYard
{
    public class CraftWindow : GameUIView
    {
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

            var craftStore = GetStore<CraftStore>();
            var craftSystem = GetSystem<CraftSystem>();

            foreach (int recipeId in craftStore.UnlockedRecipeIds)
            {
                var go = Object.Instantiate(_entryPrefab, _listContainer);
                go.SetActive(true);

                var texts = go.GetComponentsInChildren<TextMeshProUGUI>();
                if (texts.Length > 0)
                {
                    string name = CfgHelper.GetRecipeName(recipeId);
                    string inputs = CfgHelper.FormatRecipeInputs(recipeId);
                    string output = CfgHelper.FormatRecipeOutput(recipeId);
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
