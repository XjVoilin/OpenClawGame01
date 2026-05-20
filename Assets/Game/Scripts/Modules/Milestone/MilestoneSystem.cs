using JulyArch;

namespace CozyYard
{
    public class MilestoneSystem : GameSystemBase
    {
        private MilestoneStore _store;
        private InventorySystem _inventorySystem;
        private CraftSystem _craftSystem;
        private GridSystem _gridSystem;

        private struct MilestoneConfig
        {
            public int Id;
            public string ConditionType;
            public int ConditionTarget;
            public int ConditionCount;
            public string RewardType;
            public int RewardId;
            public int RewardQty;
        }

        private static readonly MilestoneConfig[] Configs = {
            new() { Id=1,  ConditionType="PlantCrop",     ConditionTarget=0, ConditionCount=1,  RewardType="Coins",       RewardId=0,    RewardQty=50 },
            new() { Id=2,  ConditionType="HarvestCrop",   ConditionTarget=0, ConditionCount=1,  RewardType="Coins",       RewardId=0,    RewardQty=100 },
            new() { Id=3,  ConditionType="BuildBuilding", ConditionTarget=1, ConditionCount=1,  RewardType="Expansion",   RewardId=0,    RewardQty=1 },
            new() { Id=4,  ConditionType="AdoptAnimal",   ConditionTarget=0, ConditionCount=1,  RewardType="Item",        RewardId=1001, RewardQty=10 },
            new() { Id=5,  ConditionType="CraftItem",     ConditionTarget=0, ConditionCount=1,  RewardType="Coins",       RewardId=0,    RewardQty=80 },
            new() { Id=6,  ConditionType="FulfillOrder",  ConditionTarget=0, ConditionCount=1,  RewardType="Coins",       RewardId=0,    RewardQty=60 },
            new() { Id=7,  ConditionType="BuildBuilding", ConditionTarget=0, ConditionCount=3,  RewardType="Expansion",   RewardId=0,    RewardQty=1 },
            new() { Id=8,  ConditionType="HarvestCrop",   ConditionTarget=0, ConditionCount=10, RewardType="Item",        RewardId=3006, RewardQty=5 },
            new() { Id=9,  ConditionType="UnlockRecipe",  ConditionTarget=0, ConditionCount=5,  RewardType="Coins",       RewardId=0,    RewardQty=200 },
            new() { Id=10, ConditionType="FulfillOrder",  ConditionTarget=0, ConditionCount=5,  RewardType="RecipeUnlock", RewardId=3,   RewardQty=1 },
        };

        protected override void OnInitialize()
        {
            _store = GetStore<MilestoneStore>();
            _inventorySystem = GetSystem<InventorySystem>();
            _craftSystem = GetSystem<CraftSystem>();
            _gridSystem = GetSystem<GridSystem>();

            this.Subscribe<CropHarvestedEvent>(e => IncrementCondition("HarvestCrop", 0));
            this.Subscribe<BuildingPlacedEvent>(e => IncrementCondition("BuildBuilding", e.BuildingId));
            this.Subscribe<OrderCompletedEvent>(e => IncrementCondition("FulfillOrder", 0));
            this.Subscribe<CraftCompletedEvent>(e => IncrementCondition("CraftItem", 0));
            this.Subscribe<RecipeUnlockedEvent>(e => IncrementCondition("UnlockRecipe", 0));
        }

        public void NotifyPlantCrop()
        {
            IncrementCondition("PlantCrop", 0);
        }

        public void NotifyAdoptAnimal()
        {
            IncrementCondition("AdoptAnimal", 0);
        }

        private void IncrementCondition(string conditionType, int targetId)
        {
            for (int i = 0; i < Configs.Length; i++)
            {
                var cfg = Configs[i];
                if (cfg.ConditionType != conditionType) continue;
                if (cfg.ConditionTarget != 0 && cfg.ConditionTarget != targetId) continue;

                var progress = _store.GetOrCreateProgress(cfg.Id);
                if (progress.Completed) continue;

                progress.CurrentCount++;
                _store.MarkDirtyExplicit();

                if (progress.CurrentCount >= cfg.ConditionCount)
                {
                    CompleteMilestone(cfg, progress);
                }
            }
        }

        private void CompleteMilestone(MilestoneConfig cfg, MilestoneProgress progress)
        {
            progress.Completed = true;
            _store.MarkDirtyExplicit();

            GrantReward(cfg);

            Publish(new MilestoneAchievedEvent { MilestoneId = cfg.Id });
        }

        private void GrantReward(MilestoneConfig cfg)
        {
            switch (cfg.RewardType)
            {
                case "Coins":
                    _inventorySystem.AddCoins(cfg.RewardQty);
                    break;
                case "Item":
                    _inventorySystem.AddItem(cfg.RewardId, cfg.RewardQty);
                    break;
                case "RecipeUnlock":
                    _craftSystem.UnlockRecipe(cfg.RewardId);
                    break;
                case "Expansion":
                    ExpandYard();
                    break;
            }
        }

        private void ExpandYard()
        {
            _store.IncrementExpansion();
            int newWidth = _gridSystem.Width + 2;
            int newHeight = _gridSystem.Height + 2;
            _gridSystem.ExpandGrid(newWidth, newHeight);
        }
    }
}
