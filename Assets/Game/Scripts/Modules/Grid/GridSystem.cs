using cfg;
using JulyArch;
using JulyCore;
using UnityEngine;

namespace CozyYard
{
    /// <summary>网格系统：管理院子的格子状态、障碍物清除、占位与坐标转换。</summary>
    public class GridSystem : GameSystemBase
    {
        private GridStore _store;
        private InventorySystem _inventorySystem;
        private TimeSystem _timeSystem;

        public int Width => _store.Width;
        public int Height => _store.Height;

        protected override void OnInitialize()
        {
            _store = GetStore<GridStore>();
            _inventorySystem = GetSystem<InventorySystem>();
            _timeSystem = GetSystem<TimeSystem>();
        }

        protected override void OnStart()
        {
            if (_store.Width > 0 && !HasAnyObstacles())
                GenerateObstacles();
        }

        private bool HasAnyObstacles()
        {
            for (int i = 0; i < _store.Width * _store.Height; i++)
            {
                var cell = _store.GetCell(i % _store.Width, i / _store.Width);
                if (cell != null && cell.State == CellState.Obstacle)
                    return true;
            }
            return false;
        }

        public GridCellData GetCell(int x, int y) => _store.GetCell(x, y);
        public bool IsInBounds(int x, int y) => _store.IsInBounds(x, y);
        public bool CanPlaceAt(int x, int y, int sizeX, int sizeY) => _store.CanPlaceAt(x, y, sizeX, sizeY);

        public bool ClearObstacle(int x, int y)
        {
            var cell = _store.GetCell(x, y);
            if (cell == null || cell.State != CellState.Obstacle) return false;

            int obstacleId = cell.ObstacleId;
            cell.ObstacleId = 0;
            _store.SetCellState(x, y, CellState.Empty);
            Publish(new GridCellChangedEvent { GridX = x, GridY = y, NewState = CellState.Empty });

            var cfg = GetObstacleConfig(obstacleId);
            if (cfg != null)
            {
                _inventorySystem.AddItem(cfg.DropItemId, cfg.DropQuantity);
                _timeSystem.ConsumeTime(cfg.ClearTime);
            }

            return true;
        }

        public bool TillSoil(int x, int y)
        {
            var cell = _store.GetCell(x, y);
            if (cell == null || cell.State != CellState.Empty) return false;

            _store.SetCellState(x, y, CellState.Soil);
            Publish(new GridCellChangedEvent { GridX = x, GridY = y, NewState = CellState.Soil });
            return true;
        }

        public bool PlaceOccupant(int x, int y, int sizeX, int sizeY, int occupantId)
        {
            if (!_store.CanPlaceAt(x, y, sizeX, sizeY)) return false;

            for (int dx = 0; dx < sizeX; dx++)
            {
                for (int dy = 0; dy < sizeY; dy++)
                {
                    _store.SetOccupant(x + dx, y + dy, occupantId);
                }
            }
            return true;
        }

        public void RemoveOccupant(int x, int y, int sizeX, int sizeY)
        {
            for (int dx = 0; dx < sizeX; dx++)
            {
                for (int dy = 0; dy < sizeY; dy++)
                {
                    _store.ClearOccupant(x + dx, y + dy);
                }
            }
        }

        public void ExpandGrid(int newWidth, int newHeight)
        {
            _store.ExpandGrid(newWidth, newHeight);
        }

        public Vector2 GridToWorldPosition(int x, int y)
        {
            return GridUtils.GridToWorld(x, y);
        }

        public Vector2Int WorldToGridPosition(Vector2 worldPos)
        {
            return GridUtils.WorldToGrid(worldPos);
        }

        private void GenerateObstacles()
        {
            var cfg = GF.Config.GetTable<TbGameConfig>();
            int seed = cfg?.ObstacleSeed ?? 42;
            float ratio = cfg?.ObstacleRatio ?? 0.3f;
            int clearRadius = cfg?.ClearRadius ?? 5;
            int maxObstacleId = cfg?.MaxObstacleId ?? 3;

            var random = new System.Random(seed);
            int totalCells = _store.Width * _store.Height;
            int obstacleCount = Mathf.RoundToInt(totalCells * ratio);

            int cx = _store.Width / 2;
            int cy = _store.Height / 2;

            for (int dx = -clearRadius; dx < clearRadius; dx++)
            {
                for (int dy = -clearRadius; dy < clearRadius; dy++)
                {
                    int nx = cx + dx;
                    int ny = cy + dy;
                    if (_store.IsInBounds(nx, ny))
                    {
                        _store.SetCellState(nx, ny, CellState.Empty);
                    }
                }
            }

            int placed = 0;
            while (placed < obstacleCount)
            {
                int x = random.Next(0, _store.Width);
                int y = random.Next(0, _store.Height);
                if (x >= cx - clearRadius && x < cx + clearRadius
                    && y >= cy - clearRadius && y < cy + clearRadius)
                    continue;

                var cell = _store.GetCell(x, y);
                if (cell.State == CellState.Unexplored)
                {
                    cell.State = CellState.Obstacle;
                    cell.ObstacleId = random.Next(1, maxObstacleId + 1);
                    placed++;
                }
            }
        }

        private Obstacle GetObstacleConfig(int obstacleId)
        {
            return GF.Config.GetTable<TbObstacle>()?.GetOrDefault(obstacleId);
        }
    }
}
