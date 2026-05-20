using JulyArch;

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

    public class CraftSystem : GameSystemBase
    {
        private CraftStore _store;
        private InventorySystem _inventorySystem;
        private BuildSystem _buildSystem;
        private TimeSystem _timeSystem;

        private struct RecipeConfig
        {
            public int Id;
            public int RequiredBuildingId;
            public int[] InputItemIds;
            public int[] InputQuantities;
            public int OutputItemId;
            public int OutputQuantity;
            public int CraftMinutes;
        }

        private static readonly RecipeConfig[] Recipes = {
            new() { Id=1, RequiredBuildingId=20, InputItemIds=new[]{3006}, InputQuantities=new[]{3}, OutputItemId=4001, OutputQuantity=2, CraftMinutes=120 },
            new() { Id=2, RequiredBuildingId=30, InputItemIds=new[]{3003}, InputQuantities=new[]{2}, OutputItemId=4002, OutputQuantity=2, CraftMinutes=60 },
            new() { Id=3, RequiredBuildingId=10, InputItemIds=new[]{4001,4002}, InputQuantities=new[]{2,2}, OutputItemId=5001, OutputQuantity=1, CraftMinutes=90 },
            new() { Id=4, RequiredBuildingId=10, InputItemIds=new[]{3101,3005}, InputQuantities=new[]{1,1}, OutputItemId=5002, OutputQuantity=1, CraftMinutes=30 },
            new() { Id=5, RequiredBuildingId=10, InputItemIds=new[]{3001}, InputQuantities=new[]{2}, OutputItemId=5003, OutputQuantity=1, CraftMinutes=20 },
            new() { Id=6, RequiredBuildingId=20, InputItemIds=new[]{3002}, InputQuantities=new[]{2}, OutputItemId=4003, OutputQuantity=2, CraftMinutes=120 },
            new() { Id=7, RequiredBuildingId=20, InputItemIds=new[]{3004}, InputQuantities=new[]{3}, OutputItemId=4004, OutputQuantity=2, CraftMinutes=120 },
            new() { Id=8, RequiredBuildingId=10, InputItemIds=new[]{4004}, InputQuantities=new[]{2}, OutputItemId=5004, OutputQuantity=1, CraftMinutes=30 },
            new() { Id=9, RequiredBuildingId=20, InputItemIds=new[]{3007}, InputQuantities=new[]{3}, OutputItemId=5005, OutputQuantity=2, CraftMinutes=180 },
        };

        protected override void OnInitialize()
        {
            _store = GetStore<CraftStore>();
            _inventorySystem = GetSystem<InventorySystem>();
            _buildSystem = GetSystem<BuildSystem>();
            _timeSystem = GetSystem<TimeSystem>();

            this.Subscribe<DayChangedEvent>(OnDayChanged);
        }

        public bool CanCraft(int recipeId)
        {
            if (!_store.IsRecipeUnlocked(recipeId)) return false;

            var cfg = GetRecipe(recipeId);
            if (cfg == null) return false;

            if (!_buildSystem.HasBuilding(cfg.Value.RequiredBuildingId)) return false;

            for (int i = 0; i < cfg.Value.InputItemIds.Length; i++)
            {
                if (!_inventorySystem.HasItem(cfg.Value.InputItemIds[i], cfg.Value.InputQuantities[i])) return false;
            }

            return true;
        }

        public bool StartCraft(int recipeId)
        {
            if (!CanCraft(recipeId)) return false;

            var cfg = GetRecipe(recipeId);
            if (cfg == null) return false;

            if (!_inventorySystem.ConsumeItems(cfg.Value.InputItemIds, cfg.Value.InputQuantities)) return false;

            _store.AddJob(new CraftingJob
            {
                RecipeId = recipeId,
                BuildingUniqueId = 0,
                RemainingMinutes = cfg.Value.CraftMinutes
            });

            _timeSystem.ConsumeTime(cfg.Value.CraftMinutes);

            // For simplicity, complete immediately (time consumed represents game time passing)
            CompleteCraft(recipeId);

            Publish(new CraftStartedEvent { RecipeId = recipeId });
            return true;
        }

        /// <summary>Ask mom for a recipe (once per day).</summary>
        public bool AskMom(int itemIdHint)
        {
            if (_store.MomAsksToday >= 1) return false;

            _store.IncrementMomAsks();

            // Find a recipe that uses this item as input and hasn't been unlocked
            for (int i = 0; i < Recipes.Length; i++)
            {
                if (_store.IsRecipeUnlocked(Recipes[i].Id)) continue;
                for (int j = 0; j < Recipes[i].InputItemIds.Length; j++)
                {
                    if (Recipes[i].InputItemIds[j] == itemIdHint)
                    {
                        UnlockRecipe(Recipes[i].Id);
                        return true;
                    }
                }
            }

            // Mom doesn't know a recipe for this
            return false;
        }

        /// <summary>Experiment: try combining items freely.</summary>
        public bool Experiment(int[] itemIds, int[] quantities)
        {
            // Check if combination matches any recipe
            for (int i = 0; i < Recipes.Length; i++)
            {
                if (MatchesRecipe(Recipes[i], itemIds, quantities))
                {
                    // Consume materials
                    if (!_inventorySystem.ConsumeItems(itemIds, quantities)) return false;

                    // Unlock and produce
                    UnlockRecipe(Recipes[i].Id);
                    _inventorySystem.AddItem(Recipes[i].OutputItemId, Recipes[i].OutputQuantity);
                    _timeSystem.ConsumeTime(Recipes[i].CraftMinutes);

                    Publish(new CraftCompletedEvent { RecipeId = Recipes[i].Id, OutputItemId = Recipes[i].OutputItemId });
                    return true;
                }
            }

            // Failed experiment - consume materials, give junk
            _inventorySystem.ConsumeItems(itemIds, quantities);
            _inventorySystem.AddItem(9001, 1); // 黑暗料理
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
            UnlockRecipe(5); // 清炒白菜
            UnlockRecipe(1); // 桂花干
            UnlockRecipe(6); // 萝卜干
        }

        private void CompleteCraft(int recipeId)
        {
            var cfg = GetRecipe(recipeId);
            if (cfg == null) return;

            _inventorySystem.AddItem(cfg.Value.OutputItemId, cfg.Value.OutputQuantity);

            // Remove job
            for (int i = _store.ActiveJobs.Count - 1; i >= 0; i--)
            {
                if (((CraftingJob)_store.ActiveJobs[i]).RecipeId == recipeId)
                {
                    _store.RemoveJob((CraftingJob)_store.ActiveJobs[i]);
                    break;
                }
            }

            Publish(new CraftCompletedEvent { RecipeId = recipeId, OutputItemId = cfg.Value.OutputItemId });
        }

        private void OnDayChanged(DayChangedEvent e)
        {
            _store.ResetMomAsks();
        }

        private bool MatchesRecipe(RecipeConfig recipe, int[] itemIds, int[] quantities)
        {
            if (itemIds.Length != recipe.InputItemIds.Length) return false;
            for (int i = 0; i < recipe.InputItemIds.Length; i++)
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

        private RecipeConfig? GetRecipe(int recipeId)
        {
            for (int i = 0; i < Recipes.Length; i++)
            {
                if (Recipes[i].Id == recipeId) return Recipes[i];
            }
            return null;
        }
    }
}
