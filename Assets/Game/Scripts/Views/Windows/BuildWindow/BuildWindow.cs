using System.Collections.Generic;
using cfg;
using JulyCore;
using UnityEngine;

namespace CozyYard
{
    public class BuildWindow : GameUIView
    {
        [SerializeField] private Transform _listContainer;
        [SerializeField] private BuildEntry _entryPrefab;
        [SerializeField] private UISmartButton _closeBtn;

        private readonly List<BuildEntry> _entries = new();

        protected override void OnViewEnable()
        {
            Subscribe<BuildingPlacedEvent>(OnBuildingPlaced);
            Subscribe<PlacementCancelledEvent>(OnPlacementCancelled);
            if (_closeBtn) _closeBtn.onClick.AddListener(OnClose);
            Refresh();
        }

        protected override void OnViewDisable()
        {
            if (_closeBtn) _closeBtn.onClick.RemoveAllListeners();
            ClearEntries();
        }

        private void OnBuildingPlaced(BuildingPlacedEvent e) => Refresh();
        private void OnPlacementCancelled(PlacementCancelledEvent e) => Refresh();

        private void Refresh()
        {
            ClearEntries();
            if (_entryPrefab == null || _listContainer == null) return;

            var buildSystem = GetSystem<BuildSystem>();
            var tbBuilding = GF.Config.GetTable<TbBuilding>();
            if (tbBuilding == null) return;

            foreach (var (id, cfg) in tbBuilding.DataMap)
            {
                var entry = Object.Instantiate(_entryPrefab, _listContainer);
                entry.gameObject.SetActive(true);

                bool canAfford = buildSystem.CanAfford(id);
                int buildingId = id;
                entry.Setup(
                    GF.Localization.Get(cfg.NameKey),
                    canAfford,
                    () => OnBuild(buildingId)
                );

                _entries.Add(entry);
            }
        }

        private void OnBuild(int buildingId)
        {
            var buildSystem = GetSystem<BuildSystem>();
            if (!buildSystem.CanAfford(buildingId)) return;

            Publish(new EnterPlacementModeEvent { BuildingId = buildingId });
            CloseWindow();
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
