using JulyArch;

namespace CozyYard
{
    public class BuildSystem : GameSystemBase
    {
        private BuildStore _store;
        private GridSystem _gridSystem;
        private InventorySystem _inventorySystem;
        private TimeSystem _timeSystem;

        // Building configs (hardcoded, matches TbBuilding)
        // Format: [id] = { sizeX, sizeY, buildTime, prerequisiteId, materialIds[], materialQtys[], refundRatio }
        private struct BuildingConfig
        {
            public int Id;
            public int SizeX;
            public int SizeY;
            public int BuildTime;
            public int PrerequisiteId;
            public int[] MaterialIds;
            public int[] MaterialQtys;
            public float RefundRatio;
        }

        private static readonly BuildingConfig[] Configs = {
            new() { Id = 1,  SizeX = 2, SizeY = 2, BuildTime = 120, PrerequisiteId = 0,  MaterialIds = new[]{1003},      MaterialQtys = new[]{20},     RefundRatio = 0.6f },
            new() { Id = 2,  SizeX = 3, SizeY = 3, BuildTime = 180, PrerequisiteId = 1,  MaterialIds = new[]{1003,1002}, MaterialQtys = new[]{30,20},  RefundRatio = 0.6f },
            new() { Id = 10, SizeX = 1, SizeY = 1, BuildTime = 30,  PrerequisiteId = 0,  MaterialIds = new[]{1003,1002}, MaterialQtys = new[]{5,3},    RefundRatio = 0.6f },
            new() { Id = 11, SizeX = 1, SizeY = 1, BuildTime = 60,  PrerequisiteId = 1,  MaterialIds = new[]{1002,1003}, MaterialQtys = new[]{10,8},   RefundRatio = 0.6f },
            new() { Id = 20, SizeX = 1, SizeY = 1, BuildTime = 30,  PrerequisiteId = 0,  MaterialIds = new[]{1003},      MaterialQtys = new[]{8},      RefundRatio = 0.6f },
            new() { Id = 30, SizeX = 1, SizeY = 1, BuildTime = 60,  PrerequisiteId = 0,  MaterialIds = new[]{1002},      MaterialQtys = new[]{15},     RefundRatio = 0.6f },
            new() { Id = 40, SizeX = 2, SizeY = 2, BuildTime = 45,  PrerequisiteId = 0,  MaterialIds = new[]{1003},      MaterialQtys = new[]{12},     RefundRatio = 0.6f },
            new() { Id = 50, SizeX = 1, SizeY = 1, BuildTime = 10,  PrerequisiteId = 0,  MaterialIds = new[]{1003},      MaterialQtys = new[]{3},      RefundRatio = 0.6f },
            new() { Id = 60, SizeX = 1, SizeY = 1, BuildTime = 20,  PrerequisiteId = 0,  MaterialIds = new[]{1003,1002}, MaterialQtys = new[]{5,3},    RefundRatio = 0.6f },
            new() { Id = 70, SizeX = 2, SizeY = 2, BuildTime = 90,  PrerequisiteId = 0,  MaterialIds = new[]{1003,1002}, MaterialQtys = new[]{15,10},  RefundRatio = 0.6f },
        };

        protected override void OnInitialize()
        {
            _store = GetStore<BuildStore>();
            _gridSystem = GetSystem<GridSystem>();
            _inventorySystem = GetSystem<InventorySystem>();
            _timeSystem = GetSystem<TimeSystem>();
        }

        public bool CanBuild(int buildingId, int x, int y)
        {
            var cfg = GetConfig(buildingId);
            if (cfg == null) return false;

            if (!_gridSystem.CanPlaceAt(x, y, cfg.Value.SizeX, cfg.Value.SizeY)) return false;

            if (cfg.Value.PrerequisiteId > 0 && !_store.HasBuilding(cfg.Value.PrerequisiteId)) return false;

            for (int i = 0; i < cfg.Value.MaterialIds.Length; i++)
            {
                if (!_inventorySystem.HasItem(cfg.Value.MaterialIds[i], cfg.Value.MaterialQtys[i])) return false;
            }

            return true;
        }

        public bool Build(int buildingId, int x, int y)
        {
            var cfg = GetConfig(buildingId);
            if (cfg == null) return false;

            if (!CanBuild(buildingId, x, y)) return false;

            if (!_inventorySystem.ConsumeItems(cfg.Value.MaterialIds, cfg.Value.MaterialQtys)) return false;

            int uid = _store.AddBuilding(buildingId, x, y, cfg.Value.SizeX, cfg.Value.SizeY);
            _gridSystem.PlaceOccupant(x, y, cfg.Value.SizeX, cfg.Value.SizeY, uid);
            _timeSystem.ConsumeTime(cfg.Value.BuildTime);

            Publish(new BuildingPlacedEvent { BuildingId = buildingId, GridX = x, GridY = y });
            return true;
        }

        public bool Demolish(int x, int y)
        {
            var building = _store.GetBuildingAt(x, y);
            if (building == null) return false;

            var cfg = GetConfig(building.BuildingId);
            _store.RemoveBuilding(building.UniqueId);
            _gridSystem.RemoveOccupant(building.GridX, building.GridY, building.SizeX, building.SizeY);

            if (cfg != null)
            {
                for (int i = 0; i < cfg.Value.MaterialIds.Length; i++)
                {
                    int refund = (int)(cfg.Value.MaterialQtys[i] * cfg.Value.RefundRatio);
                    if (refund > 0)
                    {
                        _inventorySystem.AddItem(cfg.Value.MaterialIds[i], refund);
                    }
                }
            }

            Publish(new BuildingRemovedEvent { GridX = x, GridY = y });
            return true;
        }

        public BuildingInstance GetBuildingAt(int x, int y) => _store.GetBuildingAt(x, y);
        public bool HasBuilding(int buildingId) => _store.HasBuilding(buildingId);

        private BuildingConfig? GetConfig(int buildingId)
        {
            for (int i = 0; i < Configs.Length; i++)
            {
                if (Configs[i].Id == buildingId) return Configs[i];
            }
            return null;
        }
    }
}
