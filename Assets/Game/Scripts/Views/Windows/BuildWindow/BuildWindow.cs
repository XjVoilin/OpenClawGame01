using System.Collections.Generic;
using cfg;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CozyYard
{
    public class BuildWindow : GameUIView
    {
        private const int TestGridX = 0;
        private const int TestGridY = 0;

        [SerializeField] private Transform _listContainer;
        [SerializeField] private GameObject _entryPrefab;
        [SerializeField] private Button _closeBtn;

        private readonly List<GameObject> _entries = new();

        protected override void OnViewEnable()
        {
            Subscribe<BuildingPlacedEvent>(OnBuildingPlaced);
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

            var buildSystem = GetSystem<BuildSystem>();
            var tbBuilding = CfgTable.Tables?.TbBuilding;
            if (tbBuilding == null) return;

            foreach (var (id, cfg) in tbBuilding.DataMap)
            {
                var go = Object.Instantiate(_entryPrefab, _listContainer);
                go.SetActive(true);

                var texts = go.GetComponentsInChildren<TextMeshProUGUI>();
                if (texts.Length > 0)
                {
                    string cost = CfgHelper.FormatMaterials(cfg.Materials, cfg.MaterialQtys);
                    texts[0].text = $"{cfg.Name} ({cost})";
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

        private void OnClose() => CloseWindow();
    }
}
