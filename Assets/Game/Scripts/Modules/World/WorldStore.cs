using System.Collections.Generic;
using JulyArch;
using UnityEngine;

namespace OffTrail.World
{
    public sealed class WorldData
    {
        public readonly HashSet<int> DiscoveredRegions = new();
        public int CurrentRegionId = 1;
        public readonly Dictionary<int, bool> BuiltStations = new();
    }

    public interface IWorldQueries : IStoreQueries
    {
        int CurrentRegionId { get; }
        bool IsRegionDiscovered(int regionId);
        bool IsStationBuilt(int stationId);
    }

    public sealed class WorldStore : StoreBase<WorldData>, IWorldQueries
    {
        public int CurrentRegionId => Data.CurrentRegionId;

        protected override WorldData LoadData()
        {
            var d = new WorldData();
            d.DiscoveredRegions.Add(1);
            return d;
        }

        public bool IsRegionDiscovered(int regionId) => Data.DiscoveredRegions.Contains(regionId);

        public bool IsStationBuilt(int stationId) =>
            Data.BuiltStations.TryGetValue(stationId, out var built) && built;

        public void DiscoverRegion(int regionId)
        {
            if (!Data.DiscoveredRegions.Add(regionId))
                return;

            this.Publish(new RegionDiscovered { RegionId = regionId, RegionName = "" });
        }

        public void SetCurrentRegion(int regionId) =>
            Data.CurrentRegionId = Mathf.Max(1, regionId);

        public void BuildStation(int stationId)
        {
            if (IsStationBuilt(stationId))
                return;

            Data.BuiltStations[stationId] = true;
            this.Publish(new StationBuilt { StationId = stationId, StationName = "" });
        }
    }
}
