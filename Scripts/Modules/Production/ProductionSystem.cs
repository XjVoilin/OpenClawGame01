using UnityEngine;
using IsleWorks.Data;
using JulyArch;
using System.Collections.Generic;

namespace IsleWorks.Systems
{
    /// <summary>
    /// 加工系统，负责机器的加工逻辑。
    /// </summary>
    public class ProductionSystem : GameSystemBase, IUpdatableSystem
    {
        private List<MachineInstance> _machines;

        public ProductionSystem()
        {
            _machines = new List<MachineInstance>();
        }

        /// <summary>
        /// 注册机器。
        /// </summary>
        public void RegisterMachine(MachineInstance machine)
        {
            _machines.Add(machine);
            Debug.Log($"Machine {machine.Id} registered.");
        }

        /// <summary>
        /// 每帧更新。
        /// </summary>
        public void OnUpdate(float deltaTime)
        {
            foreach (var machine in _machines)
            {
                if (machine.IsProcessing)
                {
                    // 继续加工计时
                    machine.ProcessTimer -= deltaTime;
                    if (machine.ProcessTimer <= 0)
                    {
                        machine.OutputSlot = machine.CurrentRecipe.Output;
                        machine.IsProcessing = false;
                        Debug.Log($"Machine {machine.Id} completed processing {machine.OutputSlot}.");
                    }
                }
                else if (machine.OutputSlot == ResourceType.None)
                {
                    // 检查输入是否满足配方
                    if (IsInputReady(machine))
                    {
                        ConsumeInput(machine);
                        machine.ProcessTimer = machine.CurrentRecipe.ProcessTime;
                        machine.IsProcessing = true;
                        Debug.Log($"Machine {machine.Id} started processing {machine.CurrentRecipe.Output}.");
                    }
                }
            }
        }

        /// <summary>
        /// 检查输入是否满足配方。
        /// </summary>
        private bool IsInputReady(MachineInstance machine)
        {
            var recipe = machine.CurrentRecipe;
            for (int i = 0; i < recipe.Inputs.Length; i++)
            {
                if (machine.InputSlots[i] != recipe.Inputs[i]) return false;
            }
            return true;
        }

        /// <summary>
        /// 消耗机器输入。
        /// </summary>
        private void ConsumeInput(MachineInstance machine)
        {
            for (int i = 0; i < machine.InputSlots.Length; i++)
            {
                machine.InputSlots[i] = ResourceType.None;
            }
        }
    }
}