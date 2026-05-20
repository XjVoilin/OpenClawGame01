using System.Collections.Generic;
using JulyArch;

namespace CozyYard
{
    public interface IVisitorQueries : IStoreQueries
    {
        IReadOnlyList<ActiveOrder> TodayOrders { get; }
        bool IsGateOpen { get; }
    }
}
