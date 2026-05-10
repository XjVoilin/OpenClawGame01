using JulyArch;

namespace IsleWorks.Economy
{
    public interface IInventoryQueries : IStoreQueries
    {
        int Gold { get; }
        int TotalProductionValue { get; }
    }
}
