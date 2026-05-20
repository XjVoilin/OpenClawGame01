using cfg;
using JulyArch;
using JulyCore;

namespace CozyYard
{
    /// <summary>里程碑系统：监听各类游戏事件累计进度，达成条件时发放奖励并触发院子扩建。</summary>
    public class MilestoneSystem : GameSystemBase
    {
        private MilestoneStore _store;
        private InventorySystem _inventorySystem;
        private CraftSystem _craftSystem;
        private GridSystem _gridSystem;

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
            var tbMilestone = GF.Config.GetTable<TbMilestone>();
            if (tbMilestone == null) return;

            foreach (var cfg in tbMilestone.DataList)
            {
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

        private void CompleteMilestone(Milestone cfg, MilestoneProgress progress)
        {
            progress.Completed = true;
            _store.MarkDirtyExplicit();

            GrantReward(cfg);

            Publish(new MilestoneAchievedEvent { MilestoneId = cfg.Id });
        }

        private void GrantReward(Milestone cfg)
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
