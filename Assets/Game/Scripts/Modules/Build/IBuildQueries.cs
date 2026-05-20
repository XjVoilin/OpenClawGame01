using System.Collections.Generic;
using JulyArch;

namespace CozyYard
{
    public interface IBuildQueries : IStoreQueries
    {
        IReadOnlyList<BuildingInstance> Buildings { get; }
        BuildingInstance GetBuildingAt(int x, int y);
        bool HasBuilding(int buildingId);
        int CountBuildings(int buildingId);
    }
}
