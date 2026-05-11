using System.Collections.Generic;
using IsleWorks.Production;
using JulyArch;

namespace IsleWorks.Economy
{
    public interface IInventoryQueries : IStoreQueries
    {
        int Gold { get; }
        int TotalProductionValue { get; }
        IReadOnlyList<ResourceType> PortProducts { get; }
    }
}
