using UnityEngine;
using JulyArch;
using JulyCore;
using IsleWorks.Grid;
using IsleWorks.Economy;

namespace IsleWorks.Island
{
    /// <summary>
    /// 岛屿扩展系统，负责地块的解锁与初始配置。
    /// </summary>
    public class IslandSystem : GameSystemBase
    {
        public void UnlockTile(Vector2Int position, int cost)
        {
            var grid = this.Query<IGridQueries>();
            var inventory = this.Query<IInventoryQueries>();

            if (grid.GetTile(position.x, position.y) != TileType.Locked)
            {
                GF.LogError("Cannot unlock tile: Already unlocked or invalid.");
                return;
            }

            if (inventory.Gold < cost)
            {
                GF.LogError("Cannot unlock tile: Not enough gold.");
                return;
            }

            this.Mutate<GridStore>(s => s.UpdateTileType(position.x, position.y, TileType.Normal));
            this.Mutate<InventoryStore>(s => s.UpdateGold(-cost));

            GF.Log($"Tile at {position} unlocked for {cost} gold.");
        }

        public void GenerateRandomMap(int width, int height, int lockedRatio)
        {
            var random = new System.Random();

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    TileType type = random.Next(100) < lockedRatio ? TileType.Locked : TileType.Normal;
                    this.Mutate<GridStore>(s => s.UpdateTileType(x, y, type));
                }
            }

            GF.Log("Random map generated.");
        }
    }
}
