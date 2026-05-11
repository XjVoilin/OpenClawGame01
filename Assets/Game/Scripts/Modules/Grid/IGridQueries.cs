using System.Collections.Generic;
using IsleWorks.Production;
using JulyArch;
using UnityEngine;

namespace IsleWorks.Grid
{
    public interface IGridQueries : IStoreQueries
    {
        int Width { get; }
        int Height { get; }
        TileType GetTile(int x, int y);
        int GetBuilding(int x, int y);
        ResourceType GetResourceNode(int x, int y);
        bool IsInBounds(int x, int y);
        bool CanPlace(Vector2Int pos, Vector2Int size);
        MachineInstance GetMachine(int id);
        MachineInstance GetMachineAt(int x, int y);
        ConveyorSegment GetConveyor(int id);
        ConveyorSegment GetConveyorAt(int x, int y);
        IReadOnlyList<MachineInstance> AllMachines { get; }
        IReadOnlyList<ConveyorSegment> AllConveyors { get; }
    }
}
