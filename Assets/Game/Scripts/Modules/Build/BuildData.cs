using System;
using System.Collections.Generic;
using JulyCore.Data.Save;

namespace CozyYard
{
    [Serializable]
    public class BuildingInstance
    {
        public int UniqueId;
        public int BuildingId;
        public int GridX;
        public int GridY;
        public int SizeX;
        public int SizeY;
    }

    [Serializable]
    public class BuildData : ISaveData
    {
        public List<BuildingInstance> Buildings = new();
        public int NextUniqueId = 1;

        public SaveImportance Importance => SaveImportance.Normal;
    }
}
