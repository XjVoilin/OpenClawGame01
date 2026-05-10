using UnityEngine;
using IsleWorks.Data;
using JulyArch;
using System.Collections.Generic;

namespace IsleWorks.Systems
{
    /// <summary>
    /// 加工系统，负责机器的加工逻辑，包括动态加载配方机制。
    /// </summary>
    public class ProductionSystem : GameSystemBase, IUpdatableSystem
    {
        private List<MachineInstance> _machines;
        [Inject] private RecipeConfigLoader _recipeLoader; // 引入配方加载器

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
                        // 动态读取输出配置
                        var outputConfig = _recipeLoader.GetRecipe(machine.CurrentRecipe.Id);
                        machine.OutputSlot = outputConfig.Output;
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
                int inputCount = 0;
                foreach (var slot in machine.InputSlots)
                {
                    if (slot == recipe.Inputs[i])
                    {
                        inputCount++;
                        if (inputCount >= recipe.InputQuantities[i]) break;
                    }
                }

                if (inputCount < recipe.InputQuantities[i]) return false;
            }
            return true;
        }

        /// <summary>
        /// 消耗机器输入。
        /// </summary>
        private void ConsumeInput(MachineInstance machine)
        {
            var recipe = machine.CurrentRecipe;
            for (int i = 0; i < recipe.Inputs.Length; i++)
            {
                int requiredQuantity = recipe.InputQuantities[i];
                for (int j = 0; j < machine.InputSlots.Length && requiredQuantity > 0; j++)
                {
                    if (machine.InputSlots[j] == recipe.Inputs[i])
                    {
                        machine.InputSlots[j] = ResourceType.None;
                        requiredQuantity--;
                    }
                }
            }
        }
    }
}