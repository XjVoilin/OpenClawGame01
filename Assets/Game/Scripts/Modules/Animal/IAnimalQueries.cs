using System.Collections.Generic;
using JulyArch;

namespace CozyYard
{
    public interface IAnimalQueries : IStoreQueries
    {
        IReadOnlyList<AnimalInstance> Animals { get; }
        int CountAnimals(int animalId);
    }
}
