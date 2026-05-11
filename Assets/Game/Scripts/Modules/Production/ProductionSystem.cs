using IsleWorks.Grid;
using JulyArch;

namespace IsleWorks.Production
{
    /// <summary>
    /// 加工系统，负责机器的加工逻辑。
    /// </summary>
    public class ProductionSystem : GameSystemBase, IUpdatableSystem
    {
        public void OnUpdate(float deltaTime)
        {
            var grid = this.Query<IGridQueries>();
            var machines = grid.AllMachines;

            for (int i = 0; i < machines.Count; i++)
            {
                var machine = machines[i];

                // Skip non-production machines
                if (machine.MachineTypeId == (int)MachineType.Conveyor) continue;
                if (machine.MachineTypeId == (int)MachineType.Port) continue;
                if (machine.MachineTypeId == (int)MachineType.Generator) continue;
                if (machine.MachineTypeId == (int)MachineType.Wire) continue;
                if (machine.MachineTypeId == (int)MachineType.Sorter) continue;

                if (machine.IsProcessing)
                {
                    machine.ProcessTimer -= deltaTime;
                    if (machine.ProcessTimer <= 0)
                    {
                        CompleteProcessing(machine, grid);
                    }
                }
                else if (machine.OutputSlot == ResourceType.None)
                {
                    TryStartProcessing(machine, grid);
                }
            }
        }

        private void TryStartProcessing(MachineInstance machine, IGridQueries grid)
        {
            // Miner: no input needed, just check if on resource node
            if (machine.MachineTypeId == (int)MachineType.Miner)
            {
                var resourceNode = grid.GetResourceNode(machine.Position.x, machine.Position.y);
                if (resourceNode != ResourceType.None)
                {
                    machine.ProcessTimer = 2.0f;
                    machine.IsProcessing = true;
                }
                return;
            }

            // Other machines: check recipe inputs
            int recipeId = MachineConfigLoader.GetRecipeId(machine.MachineTypeId);
            var recipe = RecipeConfigLoader.GetRecipe(recipeId);
            if (recipe == null) return;

            if (IsInputReady(machine, recipe))
            {
                ConsumeInput(machine, recipe);
                machine.ProcessTimer = recipe.ProcessTime;
                machine.IsProcessing = true;
            }
        }

        private void CompleteProcessing(MachineInstance machine, IGridQueries grid)
        {
            // Miner: output the resource node type
            if (machine.MachineTypeId == (int)MachineType.Miner)
            {
                var resourceNode = grid.GetResourceNode(machine.Position.x, machine.Position.y);
                machine.OutputSlot = resourceNode;
            }
            else
            {
                int recipeId = MachineConfigLoader.GetRecipeId(machine.MachineTypeId);
                var recipe = RecipeConfigLoader.GetRecipe(recipeId);
                if (recipe != null)
                {
                    machine.OutputSlot = recipe.Output;
                }
            }

            machine.IsProcessing = false;

            // Try to push output to downstream conveyor
            PushOutputToConveyor(machine, grid);
        }

        private void PushOutputToConveyor(MachineInstance machine, IGridQueries grid)
        {
            if (machine.OutputSlot == ResourceType.None) return;

            // Find a conveyor that has this machine as its upstream (PrevSegmentId)
            for (int i = 0; i < grid.AllConveyors.Count; i++)
            {
                var conv = grid.AllConveyors[i];
                if (conv.PrevSegmentId == machine.Id)
                {
                    if (conv.TryAcceptItem(machine.OutputSlot))
                    {
                        machine.OutputSlot = ResourceType.None;
                        return;
                    }
                }
            }
        }

        private bool IsInputReady(MachineInstance machine, RecipeConfig recipe)
        {
            for (int r = 0; r < recipe.Inputs.Length; r++)
            {
                int needed = recipe.InputQuantities[r];
                int found = 0;
                for (int s = 0; s < machine.InputSlots.Length; s++)
                {
                    if (machine.InputSlots[s] == recipe.Inputs[r])
                    {
                        found++;
                        if (found >= needed) break;
                    }
                }
                if (found < needed) return false;
            }
            return true;
        }

        private void ConsumeInput(MachineInstance machine, RecipeConfig recipe)
        {
            for (int r = 0; r < recipe.Inputs.Length; r++)
            {
                int remaining = recipe.InputQuantities[r];
                for (int s = 0; s < machine.InputSlots.Length && remaining > 0; s++)
                {
                    if (machine.InputSlots[s] == recipe.Inputs[r])
                    {
                        machine.InputSlots[s] = ResourceType.None;
                        remaining--;
                    }
                }
            }
        }
    }
}
