using System.Collections.Generic;
using cfg;
using Cysharp.Threading.Tasks;
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
            RefreshAsync().Forget();
        }

        protected override void OnViewDisable()
        {
            if (_closeBtn) _closeBtn.onClick.RemoveAllListeners();
            ClearEntries();
        }

        private void OnBuildingPlaced(BuildingPlacedEvent e) => RefreshAsync().Forget();
        private void OnPlacementCancelled(PlacementCancelledEvent e) => RefreshAsync().Forget();

        private async UniTaskVoid RefreshAsync()
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
                string displayName = $"{GF.Localization.Get(cfg.NameKey)} ({cfg.SizeX}×{cfg.SizeY})";

                Sprite icon = null;
                if (!string.IsNullOrEmpty(cfg.IconSprite))
                    icon = await SpriteLoader.LoadAsync(cfg.IconSprite);

                entry.Setup(displayName, canAfford, () => OnBuild(buildingId), icon);
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
