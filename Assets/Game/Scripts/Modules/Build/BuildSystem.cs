using cfg;
using JulyArch;
using JulyCore;

namespace CozyYard
{
    /// <summary>建造系统：验证并执行建筑的放置与拆除，消耗材料和时间，拆除返还部分材料。</summary>
    public class BuildSystem : GameSystemBase
    {
        private const float RefundRatio = 0.6f;

        private BuildStore _store;
        private GridSystem _gridSystem;
        private InventorySystem _inventorySystem;
        private TimeSystem _timeSystem;

        protected override void OnInitialize()
        {
            _store = GetStore<BuildStore>();
            _gridSystem = GetSystem<GridSystem>();
            _inventorySystem = GetSystem<InventorySystem>();
            _timeSystem = GetSystem<TimeSystem>();
        }

        public bool CanBuild(int buildingId, int x, int y)
        {
            if (!CanAfford(buildingId)) return false;

            var cfg = GF.Config.GetTable<TbBuilding>().Get(buildingId);
            return _gridSystem.CanPlaceAt(x, y, cfg.SizeX, cfg.SizeY);
        }

        public bool CanAfford(int buildingId)
        {
            var cfg = GF.Config.GetTable<TbBuilding>().Get(buildingId);
            if (cfg == null) return false;

            if (cfg.PrerequisiteId > 0 && !_store.HasBuilding(cfg.PrerequisiteId)) return false;

            for (int i = 0; i < cfg.Materials.Count; i++)
            {
                if (!_inventorySystem.HasItem(cfg.Materials[i], cfg.MaterialQtys[i])) return false;
            }

            return true;
        }

        public bool Build(int buildingId, int x, int y)
        {
            var cfg = GF.Config.GetTable<TbBuilding>().Get(buildingId);
            if (cfg == null) return false;

            if (!CanBuild(buildingId, x, y)) return false;

            if (!_inventorySystem.ConsumeItems(cfg.Materials.ToArray(), cfg.MaterialQtys.ToArray())) return false;

            int uid = _store.AddBuilding(buildingId, x, y, cfg.SizeX, cfg.SizeY);
            _gridSystem.PlaceOccupant(x, y, cfg.SizeX, cfg.SizeY, uid);
            _timeSystem.ConsumeTime(cfg.BuildTime);

            Publish(new BuildingPlacedEvent { BuildingId = buildingId, GridX = x, GridY = y });
            return true;
        }

        public bool Demolish(int x, int y)
        {
            var building = _store.GetBuildingAt(x, y);
            if (building == null) return false;

            var cfg = GF.Config.GetTable<TbBuilding>().Get(building.BuildingId);
            _store.RemoveBuilding(building.UniqueId);
            _gridSystem.RemoveOccupant(building.GridX, building.GridY, building.SizeX, building.SizeY);

            if (cfg != null)
            {
                for (int i = 0; i < cfg.Materials.Count; i++)
                {
                    int refund = (int)(cfg.MaterialQtys[i] * RefundRatio);
                    if (refund > 0)
                    {
                        _inventorySystem.AddItem(cfg.Materials[i], refund);
                    }
                }
            }

            Publish(new BuildingRemovedEvent { GridX = x, GridY = y });
            return true;
        }

        public BuildingInstance GetBuildingAt(int x, int y) => _store.GetBuildingAt(x, y);
        public bool HasBuilding(int buildingId) => _store.HasBuilding(buildingId);
    }
}
