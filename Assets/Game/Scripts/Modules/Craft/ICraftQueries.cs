using System.Collections.Generic;
using JulyArch;

namespace CozyYard
{
    public interface ICraftQueries : IStoreQueries
    {
        IReadOnlyList<int> UnlockedRecipeIds { get; }
        IReadOnlyList<CraftingJob> ActiveJobs { get; }
        bool IsRecipeUnlocked(int recipeId);
        int MomAsksToday { get; }
    }
}
