using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using JulyArch;
using IsleWorks.Economy;

namespace IsleWorks.Tech
{
    /// <summary>
    /// 科技系统，负责时代推进与科技解锁逻辑。
    /// </summary>
    public class TechSystem : GameSystemBase
    {
        public void CheckMilestone()
        {
            var inventory = this.Query<IInventoryQueries>();
            var tech = this.Query<ITechQueries>();

            int requiredValue = GetEraMilestoneRequirement(tech.CurrentEra);

            if (inventory.TotalProductionValue >= requiredValue)
            {
                RunEraTransitionProcedure();
            }
        }

        private void RunEraTransitionProcedure()
        {
            var tech = this.Query<ITechQueries>();
            var procedure = new EraTransitionProcedure(tech.CurrentEra + 1, GameObject.Find("UIRoot").transform);
            RunProcedureAsync(procedure).Forget();
        }

        private async UniTaskVoid RunProcedureAsync(EraTransitionProcedure procedure)
        {
            try
            {
                await procedure.ExecuteAsync(CancellationToken.None);
                AdvanceEra();
            }
            catch (System.Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        private void AdvanceEra()
        {
            this.Mutate<TechStore>(s => s.AdvanceEra());
            var tech = this.Query<ITechQueries>();
            UnlockEraFeatures(tech.CurrentEra);
            Debug.Log($"Era advanced! Current era: {tech.CurrentEra}");
        }

        private void UnlockEraFeatures(int era)
        {
            // TODO: 查询时代解锁表以解锁机器和配方
            Debug.Log($"Features unlocked for era {era}");
        }

        private int GetEraMilestoneRequirement(int era)
        {
            // TODO: 查询里程碑表获取目标值
            return 1000 * (era + 1);
        }
    }
}
