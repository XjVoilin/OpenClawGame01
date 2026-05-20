using cfg;
using JulyArch;
using JulyCore;

namespace CozyYard
{
    public struct RecipeUnlockedEvent
    {
        public int RecipeId;
    }

    public struct CraftStartedEvent
    {
        public int RecipeId;
    }

    public struct CraftCompletedEvent
    {
        public int RecipeId;
        public int OutputItemId;
    }

    public struct ExperimentFailedEvent { }

    /// <summary>制作系统：管理配方解锁、材料消耗制作、问妈妈获取配方、自由实验（失败产出黑暗料理）。</summary>
    public class CraftSystem : GameSystemBase
    {
        private CraftStore _store;
        private InventorySystem _inventorySystem;
        private BuildSystem _buildSystem;
        private TimeSystem _timeSystem;

        protected override void OnInitialize()
        {
            _store = GetStore<CraftStore>();
            _inventorySystem = GetSystem<InventorySystem>();
            _buildSystem = GetSystem<BuildSystem>();
            _timeSystem = GetSystem<TimeSystem>();

            this.Subscribe<DayChangedEvent>(OnDayChanged);
        }

        protected override void OnStart()
        {
            if (!_store.IsRecipeUnlocked(1))
                UnlockStarterRecipes();
        }

        public bool CanCraft(int recipeId)
        {
            if (!_store.IsRecipeUnlocked(recipeId)) return false;

            var cfg = GetRecipe(recipeId);
            if (cfg == null) return false;

            if (!_buildSystem.HasBuilding(cfg.RequiredBuildingId)) return false;

            for (int i = 0; i < cfg.InputItemIds.Count; i++)
            {
                if (!_inventorySystem.HasItem(cfg.InputItemIds[i], cfg.InputQuantities[i])) return false;
            }

            return true;
        }

        public bool StartCraft(int recipeId)
        {
            if (!CanCraft(recipeId)) return false;

            var cfg = GetRecipe(recipeId);
            if (cfg == null) return false;

            if (!_inventorySystem.ConsumeItems(cfg.InputItemIds.ToArray(), cfg.InputQuantities.ToArray())) return false;

            _store.AddJob(new CraftingJob
            {
                RecipeId = recipeId,
                BuildingUniqueId = 0,
                RemainingMinutes = cfg.CraftMinutes
            });

            _timeSystem.ConsumeTime(cfg.CraftMinutes);
            CompleteCraft(recipeId);

            Publish(new CraftStartedEvent { RecipeId = recipeId });
            return true;
        }

        /// <summary>Ask mom for a recipe (once per day).</summary>
        public bool AskMom(int itemIdHint)
        {
            if (_store.MomAsksToday >= 1) return false;
            var tbRecipe = GF.Config.GetTable<TbRecipe>();
            if (tbRecipe == null) return false;

            _store.IncrementMomAsks();

            foreach (var recipe in tbRecipe.DataList)
            {
                if (_store.IsRecipeUnlocked(recipe.Id)) continue;
                for (int j = 0; j < recipe.InputItemIds.Count; j++)
                {
                    if (recipe.InputItemIds[j] == itemIdHint)
                    {
                        UnlockRecipe(recipe.Id);
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>Experiment: try combining items freely.</summary>
        public bool Experiment(int[] itemIds, int[] quantities)
        {
            var tbRecipe = GF.Config.GetTable<TbRecipe>();
            if (tbRecipe == null) return false;

            foreach (var recipe in tbRecipe.DataList)
            {
                if (MatchesRecipe(recipe, itemIds, quantities))
                {
                    if (!_inventorySystem.ConsumeItems(itemIds, quantities)) return false;

                    UnlockRecipe(recipe.Id);
                    _inventorySystem.AddItem(recipe.OutputItemId, recipe.OutputQuantity);
                    _timeSystem.ConsumeTime(recipe.CraftMinutes);

                    Publish(new CraftCompletedEvent { RecipeId = recipe.Id, OutputItemId = recipe.OutputItemId });
                    return true;
                }
            }

            _inventorySystem.ConsumeItems(itemIds, quantities);
            _inventorySystem.AddItem(9001, 1);
            _timeSystem.ConsumeTime(30);
            Publish(new ExperimentFailedEvent());
            return false;
        }

        public void UnlockRecipe(int recipeId)
        {
            _store.UnlockRecipe(recipeId);
            Publish(new RecipeUnlockedEvent { RecipeId = recipeId });
        }

        /// <summary>Grant starter recipes on first play.</summary>
        public void UnlockStarterRecipes()
        {
            UnlockRecipe(5);
            UnlockRecipe(1);
            UnlockRecipe(6);
        }

        private void CompleteCraft(int recipeId)
        {
            var cfg = GetRecipe(recipeId);
            if (cfg == null) return;

            _inventorySystem.AddItem(cfg.OutputItemId, cfg.OutputQuantity);

            for (int i = _store.ActiveJobs.Count - 1; i >= 0; i--)
            {
                if (((CraftingJob)_store.ActiveJobs[i]).RecipeId == recipeId)
                {
                    _store.RemoveJob((CraftingJob)_store.ActiveJobs[i]);
                    break;
                }
            }

            Publish(new CraftCompletedEvent { RecipeId = recipeId, OutputItemId = cfg.OutputItemId });
        }

        private void OnDayChanged(DayChangedEvent e)
        {
            _store.ResetMomAsks();
        }

        private static bool MatchesRecipe(Recipe recipe, int[] itemIds, int[] quantities)
        {
            if (itemIds.Length != recipe.InputItemIds.Count) return false;
            for (int i = 0; i < recipe.InputItemIds.Count; i++)
            {
                bool found = false;
                for (int j = 0; j < itemIds.Length; j++)
                {
                    if (itemIds[j] == recipe.InputItemIds[i] && quantities[j] >= recipe.InputQuantities[i])
                    {
                        found = true;
                        break;
                    }
                }
                if (!found) return false;
            }
            return true;
        }

        private Recipe GetRecipe(int recipeId)
        {
            return GF.Config.GetTable<TbRecipe>()?.GetOrDefault(recipeId);
        }
    }
}
