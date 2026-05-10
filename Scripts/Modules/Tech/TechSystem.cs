using UnityEngine;
using IsleWorks.Data;
using JulyArch;

namespace IsleWorks.Systems
{
    /// <summary>
    /// 科技系统，负责时代推进与科技解锁逻辑。
    /// </summary>
    public class TechSystem : GameSystemBase
    {
        [Inject] private InventoryStore _inventoryStore;
        [Inject] private TechStore _techStore;

        /// <summary>
        /// 检查里程碑，条件满足时推进时代。
        /// </summary>
        public void CheckMilestone()
        {
            int currentEra = _techStore.CurrentEra;
            int productionValue = _inventoryStore.TotalProductionValue;

            // 获取当前时代的里程碑目标
            int requiredValue = GetEraMilestoneRequirement(currentEra);

            if (productionValue >= requiredValue)
            {
                AdvanceEra();
            }
        }

        /// <summary>
        /// 推进时代并解锁科技。
        /// </summary>
        private void AdvanceEra()
        {
            _techStore.AdvanceEra();

            // 解锁新机器与配方
            int newEra = _techStore.CurrentEra;
            UnlockEraFeatures(newEra);

            Debug.Log($"Era advanced! Current era: {newEra}");
        }

        /// <summary>
        /// 解锁当前时代的科技。
        /// </summary>
        private void UnlockEraFeatures(int era)
        {
            // TODO: 查询时代解锁表以解锁机器和配方
            Debug.Log($"Features unlocked for era {era}");
        }

        /// <summary>
        /// 获取里程碑目标。
        /// </summary>
        private int GetEraMilestoneRequirement(int era)
        {
            // TODO: 查询里程碑表获取目标值
            return 1000 * (era + 1); // 示例逻辑
        }
    }
}