using System.Collections.Generic;
using IsleWorks.Production;
using JulyArch;
using JulyCore;
using UnityEngine;

namespace IsleWorks.Grid
{
    /// <summary>
    /// 网格存储 —— 持有网格相关数据，提供操作接口。
    /// </summary>
    public class GridStore : StoreBase<GridData>, IGridQueries
    {
        public int Width => Data.Width;
        public int Height => Data.Height;
        public IReadOnlyList<MachineInstance> AllMachines => Data.Machines;
        public IReadOnlyList<ConveyorSegment> AllConveyors => Data.Conveyors;

        protected override GridData LoadData()
        {
            return new GridData(8, 8);
        }

        protected override void OnReady()
        {
            SetDefaultLayout();
            SetResourceNodes();
            CreatePortMachine();
            GF.Log("GridStore initialized: 8x8 with default layout");
        }

        private void SetDefaultLayout()
        {
            for (int x = 0; x < Data.Width; x++)
            {
                for (int y = 0; y < Data.Height; y++)
                {
                    Data.SetTileType(x, y, TileType.Normal);
                }
            }
            // Port at right edge center
            Data.SetTileType(Data.Width - 1, Data.Height / 2, TileType.Port);
        }

        private void CreatePortMachine()
        {
            var portPos = new Vector2Int(Data.Width - 1, Data.Height / 2);
            int portId = Data.AllocateBuildingId();
            var portMachine = new MachineInstance(portId, (int)MachineType.Port, portPos, Vector2Int.one, 0);
            Data.Machines.Add(portMachine);
            Data.SetBuildingId(portPos.x, portPos.y, portId);
        }

        private void SetResourceNodes()
        {
            Data.SetResourceNode(2, 2, ResourceType.Ore);
            Data.SetResourceNode(5, 3, ResourceType.Ore);
            Data.SetResourceNode(3, 5, ResourceType.Ore);
        }

        public TileType GetTile(int x, int y) => Data.GetTileType(x, y);

        public int GetBuilding(int x, int y) => Data.GetBuildingId(x, y);

        public ResourceType GetResourceNode(int x, int y) => Data.GetResourceNode(x, y);

        public bool IsInBounds(int x, int y) => Data.IsInBounds(x, y);

        public bool CanPlace(Vector2Int pos, Vector2Int size)
        {
            for (int x = 0; x < size.x; x++)
            {
                for (int y = 0; y < size.y; y++)
                {
                    int tx = pos.x + x;
                    int ty = pos.y + y;
                    if (!Data.IsInBounds(tx, ty)) return false;
                    if (Data.GetTileType(tx, ty) != TileType.Normal) return false;
                    if (Data.GetBuildingId(tx, ty) != 0) return false;
                }
            }
            return true;
        }

        public MachineInstance GetMachine(int id) => Data.GetMachineById(id);

        public MachineInstance GetMachineAt(int x, int y) => Data.GetMachineAt(x, y);

        public ConveyorSegment GetConveyor(int id) => Data.GetConveyorById(id);

        public ConveyorSegment GetConveyorAt(int x, int y) => Data.GetConveyorAt(x, y);

        // Mutation methods (called via IMutationContext.GetStore<GridStore>())
        public void AddMachine(MachineInstance machine) => Data.AddMachine(machine);

        public void RemoveMachine(int id) => Data.RemoveMachine(id);

        public void AddConveyor(ConveyorSegment conveyor) => Data.AddConveyor(conveyor);

        public void RemoveConveyor(int id) => Data.RemoveConveyor(id);

        public void UpdateTileType(int x, int y, TileType tileType) => Data.SetTileType(x, y, tileType);

        public void PlaceBuilding(int x, int y, int buildingId) => Data.SetBuildingId(x, y, buildingId);

        public int AllocateBuildingId() => Data.AllocateBuildingId();
    }
}
