using System;
using System.Collections.Generic;
using JulyCore.Data.Save;

namespace CozyYard
{
    [Serializable]
    public class GridCellData
    {
        public int X;
        public int Y;
        public CellState State = CellState.Unexplored;
        public int OccupantId;
        public int ObstacleId;
    }

    [Serializable]
    public class GridData : ISaveData
    {
        public int Width;
        public int Height;
        public List<GridCellData> Cells = new();

        public SaveImportance Importance => SaveImportance.Normal;
    }
}
