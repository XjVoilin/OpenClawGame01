using System;
using System.Collections.Generic;
using JulyCore.Data.Save;

namespace CozyYard
{
    [Serializable]
    public class AnimalInstance
    {
        public int AnimalId;
        public int DaysSinceLastFed;
        public int DaysSinceLastProduce;
        public bool FedToday;
    }

    [Serializable]
    public class AnimalData : ISaveData
    {
        public List<AnimalInstance> Animals = new();

        public SaveImportance Importance => SaveImportance.Normal;
    }
}
