using JulyArch;

namespace IsleWorks.Tech
{
    public interface ITechQueries : IStoreQueries
    {
        int CurrentEra { get; }
    }
}
