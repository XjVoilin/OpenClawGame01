using System;
using System.Collections.Generic;
using JulyCore.Data.Save;

namespace CozyYard
{
    public enum CropGrowthStage
    {
        Seed,
        Sprout,
        Growing,
        Mature,
        Withered
    }

    [Serializable]
    public class CropInstance
    {
        public int CropId;
        public int GridX;
        public int GridY;
        public CropGrowthStage Stage = CropGrowthStage.Seed;
        public int GrowthProgress;
        public int DaysSinceMature;
        public bool WateredToday;
    }

    [Serializable]
    public class FarmData : ISaveData
    {
        public List<CropInstance> Crops = new();

        public SaveImportance Importance => SaveImportance.Normal;
    }
}
