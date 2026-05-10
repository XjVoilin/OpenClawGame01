namespace IsleWorks.Production
{
    /// <summary>
    /// 传送带段，环形缓冲区实现的物品队列。
    /// </summary>
    public class ConveyorSegment
    {
        public int Id;
        public int NextSegmentId;
        public ResourceType[] Slots;
        public int HeadIndex;
        public int Count;
        public bool IsBlocked;

        public ConveyorSegment(int id, int capacity, int nextSegmentId)
        {
            Id = id;
            NextSegmentId = nextSegmentId;
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
    }
}
