using System;
using UnityEngine;

namespace IsleWorks.Simulation
{
    /// <summary>
    /// 机器实例，用于模拟运行状态。
    /// </summary>
    public class MachineInstance
    {
        public int Id;
        public RecipeConfig CurrentRecipe;
        public ResourceType[] InputSlots;
        public ResourceType OutputSlot;
        public float ProcessTimer;
        public bool IsProcessing;

        public MachineInstance(int id, int inputSlotSize)
        {
            Id = id;
            InputSlots = new ResourceType[inputSlotSize];
            OutputSlot = ResourceType.None;
        }

        /// <summary>
        /// 向输入槽中添加资源。
        /// </summary>
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

            return false; // 没有空位添加传入资源
        }
    }
}