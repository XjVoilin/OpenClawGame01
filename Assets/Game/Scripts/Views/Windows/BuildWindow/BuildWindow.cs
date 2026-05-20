using System.Collections.Generic;
using JulyArch;
using JulyCore;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CozyYard
{
    public class BuildWindow : GameView
    {
        private static readonly int[] BuildingIds = { 1, 2, 10, 11, 20, 30, 40, 50, 60, 70 };

        private static readonly Dictionary<int, string> BuildingNames = new()
        {
            { 1, "茅草屋" }, { 2, "土砖房" }, { 10, "野外篝火" }, { 11, "土灶" },
            { 20, "简易竹架" }, { 30, "石磨" }, { 40, "露天围栏" }, { 50, "围栏" },
            { 60, "饲料槽" }, { 70, "仓库" },
        };

        private static readonly Dictionary<int, string> MaterialCosts = new()
        {
            { 1, "#1003×20" }, { 2, "#1003×30, #1002×20" }, { 10, "#1003×5, #1002×3" },
            { 11, "#1002×10, #1003×8" }, { 20, "#1003×8" }, { 30, "#1002×15" },
            { 40, "#1003×12" }, { 50, "#1003×3" }, { 60, "#1003×5, #1002×3" },
            { 70, "#1003×15, #1002×10" },
        };

        private const int TestGridX = 0;
        private const int TestGridY = 0;

        [SerializeField] private Transform _listContainer;
        [SerializeField] private GameObject _entryPrefab;
        [SerializeField] private Button _closeBtn;

        private readonly List<GameObject> _entries = new();

        public override IGameContext GetArchitecture() => AppArch.Context;

        protected override void OnViewEnable()
        {
            this.Subscribe<BuildingPlacedEvent>(OnBuildingPlaced);
            if (_closeBtn) _closeBtn.onClick.AddListener(OnClose);
            Refresh();
        }

        protected override void OnViewDisable()
        {
            if (_closeBtn) _closeBtn.onClick.RemoveAllListeners();
            ClearEntries();
        }

        private void OnBuildingPlaced(BuildingPlacedEvent e) => Refresh();

        private void Refresh()
        {
            ClearEntries();
            if (_entryPrefab == null || _listContainer == null) return;

            var buildSystem = this.GetSystem<BuildSystem>();

            foreach (int id in BuildingIds)
            {
                var go = Object.Instantiate(_entryPrefab, _listContainer);
                go.SetActive(true);

                var texts = go.GetComponentsInChildren<TextMeshProUGUI>();
                if (texts.Length > 0)
                {
                    string name = BuildingNames.TryGetValue(id, out var n) ? n : $"#{id}";
                    string cost = MaterialCosts.TryGetValue(id, out var c) ? c : "?";
                    texts[0].text = $"{name} ({cost})";
                }

                var buildBtn = go.GetComponentInChildren<Button>();
                if (buildBtn)
                {
                    bool canBuild = buildSystem.CanBuild(id, TestGridX, TestGridY);
                    buildBtn.interactable = canBuild;
                    int buildingId = id;
                    buildBtn.onClick.AddListener(() => OnBuild(buildSystem, buildingId));
                }

                _entries.Add(go);
            }
        }

        private void OnBuild(BuildSystem buildSystem, int buildingId)
        {
            buildSystem.Build(buildingId, TestGridX, TestGridY);
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

        private void OnClose() => gameObject.SetActive(false);
    }
}
