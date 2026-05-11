using System.Collections.Generic;
using JulyArch;
using JulyCore;

namespace IsleWorks.Tech
{
    public class TechData
    {
        public int CurrentEra;
        public HashSet<int> UnlockedMachineTypes;
        public HashSet<int> UnlockedRecipes;
    }

    /// <summary>
    /// 科技存储，管理当前时代进度和已解锁内容。
    /// </summary>
    public class TechStore : StoreBase<TechData>, ITechQueries
    {
        public int CurrentEra => Data.CurrentEra;
        public IReadOnlyCollection<int> UnlockedMachineTypes => Data.UnlockedMachineTypes;
        public IReadOnlyCollection<int> UnlockedRecipes => Data.UnlockedRecipes;

        protected override TechData LoadData()
        {
            return new TechData
            {
                CurrentEra = 0,
                UnlockedMachineTypes = new HashSet<int> { 1, 2, 3, 4 },
                UnlockedRecipes = new HashSet<int> { 1, 2, 3 }
            };
        }

        public bool IsMachineUnlocked(int machineTypeId)
        {
            return Data.UnlockedMachineTypes.Contains(machineTypeId);
        }

        public bool IsRecipeUnlocked(int recipeId)
        {
            return Data.UnlockedRecipes.Contains(recipeId);
        }

        public void AdvanceEra()
        {
            Data.CurrentEra++;
            GF.Log($"Era advanced to {Data.CurrentEra}");
        }

        public void UnlockMachine(int machineTypeId)
        {
            Data.UnlockedMachineTypes.Add(machineTypeId);
        }

        public void UnlockRecipe(int recipeId)
        {
            Data.UnlockedRecipes.Add(recipeId);
        }
    }
}
