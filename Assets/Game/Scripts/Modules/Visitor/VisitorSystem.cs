using JulyArch;

namespace CozyYard
{
    public struct VisitorArrivedEvent
    {
        public int VisitorId;
        public int RequestedItemId;
        public int RequestedQuantity;
    }

    public struct VisitorLeftEvent
    {
        public int VisitorId;
    }

    public class VisitorSystem : GameSystemBase
    {
        private VisitorStore _store;
        private InventorySystem _inventorySystem;

        private struct VisitorConfig
        {
            public int Id;
            public string Name;
            public int[] OrderItemIds;
            public int[] OrderQuantities;
            public int RewardCoins;
            public int RewardItemId;
            public int RewardItemQty;
            public int VisitChance;
        }

        private static readonly VisitorConfig[] Visitors = {
            new() { Id=1, Name="张阿婆", OrderItemIds=new[]{5001,5003}, OrderQuantities=new[]{1,2}, RewardCoins=30, RewardItemId=0, RewardItemQty=0, VisitChance=40 },
            new() { Id=2, Name="李大爷", OrderItemIds=new[]{5002,5005}, OrderQuantities=new[]{1,1}, RewardCoins=20, RewardItemId=1001, RewardItemQty=3, VisitChance=35 },
            new() { Id=3, Name="小花", OrderItemIds=new[]{5004,5001}, OrderQuantities=new[]{1,1}, RewardCoins=25, RewardItemId=0, RewardItemQty=0, VisitChance=30 },
            new() { Id=4, Name="王货郎", OrderItemIds=new[]{4001,4003,4004}, OrderQuantities=new[]{2,2,2}, RewardCoins=50, RewardItemId=3006, RewardItemQty=2, VisitChance=20 },
        };

        protected override void OnInitialize()
        {
            _store = GetStore<VisitorStore>();
            _inventorySystem = GetSystem<InventorySystem>();

            this.Subscribe<DayChangedEvent>(OnDayChanged);
        }

        public void ToggleGate()
        {
            _store.SetGateOpen(!_store.IsGateOpen);
        }

        public void SetGateOpen(bool open)
        {
            _store.SetGateOpen(open);
        }

        public bool FulfillOrder(ActiveOrder order)
        {
            if (!_inventorySystem.HasItem(order.RequestedItemId, order.RequestedQuantity)) return false;

            _inventorySystem.RemoveItem(order.RequestedItemId, order.RequestedQuantity);

            if (order.RewardCoins > 0)
            {
                _inventorySystem.AddCoins(order.RewardCoins);
            }

            if (order.RewardItemId > 0 && order.RewardItemQty > 0)
            {
                _inventorySystem.AddItem(order.RewardItemId, order.RewardItemQty);
            }

            _store.RemoveOrder(order);

            Publish(new OrderCompletedEvent { VisitorId = order.VisitorId, RewardCoins = order.RewardCoins });
            Publish(new VisitorLeftEvent { VisitorId = order.VisitorId });
            return true;
        }

        public void DismissVisitor(ActiveOrder order)
        {
            _store.RemoveOrder(order);
            Publish(new VisitorLeftEvent { VisitorId = order.VisitorId });
        }

        private void OnDayChanged(DayChangedEvent e)
        {
            GenerateDailyVisitors();
        }

        private void GenerateDailyVisitors()
        {
            _store.ClearOrders();

            if (!_store.IsGateOpen) return;

            var rng = new System.Random();

            for (int i = 0; i < Visitors.Length; i++)
            {
                var v = Visitors[i];
                if (rng.Next(100) >= v.VisitChance) continue;

                int itemIdx = rng.Next(v.OrderItemIds.Length);
                int itemId = v.OrderItemIds[itemIdx];
                int qty = v.OrderQuantities[itemIdx];

                var order = new ActiveOrder
                {
                    VisitorId = v.Id,
                    RequestedItemId = itemId,
                    RequestedQuantity = qty,
                    RewardCoins = v.RewardCoins,
                    RewardItemId = v.RewardItemId,
                    RewardItemQty = v.RewardItemQty
                };

                _store.AddOrder(order);
                Publish(new VisitorArrivedEvent { VisitorId = v.Id, RequestedItemId = itemId, RequestedQuantity = qty });
            }
        }
    }
}
