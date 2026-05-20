using System.Collections.Generic;

namespace CozyYard
{
    public class BuildStore : SavableStoreBase<BuildData>
    {
        protected override string SaveKey => SaveKeys.BuildData;

        public IReadOnlyList<BuildingInstance> Buildings => Data.Buildings;

        public BuildingInstance GetBuildingAt(int x, int y)
        {
            for (int i = 0; i < Data.Buildings.Count; i++)
            {
                var b = Data.Buildings[i];
                if (x >= b.GridX && x < b.GridX + b.SizeX &&
                    y >= b.GridY && y < b.GridY + b.SizeY)
                    return b;
            }
            return null;
        }

        public bool HasBuilding(int buildingId)
        {
            for (int i = 0; i < Data.Buildings.Count; i++)
            {
                if (Data.Buildings[i].BuildingId == buildingId) return true;
            }
            return false;
        }

        public int CountBuildings(int buildingId)
        {
            int count = 0;
            for (int i = 0; i < Data.Buildings.Count; i++)
            {
                if (Data.Buildings[i].BuildingId == buildingId) count++;
            }
            return count;
        }

        public int AddBuilding(int buildingId, int x, int y, int sizeX, int sizeY)
        {
            int uid = Data.NextUniqueId++;
            Data.Buildings.Add(new BuildingInstance
            {
                UniqueId = uid,
                BuildingId = buildingId,
                GridX = x,
                GridY = y,
                SizeX = sizeX,
                SizeY = sizeY
            });
            MarkDirty();
            return uid;
        }

        public BuildingInstance RemoveBuilding(int uniqueId)
        {
            for (int i = 0; i < Data.Buildings.Count; i++)
            {
                if (Data.Buildings[i].UniqueId == uniqueId)
                {
                    var b = Data.Buildings[i];
                    Data.Buildings.RemoveAt(i);
                    MarkDirty();
                    return b;
                }
            }
            return null;
        }
    }
}
