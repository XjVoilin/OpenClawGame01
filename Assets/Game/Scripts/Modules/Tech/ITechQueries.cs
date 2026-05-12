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

        /// <summary>
        /// 返回当前玩家可建造的机器 ID 列表（已解锁且 cost > 0），按 ID 排序。
        /// </summary>
        IReadOnlyList<int> GetBuildableMachineIds();
    }
}
