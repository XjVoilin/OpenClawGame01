using UnityEngine;
using JulyArch;
using IsleWorks.Economy;

namespace IsleWorks.Grid
{
    /// <summary>
    /// 建造系统 —— 负责放置和拆除建筑。
    /// </summary>
    public class BuildSystem : GameSystemBase
    {
        public void PlaceBuilding(Vector2Int position, int machineTypeId)
        {
            var grid = this.Query<IGridQueries>();
            var inventory = this.Query<IInventoryQueries>();

            if (grid.GetTile(position.x, position.y) != TileType.Normal)
            {
                Debug.LogError("Cannot place building: Tile type invalid.");
                return;
            }

            if (grid.GetBuilding(position.x, position.y) != 0)
            {
                Debug.LogError("Cannot place building: Tile is already occupied.");
                return;
            }

            int buildingCost = GetBuildingCost(machineTypeId);
            if (inventory.Gold < buildingCost)
            {
                Debug.LogError("Cannot place building: Not enough gold.");
                return;
            }

            this.Mutate<GridStore>(s => s.PlaceBuilding(position.x, position.y, machineTypeId));
            this.Mutate<InventoryStore>(s => s.UpdateGold(-buildingCost));

            Debug.Log($"Building {machineTypeId} placed at {position} for {buildingCost} gold.");
        }

        public void RemoveBuilding(Vector2Int position)
        {
            var grid = this.Query<IGridQueries>();
            int buildingId = grid.GetBuilding(position.x, position.y);

            if (buildingId == 0)
            {
                Debug.LogError("Cannot remove building: No building found at the specified position.");
                return;
            }

            int refundAmount = GetRefundAmount(buildingId);
            this.Mutate<GridStore>(s => s.PlaceBuilding(position.x, position.y, 0));
            this.Mutate<InventoryStore>(s => s.UpdateGold(refundAmount));

            Debug.Log($"Building {buildingId} removed at {position}. Refunded {refundAmount} gold.");
        }

        private int GetBuildingCost(int machineTypeId)
        {
            // TODO: 查询机器配表获取成本
            return 100;
        }

        private int GetRefundAmount(int buildingId)
        {
            // TODO: 查询机器配表计算退款比例
            return 50;
        }
    }
}
