using System;
using UnityEngine;
using JulyArch;

namespace IsleWorks.Grid
{
    /// <summary>
    /// 网格存储 —— 持有网格相关数据，提供操作接口。
    /// </summary>
    public class GridStore : StoreBase<GridData>, IGridQueries
    {
        public int Width => Data.Width;
        public int Height => Data.Height;

        public void Initialize(int width, int height)
        {
            Data = new GridData(width, height);
            Debug.Log($"Grid initialized: {width}×{height}");
        }

        public TileType GetTile(int x, int y)
        {
            return Data.GetTileType(x, y);
        }

        public int GetBuilding(int x, int y)
        {
            return Data.GetBuildingId(x, y);
        }

        public void PlaceBuilding(int x, int y, int buildingId)
        {
            Data.SetBuildingId(x, y, buildingId);
            Debug.Log($"Building {buildingId} placed at ({x}, {y})");
        }

        public void UpdateTileType(int x, int y, TileType tileType)
        {
            Data.SetTileType(x, y, tileType);
            Debug.Log($"Tile at ({x}, {y}) updated to {tileType}");
        }
    }
}