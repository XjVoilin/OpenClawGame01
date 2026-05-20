using JulyArch;

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

    public class AnimalSystem : GameSystemBase
    {
        private AnimalStore _store;
        private InventorySystem _inventorySystem;
        private BuildSystem _buildSystem;

        private struct AnimalConfig
        {
            public int Id;
            public AnimalType Type;
            public int ProduceItemId;
            public int ProduceCycleDays;
            public int RequiredBuildingId;
            public int FeedItemId;
            public int FeedQuantity;
        }

        private static readonly AnimalConfig[] Configs = {
            new() { Id = 1, Type = AnimalType.Poultry, ProduceItemId = 3101, ProduceCycleDays = 2, RequiredBuildingId = 40, FeedItemId = 1001, FeedQuantity = 2 },
            new() { Id = 2, Type = AnimalType.Pet, ProduceItemId = 0, ProduceCycleDays = 0, RequiredBuildingId = 0, FeedItemId = 0, FeedQuantity = 0 },
        };

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

            if (cfg.Value.RequiredBuildingId > 0 && !_buildSystem.HasBuilding(cfg.Value.RequiredBuildingId))
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
            return true;
        }

        public bool FeedAll()
        {
            bool anyFed = false;
            foreach (var animal in _store.Animals)
            {
                var cfg = GetConfig(animal.AnimalId);
                if (cfg == null || cfg.Value.Type != AnimalType.Poultry) continue;
                if (animal.FedToday) continue;

                if (_inventorySystem.HasItem(cfg.Value.FeedItemId, cfg.Value.FeedQuantity))
                {
                    _inventorySystem.RemoveItem(cfg.Value.FeedItemId, cfg.Value.FeedQuantity);
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
            if (cfg == null || cfg.Value.Type != AnimalType.Poultry) return false;
            if (animal.FedToday) return false;

            if (!_inventorySystem.HasItem(cfg.Value.FeedItemId, cfg.Value.FeedQuantity)) return false;

            _inventorySystem.RemoveItem(cfg.Value.FeedItemId, cfg.Value.FeedQuantity);
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

                if (cfg.Value.Type == AnimalType.Poultry)
                {
                    ProcessPoultry(animal, cfg.Value);
                }
                else if (cfg.Value.Type == AnimalType.Pet)
                {
                    ProcessPet(animal, random);
                }
            }

            _store.MarkDirtyExplicit();
        }

        private void ProcessPoultry(AnimalInstance animal, AnimalConfig cfg)
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

        private AnimalConfig? GetConfig(int animalId)
        {
            for (int i = 0; i < Configs.Length; i++)
            {
                if (Configs[i].Id == animalId) return Configs[i];
            }
            return null;
        }
    }
}
