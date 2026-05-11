using UnityEngine;

namespace IsleWorks.Production
{
    /// <summary>
    /// 机器实例，运行时状态。
    /// </summary>
    public class MachineInstance
    {
        public int Id;
        public int MachineTypeId;
        public Vector2Int Position;
        public Vector2Int Size;
        public ResourceType[] InputSlots;
        public ResourceType OutputSlot;
        public float ProcessTimer;
        public bool IsProcessing;

        public MachineInstance(int id, int machineTypeId, Vector2Int position, Vector2Int size, int inputSlotSize)
        {
            Id = id;
            MachineTypeId = machineTypeId;
            Position = position;
            Size = size;
            InputSlots = new ResourceType[inputSlotSize];
            OutputSlot = ResourceType.None;
        }

        public bool AddToInput(ResourceType resource)
        {
            for (int i = 0; i < InputSlots.Length; i++)
            {
                if (InputSlots[i] == ResourceType.None)
                {
                    InputSlots[i] = resource;
                    return true;
                }
            }
            return false;
        }

        public bool HasEmptyInputSlot()
        {
            for (int i = 0; i < InputSlots.Length; i++)
            {
                if (InputSlots[i] == ResourceType.None) return true;
            }
            return false;
        }
    }
}
