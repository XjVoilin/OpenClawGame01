using System;
using System.Collections.Generic;
using UnityEngine;

namespace IsleWorks.Grid
{
    public enum TileType : byte
    {
        Locked,     // 未购买（迷雾）
        Normal,     // 普通地块
        Water,      // 水域（需填海）
        Mountain,   // 山地（需平整）
        Port        // 港口（卖产品）
    }

    /// <summary>
    /// 网格数据 —— GridStore 的核心存储结构。
    /// 使用扁平数组，索引 = x + y * Width，缓存友好。
    /// </summary>
    public class GridData
    {
        public int Width;
        public int Height;
        public TileType[] Tiles;            // 地块类型
        public int[] BuildingIds;           // 每格的建筑 ID（0 = 空）

        /// <summary>
        /// 初始化网格数据。
        /// </summary>
        public GridData() { }

        public GridData(int width, int height)
        {
            Width = width;
            Height = height;
            Tiles = new TileType[width * height];
            BuildingIds = new int[width * height];
        }

        /// <summary>
        /// 索引地块类型。
        /// </summary>
        public TileType GetTileType(int x, int y)
        {
            return Tiles[x + y * Width];
        }

        /// <summary>
        /// 设置地块类型。
        /// </summary>
        public void SetTileType(int x, int y, TileType type)
        {
            Tiles[x + y * Width] = type;
        }

        /// <summary>
        /// 获取建筑 ID。
        /// </summary>
        public int GetBuildingId(int x, int y)
        {
            return BuildingIds[x + y * Width];
        }

        /// <summary>
        /// 设置建筑 ID。
        /// </summary>
        public void SetBuildingId(int x, int y, int buildingId)
        {
            BuildingIds[x + y * Width] = buildingId;
        }
    }
}