using System.Collections.Generic;

namespace CozyYard
{
    public class AnimalStore : SavableStoreBase<AnimalData>, IAnimalQueries
    {
        protected override string SaveKey => SaveKeys.AnimalData;

        public IReadOnlyList<AnimalInstance> Animals => Data.Animals;

        public int CountAnimals(int animalId)
        {
            int count = 0;
            for (int i = 0; i < Data.Animals.Count; i++)
            {
                if (Data.Animals[i].AnimalId == animalId) count++;
            }
            return count;
        }

        public void AddAnimal(AnimalInstance animal)
        {
            Data.Animals.Add(animal);
            MarkDirty();
        }

        public void RemoveAnimal(AnimalInstance animal)
        {
            Data.Animals.Remove(animal);
            MarkDirty();
        }

        public void MarkDirtyExplicit() => MarkDirty();
    }
}
