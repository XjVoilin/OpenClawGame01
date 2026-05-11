using System.Collections.Generic;
using IsleWorks.Production;

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
        public TileType[] Tiles;
        public int[] BuildingIds;
        public ResourceType[] ResourceNodes;
        public List<MachineInstance> Machines;
        public List<ConveyorSegment> Conveyors;
        public int NextBuildingId;

        public GridData()
        {
            Machines = new List<MachineInstance>();
            Conveyors = new List<ConveyorSegment>();
            NextBuildingId = 1;
        }

        public GridData(int width, int height)
        {
            Width = width;
            Height = height;
            Tiles = new TileType[width * height];
            BuildingIds = new int[width * height];
            ResourceNodes = new ResourceType[width * height];
            Machines = new List<MachineInstance>();
            Conveyors = new List<ConveyorSegment>();
            NextBuildingId = 1;
        }

        public TileType GetTileType(int x, int y) => Tiles[x + y * Width];

        public void SetTileType(int x, int y, TileType type) => Tiles[x + y * Width] = type;

        public int GetBuildingId(int x, int y) => BuildingIds[x + y * Width];

        public void SetBuildingId(int x, int y, int buildingId) => BuildingIds[x + y * Width] = buildingId;

        public ResourceType GetResourceNode(int x, int y) => ResourceNodes[x + y * Width];

        public void SetResourceNode(int x, int y, ResourceType type) => ResourceNodes[x + y * Width] = type;

        public int AllocateBuildingId() => NextBuildingId++;

        public void AddMachine(MachineInstance machine)
        {
            Machines.Add(machine);
            for (int x = 0; x < machine.Size.x; x++)
            {
                for (int y = 0; y < machine.Size.y; y++)
                {
                    SetBuildingId(machine.Position.x + x, machine.Position.y + y, machine.Id);
                }
            }
        }

        public void RemoveMachine(int id)
        {
            for (int i = Machines.Count - 1; i >= 0; i--)
            {
                if (Machines[i].Id == id)
                {
                    var machine = Machines[i];
                    for (int x = 0; x < machine.Size.x; x++)
                    {
                        for (int y = 0; y < machine.Size.y; y++)
                        {
                            SetBuildingId(machine.Position.x + x, machine.Position.y + y, 0);
                        }
                    }
                    Machines.RemoveAt(i);
                    return;
                }
            }
        }

        public MachineInstance GetMachineById(int id)
        {
            for (int i = 0; i < Machines.Count; i++)
            {
                if (Machines[i].Id == id) return Machines[i];
            }
            return null;
        }

        public MachineInstance GetMachineAt(int x, int y)
        {
            int buildingId = GetBuildingId(x, y);
            return buildingId > 0 ? GetMachineById(buildingId) : null;
        }

        public void AddConveyor(ConveyorSegment conveyor)
        {
            Conveyors.Add(conveyor);
            SetBuildingId(conveyor.Position.x, conveyor.Position.y, conveyor.Id);
        }

        public void RemoveConveyor(int id)
        {
            for (int i = Conveyors.Count - 1; i >= 0; i--)
            {
                if (Conveyors[i].Id == id)
                {
                    var conv = Conveyors[i];
                    SetBuildingId(conv.Position.x, conv.Position.y, 0);
                    Conveyors.RemoveAt(i);
                    return;
                }
            }
        }

        public ConveyorSegment GetConveyorById(int id)
        {
            for (int i = 0; i < Conveyors.Count; i++)
            {
                if (Conveyors[i].Id == id) return Conveyors[i];
            }
            return null;
        }

        public ConveyorSegment GetConveyorAt(int x, int y)
        {
            int buildingId = GetBuildingId(x, y);
            return buildingId > 0 ? GetConveyorById(buildingId) : null;
        }

        public bool IsInBounds(int x, int y) => x >= 0 && x < Width && y >= 0 && y < Height;
    }
}
