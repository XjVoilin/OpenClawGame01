using JulyArch;
using UnityEngine;

namespace CozyYard
{
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

        public void InitializeNewGrid(int width, int height)
        {
            _store.InitializeGrid(width, height);
            GenerateObstacles();
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

            switch (obstacleId)
            {
                case 1:
                    _inventorySystem.AddItem(1001, 2);
                    _timeSystem.ConsumeTime(15);
                    break;
                case 2:
                    _inventorySystem.AddItem(1002, 3);
                    _timeSystem.ConsumeTime(30);
                    break;
                case 3:
                    _inventorySystem.AddItem(1003, 5);
                    _timeSystem.ConsumeTime(60);
                    break;
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

        public Vector2 GridToWorldPosition(int x, int y)
        {
            return IsometricUtils.GridToWorld(x, y);
        }

        public Vector2Int WorldToGridPosition(Vector2 worldPos)
        {
            return IsometricUtils.WorldToGrid(worldPos);
        }

        private void GenerateObstacles()
        {
            var random = new System.Random(42);
            int totalCells = _store.Width * _store.Height;
            int obstacleCount = Mathf.RoundToInt(totalCells * 0.4f);

            int placed = 0;
            while (placed < obstacleCount)
            {
                int x = random.Next(0, _store.Width);
                int y = random.Next(0, _store.Height);
                var cell = _store.GetCell(x, y);
                if (cell.State == CellState.Unexplored)
                {
                    cell.State = CellState.Obstacle;
                    cell.ObstacleId = random.Next(1, 4);
                    placed++;
                }
            }

            int cx = _store.Width / 2;
            int cy = _store.Height / 2;
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    int nx = cx + dx;
                    int ny = cy + dy;
                    if (_store.IsInBounds(nx, ny))
                    {
                        _store.SetCellState(nx, ny, CellState.Empty);
                        _store.GetCell(nx, ny).ObstacleId = 0;
                    }
                }
            }
        }
    }
}
