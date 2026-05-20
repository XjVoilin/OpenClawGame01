namespace CozyYard
{
    public class GridStore : SavableStoreBase<GridData>, IGridQueries
    {
        protected override string SaveKey => SaveKeys.GridData;

        public int Width => Data.Width;
        public int Height => Data.Height;

        public GridCellData GetCell(int x, int y)
        {
            if (!IsInBounds(x, y)) return null;
            return Data.Cells[y * Data.Width + x];
        }

        public bool IsInBounds(int x, int y)
        {
            return x >= 0 && x < Data.Width && y >= 0 && y < Data.Height;
        }

        public bool IsCellEmpty(int x, int y)
        {
            var cell = GetCell(x, y);
            return cell != null && cell.State == CellState.Empty && cell.OccupantId == 0;
        }

        public bool IsCellBuildable(int x, int y)
        {
            var cell = GetCell(x, y);
            if (cell == null) return false;
            return (cell.State == CellState.Empty || cell.State == CellState.Paved)
                   && cell.OccupantId == 0;
        }

        public bool CanPlaceAt(int x, int y, int sizeX, int sizeY)
        {
            for (int dx = 0; dx < sizeX; dx++)
            {
                for (int dy = 0; dy < sizeY; dy++)
                {
                    if (!IsCellBuildable(x + dx, y + dy)) return false;
                }
            }
            return true;
        }

        public void SetCellState(int x, int y, CellState state)
        {
            var cell = GetCell(x, y);
            if (cell == null) return;
            cell.State = state;
            MarkDirty();
        }

        public void SetOccupant(int x, int y, int occupantId)
        {
            var cell = GetCell(x, y);
            if (cell == null) return;
            cell.OccupantId = occupantId;
            MarkDirty();
        }

        public void ClearOccupant(int x, int y)
        {
            SetOccupant(x, y, 0);
        }

        public void InitializeGrid(int width, int height)
        {
            Data.Width = width;
            Data.Height = height;
            Data.Cells.Clear();
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Data.Cells.Add(new GridCellData { X = x, Y = y, State = CellState.Unexplored });
                }
            }
            MarkDirty();
        }

        public void ExpandGrid(int newWidth, int newHeight)
        {
            if (newWidth <= Data.Width && newHeight <= Data.Height) return;

            int oldW = Data.Width;
            int oldH = Data.Height;
            var oldCells = Data.Cells;

            Data.Width = newWidth;
            Data.Height = newHeight;
            Data.Cells = new System.Collections.Generic.List<GridCellData>();

            for (int y = 0; y < newHeight; y++)
            {
                for (int x = 0; x < newWidth; x++)
                {
                    if (x < oldW && y < oldH)
                    {
                        var cell = oldCells[y * oldW + x];
                        cell.X = x;
                        cell.Y = y;
                        Data.Cells.Add(cell);
                    }
                    else
                    {
                        Data.Cells.Add(new GridCellData
                        {
                            X = x,
                            Y = y,
                            State = CellState.Empty,
                            OccupantId = 0,
                            ObstacleId = 0
                        });
                    }
                }
            }
            MarkDirty();
        }
    }
}
