using cfg;
using JulyArch;

namespace CozyYard
{
    public class FarmSystem : GameSystemBase
    {
        private FarmStore _store;
        private GridSystem _gridSystem;
        private InventorySystem _inventorySystem;
        private TimeSystem _timeSystem;

        protected override void OnInitialize()
        {
            _store = GetStore<FarmStore>();
            _gridSystem = GetSystem<GridSystem>();
            _inventorySystem = GetSystem<InventorySystem>();
            _timeSystem = GetSystem<TimeSystem>();

            this.Subscribe<DayChangedEvent>(OnDayChanged);
        }

        public CropInstance GetCropAt(int x, int y) => _store.GetCropAt(x, y);

        public bool TillSoil(int x, int y)
        {
            bool success = _gridSystem.TillSoil(x, y);
            if (success)
            {
                _timeSystem.ConsumeTime(10);
            }
            return success;
        }

        public bool PlantCrop(int x, int y, int cropId, int seedItemId)
        {
            var cell = _gridSystem.GetCell(x, y);
            if (cell == null || cell.State != CellState.Soil) return false;
            if (_store.HasCropAt(x, y)) return false;
            if (!_inventorySystem.HasItem(seedItemId)) return false;

            _inventorySystem.RemoveItem(seedItemId, 1);

            var crop = new CropInstance
            {
                CropId = cropId,
                GridX = x,
                GridY = y,
                Stage = CropGrowthStage.Seed,
                GrowthProgress = 0,
                DaysSinceMature = 0,
                WateredToday = false
            };

            _store.AddCrop(crop);
            _timeSystem.ConsumeTime(15);

            Publish(new CropPlantedEvent { GridX = x, GridY = y, CropId = cropId });
            GetSystem<MilestoneSystem>().NotifyPlantCrop();
            return true;
        }

        public bool WaterCrop(int x, int y)
        {
            var crop = _store.GetCropAt(x, y);
            if (crop == null) return false;
            if (crop.WateredToday) return false;
            if (crop.Stage == CropGrowthStage.Mature || crop.Stage == CropGrowthStage.Withered) return false;

            crop.WateredToday = true;
            _store.MarkDirtyExplicit();
            _timeSystem.ConsumeTime(5);

            Publish(new CropWateredEvent { GridX = x, GridY = y });
            return true;
        }

        public bool HarvestCrop(int x, int y)
        {
            var crop = _store.GetCropAt(x, y);
            if (crop == null || crop.Stage != CropGrowthStage.Mature) return false;

            var cfg = GetCropConfig(crop.CropId);
            if (cfg == null) return false;

            if (!_inventorySystem.AddItem(cfg.ProduceItemId, cfg.ProduceQuantity)) return false;

            _store.RemoveCrop(crop);
            _timeSystem.ConsumeTime(10);

            Publish(new CropHarvestedEvent { CropId = crop.CropId, Quantity = cfg.ProduceQuantity });
            return true;
        }

        public void RemoveWithered(int x, int y)
        {
            var crop = _store.GetCropAt(x, y);
            if (crop == null || crop.Stage != CropGrowthStage.Withered) return;

            _store.RemoveCrop(crop);
            _timeSystem.ConsumeTime(5);
        }

        private void OnDayChanged(DayChangedEvent e)
        {
            ProcessDailyGrowth();
        }

        private void ProcessDailyGrowth()
        {
            for (int i = _store.Crops.Count - 1; i >= 0; i--)
            {
                var crop = (CropInstance)_store.Crops[i];

                if (crop.Stage == CropGrowthStage.Withered) continue;

                if (crop.Stage == CropGrowthStage.Mature)
                {
                    crop.DaysSinceMature++;
                    int harvestWindow = GetHarvestWindow(crop.CropId);
                    if (crop.DaysSinceMature > harvestWindow)
                    {
                        crop.Stage = CropGrowthStage.Withered;
                        Publish(new CropWitheredEvent { GridX = crop.GridX, GridY = crop.GridY });
                    }
                    continue;
                }

                int growthIncrement = crop.WateredToday ? 2 : 1;
                crop.GrowthProgress += growthIncrement;
                crop.WateredToday = false;

                int totalGrowthNeeded = GetGrowthDays(crop.CropId) * 2;
                float ratio = (float)crop.GrowthProgress / totalGrowthNeeded;

                CropGrowthStage newStage;
                if (ratio >= 1f)
                    newStage = CropGrowthStage.Mature;
                else if (ratio >= 0.6f)
                    newStage = CropGrowthStage.Growing;
                else if (ratio >= 0.2f)
                    newStage = CropGrowthStage.Sprout;
                else
                    newStage = CropGrowthStage.Seed;

                if (newStage != crop.Stage)
                {
                    crop.Stage = newStage;
                    Publish(new CropGrowthEvent { GridX = crop.GridX, GridY = crop.GridY, NewStage = newStage });

                    if (newStage == CropGrowthStage.Mature)
                    {
                        Publish(new CropReadyEvent { GridX = crop.GridX, GridY = crop.GridY, CropId = crop.CropId });
                    }
                }
            }

            _store.MarkDirtyExplicit();
        }

        private Crop GetCropConfig(int cropId)
        {
            return CfgTable.Tables?.TbCrop.GetOrDefault(cropId);
        }

        private int GetGrowthDays(int cropId)
        {
            return GetCropConfig(cropId)?.GrowthDays ?? 5;
        }

        private int GetHarvestWindow(int cropId)
        {
            return GetCropConfig(cropId)?.HarvestWindow ?? 4;
        }
    }
}
