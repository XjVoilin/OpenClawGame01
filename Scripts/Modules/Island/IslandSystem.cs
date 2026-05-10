using System;
using UnityEngine;
using JulyArch;
using IsleWorks.Data;
using IsleWorks.Stores;

namespace IsleWorks.Systems
{
    /// <summary>
    /// 岛屿扩展系统，负责地块的解锁与初始配置。
    /// </summary>
    public class IslandSystem : GameSystemBase
    {
        [Inject] private GridStore _gridStore;
        [Inject] private InventoryStore _inventoryStore;

        /// <summary>
        /// 解锁新的岛屿地块。
        /// </summary>
        public void UnlockTile(Vector2Int position, int cost)
        {
            // 检查地块是否已解锁
            if (_gridStore.GetTile(position.x, position.y) != TileType.Locked)
            {
                Debug.LogError("Cannot unlock tile: Already unlocked or invalid.");
                return;
            }

            // 检查金币是否足够
            if (_inventoryStore.Gold < cost)
            {
                Debug.LogError("Cannot unlock tile: Not enough gold.");
                return;
            }

            // 解锁地块并扣金币
            _gridStore.UpdateTileType(position.x, position.y, TileType.Normal);
            _inventoryStore.UpdateGold(-cost);

            Debug.Log($"Tile at {position} unlocked for {cost} gold.");
        }

        /// <summary>
        /// 生成随机地图。
        /// </summary>
        public void GenerateRandomMap(int width, int height, int lockedRatio)
        {
            var random = new System.Random();

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    TileType type = random.Next(100) < lockedRatio ? TileType.Locked : TileType.Normal;
                    _gridStore.UpdateTileType(x, y, type);
                }
            }

            Debug.Log("Random map generated.");
        }
    }
}