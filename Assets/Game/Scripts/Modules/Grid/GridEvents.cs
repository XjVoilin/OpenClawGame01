using UnityEngine;

namespace IsleWorks.Grid
{
    public readonly struct BuildingPlacedEvent
    {
        public readonly Vector2Int Position;
        public readonly int MachineTypeId;
        public readonly int BuildingId;

        public BuildingPlacedEvent(Vector2Int position, int machineTypeId, int buildingId)
        {
            Position = position;
            MachineTypeId = machineTypeId;
            BuildingId = buildingId;
        }
    }

    public readonly struct BuildingRemovedEvent
    {
        public readonly Vector2Int Position;
        public readonly int BuildingId;

        public BuildingRemovedEvent(Vector2Int position, int buildingId)
        {
            Position = position;
            BuildingId = buildingId;
        }
    }

    public readonly struct TileChangedEvent
    {
        public readonly Vector2Int Position;
        public readonly TileType NewType;

        public TileChangedEvent(Vector2Int position, TileType newType)
        {
            Position = position;
            NewType = newType;
        }
    }

    public readonly struct MachineSelectedEvent
    {
        public readonly int MachineTypeId;

        public MachineSelectedEvent(int machineTypeId)
        {
            MachineTypeId = machineTypeId;
        }
    }
}
