using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using JulyArch;
using JulyCore;
using IsleWorks.Economy;

namespace IsleWorks.Tech
{
    /// <summary>
    /// 科技系统，负责里程碑检测与时代推进。
    /// </summary>
    public class TechSystem : GameSystemBase
    {
        private bool _transitioning;

        public void CheckMilestone()
        {
            if (_transitioning) return;

            var inventory = this.Query<IInventoryQueries>();
            var tech = this.Query<ITechQueries>();

            int nextEra = tech.CurrentEra + 1;
            int requiredValue = MilestoneConfigLoader.GetRequiredValueForEra(nextEra);

            if (inventory.TotalProductionValue >= requiredValue)
            {
                RunEraTransitionProcedure(nextEra);
            }
        }

        private void RunEraTransitionProcedure(int newEra)
        {
            _transitioning = true;
            var uiRoot = GameObject.Find("UIRoot");
            var viewRoot = uiRoot != null ? uiRoot.transform : null;
            var procedure = new EraTransitionProcedure(newEra, viewRoot);
            RunProcedureAsync(procedure, newEra).Forget();
        }

        private async UniTaskVoid RunProcedureAsync(EraTransitionProcedure procedure, int newEra)
        {
            try
            {
                await procedure.ExecuteAsync(CancellationToken.None);
                AdvanceEra(newEra);
            }
            catch (System.Exception ex)
            {
                GF.LogException(ex);
            }
            finally
            {
                _transitioning = false;
            }
        }

        private void AdvanceEra(int newEra)
        {
            this.Mutate<TechStore>(s => s.AdvanceEra());
            UnlockEraFeatures(newEra);
            this.Publish(new EraChangedEvent(newEra));
            GF.Log($"Era advanced! Current era: {newEra}");
        }

        private void UnlockEraFeatures(int era)
        {
            var milestone = MilestoneConfigLoader.GetMilestoneForEra(era);
            if (milestone == null)
            {
                GF.LogError($"No milestone found for era {era}");
                return;
            }

            this.Mutate<TechStore>(store =>
            {
                for (int i = 0; i < milestone.UnlockMachines.Length; i++)
                {
                    store.UnlockMachine(milestone.UnlockMachines[i]);
                    GF.Log($"Unlocked machine: {milestone.UnlockMachines[i]}");
                }
                for (int i = 0; i < milestone.UnlockRecipes.Length; i++)
                {
                    store.UnlockRecipe(milestone.UnlockRecipes[i]);
                    GF.Log($"Unlocked recipe: {milestone.UnlockRecipes[i]}");
                }
            });
        }
    }
}
