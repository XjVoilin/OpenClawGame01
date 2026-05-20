using cfg;
using JulyArch;
using JulyCore;

namespace CozyYard
{
    public struct AnimalFedEvent
    {
        public int AnimalId;
    }

    public struct AnimalProducedEvent
    {
        public int AnimalId;
        public int ItemId;
        public int Quantity;
    }

    public struct PetGiftEvent
    {
        public int AnimalId;
        public int ItemId;
    }

    /// <summary>动物系统：管理动物的收养、每日喂食、产出周期计算，以及宠物随机礼物。</summary>
    public class AnimalSystem : GameSystemBase
    {
        private AnimalStore _store;
        private InventorySystem _inventorySystem;
        private BuildSystem _buildSystem;

        protected override void OnInitialize()
        {
            _store = GetStore<AnimalStore>();
            _inventorySystem = GetSystem<InventorySystem>();
            _buildSystem = GetSystem<BuildSystem>();

            this.Subscribe<DayChangedEvent>(OnDayChanged);
        }

        public bool CanAdopt(int animalId)
        {
            var cfg = GetConfig(animalId);
            if (cfg == null) return false;

            if (cfg.RequiredBuildingId > 0 && !_buildSystem.HasBuilding(cfg.RequiredBuildingId))
                return false;

            return true;
        }

        public bool AdoptAnimal(int animalId)
        {
            if (!CanAdopt(animalId)) return false;

            _store.AddAnimal(new AnimalInstance
            {
                AnimalId = animalId,
                DaysSinceLastFed = 0,
                DaysSinceLastProduce = 0,
                FedToday = false
            });
            GetSystem<MilestoneSystem>().NotifyAdoptAnimal();
            return true;
        }

        public bool FeedAll()
        {
            bool anyFed = false;
            foreach (var animal in _store.Animals)
            {
                var cfg = GetConfig(animal.AnimalId);
                if (cfg == null || !IsPoultry(cfg)) continue;
                if (animal.FedToday) continue;

                if (_inventorySystem.HasItem(cfg.FeedItemId, cfg.FeedQuantity))
                {
                    _inventorySystem.RemoveItem(cfg.FeedItemId, cfg.FeedQuantity);
                    animal.FedToday = true;
                    anyFed = true;
                }
            }

            if (anyFed)
            {
                _store.MarkDirtyExplicit();
                Publish(new AnimalFedEvent { AnimalId = 0 });
            }
            return anyFed;
        }

        public bool FeedAnimal(AnimalInstance animal)
        {
            var cfg = GetConfig(animal.AnimalId);
            if (cfg == null || !IsPoultry(cfg)) return false;
            if (animal.FedToday) return false;

            if (!_inventorySystem.HasItem(cfg.FeedItemId, cfg.FeedQuantity)) return false;

            _inventorySystem.RemoveItem(cfg.FeedItemId, cfg.FeedQuantity);
            animal.FedToday = true;
            _store.MarkDirtyExplicit();

            Publish(new AnimalFedEvent { AnimalId = animal.AnimalId });
            return true;
        }

        private void OnDayChanged(DayChangedEvent e)
        {
            ProcessDaily();
        }

        private void ProcessDaily()
        {
            var random = new System.Random();

            for (int i = 0; i < _store.Animals.Count; i++)
            {
                var animal = (AnimalInstance)_store.Animals[i];
                var cfg = GetConfig(animal.AnimalId);
                if (cfg == null) continue;

                if (IsPoultry(cfg))
                {
                    ProcessPoultry(animal, cfg);
                }
                else if (IsPet(cfg))
                {
                    ProcessPet(animal, random);
                }
            }

            _store.MarkDirtyExplicit();
        }

        private void ProcessPoultry(AnimalInstance animal, Animal cfg)
        {
            if (!animal.FedToday)
            {
                animal.DaysSinceLastFed++;
            }
            else
            {
                animal.DaysSinceLastFed = 0;
            }

            animal.FedToday = false;
            animal.DaysSinceLastProduce++;

            int requiredDays = cfg.ProduceCycleDays;
            if (animal.DaysSinceLastFed >= 2)
            {
                requiredDays += animal.DaysSinceLastFed;
            }

            if (animal.DaysSinceLastProduce >= requiredDays && animal.DaysSinceLastFed == 0)
            {
                animal.DaysSinceLastProduce = 0;
                _inventorySystem.AddItem(cfg.ProduceItemId, 1);
                Publish(new AnimalProducedEvent { AnimalId = animal.AnimalId, ItemId = cfg.ProduceItemId, Quantity = 1 });
            }
        }

        private void ProcessPet(AnimalInstance animal, System.Random random)
        {
            if (random.Next(100) < 5)
            {
                int[] possibleGifts = { 1001, 1002, 1003 };
                int giftId = possibleGifts[random.Next(possibleGifts.Length)];
                _inventorySystem.AddItem(giftId, 1);
                Publish(new PetGiftEvent { AnimalId = animal.AnimalId, ItemId = giftId });
            }
        }

        private static bool IsPoultry(Animal cfg) => cfg.Type == nameof(AnimalType.Poultry);

        private static bool IsPet(Animal cfg) => cfg.Type == nameof(AnimalType.Pet);

        private Animal GetConfig(int animalId)
        {
            return GF.Config.GetTable<TbAnimal>()?.GetOrDefault(animalId);
        }
    }
}
