using System.Collections.Generic;

namespace CozyYard
{
    public class VisitorStore : SavableStoreBase<VisitorData>, IVisitorQueries
    {
        protected override string SaveKey => SaveKeys.VisitorData;

        public IReadOnlyList<ActiveOrder> TodayOrders => Data.TodayOrders;
        public bool IsGateOpen => Data.GateOpen;

        public void SetGateOpen(bool open)
        {
            Data.GateOpen = open;
            MarkDirty();
        }

        public void AddOrder(ActiveOrder order)
        {
            Data.TodayOrders.Add(order);
            MarkDirty();
        }

        public void RemoveOrder(ActiveOrder order)
        {
            Data.TodayOrders.Remove(order);
            MarkDirty();
        }

        public void ClearOrders()
        {
            Data.TodayOrders.Clear();
            MarkDirty();
        }
    }
}
