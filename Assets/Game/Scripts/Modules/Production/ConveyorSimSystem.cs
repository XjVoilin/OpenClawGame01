using IsleWorks.Economy;
using IsleWorks.Grid;
using JulyArch;

namespace IsleWorks.Production
{
    /// <summary>
    /// 传送带模拟系统，负责物品的流动逻辑。
    /// </summary>
    public class ConveyorSimSystem : GameSystemBase, IUpdatableSystem
    {
        private float _moveTimer;

        public void OnUpdate(float deltaTime)
        {
            _moveTimer += deltaTime;
            if (_moveTimer < SimConstants.ConveyorMoveInterval) return;
            _moveTimer -= SimConstants.ConveyorMoveInterval;

            var grid = this.Query<IGridQueries>();
            var conveyors = grid.AllConveyors;

            // Reverse iteration to avoid double-pushing in the same tick
            for (int i = conveyors.Count - 1; i >= 0; i--)
            {
                var segment = conveyors[i];
                if (segment.Count <= 0) continue;

                var item = segment.PeekHead();
                if (item == ResourceType.None) continue;

                bool transferred = false;

                if (segment.NextSegmentId > 0)
                {
                    // Try conveyor target
                    var nextConv = grid.GetConveyor(segment.NextSegmentId);
                    if (nextConv != null)
                    {
                        if (nextConv.TryAcceptItem(item))
                        {
                            segment.RemoveItem();
                            transferred = true;
                        }
                    }
                    else
                    {
                        // Try machine target (including port)
                        var nextMachine = grid.GetMachine(segment.NextSegmentId);
                        if (nextMachine != null)
                        {
                            if (nextMachine.MachineTypeId == (int)MachineType.Port)
                            {
                                // Port: deposit item for selling
                                this.Mutate<InventoryStore>(store => store.AddPortProduct(item));
                                segment.RemoveItem();
                                transferred = true;
                            }
                            else if (nextMachine.HasEmptyInputSlot())
                            {
                                nextMachine.AddToInput(item);
                                segment.RemoveItem();
                                transferred = true;
                            }
                        }
                    }
                }

                segment.IsBlocked = !transferred;
            }
        }
    }
}
