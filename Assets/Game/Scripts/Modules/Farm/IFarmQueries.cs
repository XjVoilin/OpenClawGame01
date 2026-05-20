using System.Collections.Generic;
using JulyArch;

namespace CozyYard
{
    public interface IFarmQueries : IStoreQueries
    {
        IReadOnlyList<CropInstance> Crops { get; }
        CropInstance GetCropAt(int x, int y);
        bool HasCropAt(int x, int y);
    }
}
