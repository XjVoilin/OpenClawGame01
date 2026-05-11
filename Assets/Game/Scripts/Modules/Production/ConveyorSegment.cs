using UnityEngine;
using IsleWorks.Grid;

namespace IsleWorks.Production
{
    /// <summary>
    /// 传送带段，环形缓冲区实现的物品队列。
    /// </summary>
    public class ConveyorSegment
    {
        public int Id;
        public Vector2Int Position;
        public Direction Direction;
        public int NextSegmentId;
        public int PrevSegmentId;
        public ResourceType[] Slots;
        public int HeadIndex;
        public int Count;
        public bool IsBlocked;

        public ConveyorSegment(int id, Vector2Int position, Direction direction, int capacity)
        {
            Id = id;
            Position = position;
            Direction = direction;
            NextSegmentId = -1;
            PrevSegmentId = -1;
            Slots = new ResourceType[capacity];
            HeadIndex = 0;
            Count = 0;
        }

        public bool TryAcceptItem(ResourceType item)
        {
            if (Count >= Slots.Length) return false;
            int tail = (HeadIndex + Count) % Slots.Length;
            Slots[tail] = item;
            Count++;
            return true;
        }

        public void RemoveItem()
        {
            if (Count <= 0) return;
            Slots[HeadIndex] = ResourceType.None;
            HeadIndex = (HeadIndex + 1) % Slots.Length;
            Count--;
        }

        public ResourceType PeekHead()
        {
            return Count > 0 ? Slots[HeadIndex] : ResourceType.None;
        }
    }
}
