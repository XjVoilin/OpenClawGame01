using UnityEngine;
using JulyArch;
using IsleWorks.Data;
using IsleWorks.Stores;

namespace IsleWorks.Systems
{
    /// <summary>
    /// 建造系统 —— 负责放置和拆除建筑。
    /// </summary>
    public class BuildSystem : GameSystemBase
    {
        [Inject] private GridStore _gridStore;
        [Inject] private InventoryStore _inventoryStore;

        /// <summary>
        /// 放置建筑。
        /// </summary>
        public void PlaceBuilding(Vector2Int position, int machineTypeId)
        {
            // 验证位置是否合法
            if (!_gridStore.GetTile(position.x, position.y).Equals(TileType.Normal))
            {
                Debug.LogError("Cannot place building: Tile type invalid.");
                return;
            }

            if (_gridStore.GetBuilding(position.x, position.y) != 0)
            {
                Debug.LogError("Cannot place building: Tile is already occupied.");
                return;
            }

            // 检查金币是否足够
            int buildingCost = GetBuildingCost(machineTypeId);
            if (_inventoryStore.Gold < buildingCost)
            {
                Debug.LogError("Cannot place building: Not enough gold.");
                return;
            }

            // 执行建造操作
            _gridStore.PlaceBuilding(position.x, position.y, machineTypeId);
            _inventoryStore.UpdateGold(-buildingCost);

            Debug.Log($"Building {machineTypeId} placed at {position} for {buildingCost} gold.");
        }

        /// <summary>
        /// 拆除建筑。
        /// </summary>
        public void RemoveBuilding(Vector2Int position)
        {
            int buildingId = _gridStore.GetBuilding(position.x, position.y);

            if (buildingId == 0)
            {
                Debug.LogError("Cannot remove building: No building found at the specified position.");
                return;
            }

            // 执行拆除操作
            int refundAmount = GetRefundAmount(buildingId);
            _gridStore.PlaceBuilding(position.x, position.y, 0);
            _inventoryStore.UpdateGold(refundAmount);

            Debug.Log($"Building {buildingId} removed at {position}. Refunded {refundAmount} gold.");
        }

        /// <summary>
        /// 获取建筑成本。
        /// </summary>
        private int GetBuildingCost(int machineTypeId)
        {
            // TODO: 查询机器配表获取成本
            return 100; // 示例成本
        }

        /// <summary>
        /// 获取拆除退款金额。
        /// </summary>
        private int GetRefundAmount(int buildingId)
        {
            // TODO: 查询机器配表计算退款比例
            return 50; // 示例退款
        }
    }
}