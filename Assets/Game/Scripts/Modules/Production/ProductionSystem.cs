using UnityEngine;
using JulyArch;
using System.Collections.Generic;

namespace IsleWorks.Production
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

        public void RegisterMachine(MachineInstance machine)
        {
            _machines.Add(machine);
            Debug.Log($"Machine {machine.Id} registered.");
        }

        public void OnUpdate(float deltaTime)
        {
            foreach (var machine in _machines)
            {
                if (machine.IsProcessing)
                {
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
