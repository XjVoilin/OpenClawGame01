using System;
using System.Collections.Generic;
using JulyCore.Data.Save;

namespace CozyYard
{
    [Serializable]
    public class CraftingJob
    {
        public int RecipeId;
        public int BuildingUniqueId;
        public int RemainingMinutes;
    }

    [Serializable]
    public class CraftData : ISaveData
    {
        public List<int> UnlockedRecipeIds = new();
        public List<CraftingJob> ActiveJobs = new();
        public int MomAsksToday;

        public SaveImportance Importance => SaveImportance.Normal;
    }
}
