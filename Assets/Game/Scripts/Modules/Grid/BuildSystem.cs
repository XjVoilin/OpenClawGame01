using IsleWorks.Economy;
using IsleWorks.Production;
using JulyArch;
using JulyCore;
using UnityEngine;

namespace IsleWorks.Grid
{
    /// <summary>
    /// 建造系统 —— 负责放置和拆除建筑、传送带。
    /// </summary>
    public class BuildSystem : GameSystemBase
    {
        public void PlaceBuilding(Vector2Int position, int machineTypeId)
        {
            var grid = this.Query<IGridQueries>();
            var inventory = this.Query<IInventoryQueries>();

            var size = MachineConfigLoader.GetSize(machineTypeId);
            int cost = MachineConfigLoader.GetCost(machineTypeId);

            if (!grid.CanPlace(position, size))
            {
                GF.LogError($"Cannot place building at {position}: area not free or invalid tile.");
                return;
            }

            if (inventory.Gold < cost)
            {
                GF.LogError("Cannot place building: not enough gold.");
                return;
            }

            int inputSlotSize = MachineConfigLoader.GetInputSlotSize(machineTypeId);
            int buildingId = 0;

            this.Mutate<GridStore>(store =>
            {
                buildingId = store.AllocateBuildingId();
                var machine = new MachineInstance(buildingId, machineTypeId, position, size, inputSlotSize);
                store.AddMachine(machine);
            });

            this.Mutate<InventoryStore>(store => store.UpdateGold(-cost));

            var inv = this.Query<IInventoryQueries>();
            this.Publish(new GoldChangedEvent(inv.Gold));

            RebuildConveyorLinks();
            this.Publish(new BuildingPlacedEvent(position, machineTypeId, buildingId));
            GF.Log($"Building {machineTypeId} placed at {position} for {cost} gold.");
        }

        public void PlaceConveyor(Vector2Int position, Direction direction)
        {
            var grid = this.Query<IGridQueries>();
            var inventory = this.Query<IInventoryQueries>();

            int cost = MachineConfigLoader.GetCost((int)MachineType.Conveyor);

            if (!grid.CanPlace(position, Vector2Int.one))
            {
                GF.LogError($"Cannot place conveyor at {position}: tile not free.");
                return;
            }

            if (inventory.Gold < cost)
            {
                GF.LogError("Cannot place conveyor: not enough gold.");
                return;
            }

            int buildingId = 0;

            this.Mutate<GridStore>(store =>
            {
                buildingId = store.AllocateBuildingId();
                var conveyor = new ConveyorSegment(buildingId, position, direction, SimConstants.ConveyorCapacity);
                store.AddConveyor(conveyor);
            });

            this.Mutate<InventoryStore>(store => store.UpdateGold(-cost));

            var inv2 = this.Query<IInventoryQueries>();
            this.Publish(new GoldChangedEvent(inv2.Gold));

            RebuildConveyorLinks();
            this.Publish(new BuildingPlacedEvent(position, (int)MachineType.Conveyor, buildingId));
        }

        public void RemoveBuilding(Vector2Int position)
        {
            var grid = this.Query<IGridQueries>();
            int buildingId = grid.GetBuilding(position.x, position.y);

            if (buildingId == 0)
            {
                GF.LogError("Cannot remove building: nothing at this position.");
                return;
            }

            int refund = 0;
            int machineTypeId = 0;

            var machine = grid.GetMachine(buildingId);
            if (machine != null)
            {
                machineTypeId = machine.MachineTypeId;
                int cost = MachineConfigLoader.GetCost(machineTypeId);
                float ratio = MachineConfigLoader.GetRefundRatio(machineTypeId);
                refund = Mathf.RoundToInt(cost * ratio);
                this.Mutate<GridStore>(store => store.RemoveMachine(buildingId));
            }
            else
            {
                int convCost = MachineConfigLoader.GetCost((int)MachineType.Conveyor);
                refund = Mathf.RoundToInt(convCost * MachineConfigLoader.GetRefundRatio((int)MachineType.Conveyor));
                this.Mutate<GridStore>(store => store.RemoveConveyor(buildingId));
            }

            this.Mutate<InventoryStore>(store => store.UpdateGold(refund));

            var inv3 = this.Query<IInventoryQueries>();
            this.Publish(new GoldChangedEvent(inv3.Gold));

            RebuildConveyorLinks();
            this.Publish(new BuildingRemovedEvent(position, buildingId));
            GF.Log($"Building removed at {position}. Refunded {refund} gold.");
        }

        private void RebuildConveyorLinks()
        {
            var grid = this.Query<IGridQueries>();

            // Reset all conveyor links
            for (int i = 0; i < grid.AllConveyors.Count; i++)
            {
                var conv = grid.AllConveyors[i];
                conv.NextSegmentId = -1;
                conv.PrevSegmentId = -1;
            }

            // Rebuild links
            for (int i = 0; i < grid.AllConveyors.Count; i++)
            {
                var conv = grid.AllConveyors[i];
                var nextPos = conv.Position + conv.Direction.ToVector2Int();

                if (!grid.IsInBounds(nextPos.x, nextPos.y)) continue;

                // Check if next position has a conveyor
                var nextConv = grid.GetConveyorAt(nextPos.x, nextPos.y);
                if (nextConv != null)
                {
                    conv.NextSegmentId = nextConv.Id;
                    nextConv.PrevSegmentId = conv.Id;
                    continue;
                }

                // Check if next position has a machine (including port)
                var nextMachine = grid.GetMachineAt(nextPos.x, nextPos.y);
                if (nextMachine != null)
                {
                    conv.NextSegmentId = nextMachine.Id;
                }
            }
        }
    }
}
