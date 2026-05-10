using System;
using UnityEngine;
using IsleWorks.Data;
using JulyArch;

namespace IsleWorks.Stores
{
    /// <summary>
    /// 网格存储 —— 持有网格相关数据，提供操作接口。
    /// </summary>
    public class GridStore : StoreBase
    {
        private GridData _gridData;

        public int Width => _gridData.Width;
        public int Height => _gridData.Height;

        /// <summary>
        /// 初始化网格存储。
        /// </summary>
        public void Initialize(int width, int height)
        {
            _gridData = new GridData(width, height);
            Debug.Log($"Grid initialized: {width}×{height}");
        }

        /// <summary>
        /// 查询地块类型。
        /// </summary>
        public TileType GetTile(int x, int y)
        {
            return _gridData.GetTileType(x, y);
        }

        /// <summary>
        /// 查询建筑 ID。
        /// </summary>
        public int GetBuilding(int x, int y)
        {
            return _gridData.GetBuildingId(x, y);
        }

        /// <summary>
        /// 放置建筑。
        /// </summary>
        public void PlaceBuilding(int x, int y, int buildingId)
        {
            _gridData.SetBuildingId(x, y, buildingId);
            Debug.Log($"Building {buildingId} placed at ({x}, {y})");
        }

        /// <summary>
        /// 更新地块类型。
        /// </summary>
        public void UpdateTileType(int x, int y, TileType tileType)
        {
            _gridData.SetTileType(x, y, tileType);
            Debug.Log($"Tile at ({x}, {y}) updated to {tileType}");
        }
    }
}