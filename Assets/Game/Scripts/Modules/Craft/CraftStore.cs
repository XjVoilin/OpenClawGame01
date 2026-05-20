using System.Collections.Generic;

namespace CozyYard
{
    public class CraftStore : SavableStoreBase<CraftData>, ICraftQueries
    {
        protected override string SaveKey => SaveKeys.CraftData;

        public IReadOnlyList<int> UnlockedRecipeIds => Data.UnlockedRecipeIds;
        public IReadOnlyList<CraftingJob> ActiveJobs => Data.ActiveJobs;
        public int MomAsksToday => Data.MomAsksToday;

        public bool IsRecipeUnlocked(int recipeId)
        {
            return Data.UnlockedRecipeIds.Contains(recipeId);
        }

        public void UnlockRecipe(int recipeId)
        {
            if (!Data.UnlockedRecipeIds.Contains(recipeId))
            {
                Data.UnlockedRecipeIds.Add(recipeId);
                MarkDirty();
            }
        }

        public void AddJob(CraftingJob job)
        {
            Data.ActiveJobs.Add(job);
            MarkDirty();
        }

        public void RemoveJob(CraftingJob job)
        {
            Data.ActiveJobs.Remove(job);
            MarkDirty();
        }

        public void IncrementMomAsks()
        {
            Data.MomAsksToday++;
            MarkDirty();
        }

        public void ResetMomAsks()
        {
            Data.MomAsksToday = 0;
            MarkDirty();
        }

        public void MarkDirtyExplicit() => MarkDirty();
    }
}
