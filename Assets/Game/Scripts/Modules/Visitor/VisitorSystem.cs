using cfg;
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

    /// <summary>来客系统：每日根据概率和天气生成来客订单，处理订单完成与拒绝，支持大门开关控制。</summary>
    public class VisitorSystem : GameSystemBase
    {
        private VisitorStore _store;
        private InventorySystem _inventorySystem;

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
            if (CfgTable.Tables == null) return;

            var rng = new System.Random();
            var weatherModifier = GetSystem<WeatherSystem>().GetVisitorChanceModifier();

            foreach (var visitor in CfgTable.Tables.TbVisitor.DataList)
            {
                int adjustedChance = (int)(visitor.VisitChance * weatherModifier);
                if (rng.Next(100) >= adjustedChance) continue;

                int itemIdx = rng.Next(visitor.OrderItemIds.Count);
                int itemId = visitor.OrderItemIds[itemIdx];
                int qty = visitor.OrderQuantities[itemIdx];

                var order = new ActiveOrder
                {
                    VisitorId = visitor.Id,
                    RequestedItemId = itemId,
                    RequestedQuantity = qty,
                    RewardCoins = visitor.RewardCoins,
                    RewardItemId = visitor.RewardItemId,
                    RewardItemQty = visitor.RewardItemQty
                };

                _store.AddOrder(order);
                Publish(new VisitorArrivedEvent { VisitorId = visitor.Id, RequestedItemId = itemId, RequestedQuantity = qty });
            }
        }
    }
}
