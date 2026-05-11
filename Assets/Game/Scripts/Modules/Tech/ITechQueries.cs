using System.Collections.Generic;
using JulyArch;

namespace IsleWorks.Tech
{
    public interface ITechQueries : IStoreQueries
    {
        int CurrentEra { get; }
        IReadOnlyCollection<int> UnlockedMachineTypes { get; }
        IReadOnlyCollection<int> UnlockedRecipes { get; }
        bool IsMachineUnlocked(int machineTypeId);
        bool IsRecipeUnlocked(int recipeId);
    }
}
