using System.Collections.Generic;
using IsleWorks.Production;

namespace IsleWorks.Economy
{
    /// <summary>
    /// 铜器时代新机器和配方配置。
    /// </summary>
    public static class NewMachineAndRecipeConfigs
    {
        public static Dictionary<int, RecipeConfig> GetCopperEraRecipes()
        {
            return new Dictionary<int, RecipeConfig>
            {
                { 4, new RecipeConfig(4, new[] { ResourceType.Ingot, ResourceType.Plastic }, new[] { 1, 1 }, ResourceType.MechanicalComponent, 15f) },
                { 5, new RecipeConfig(5, new[] { ResourceType.Plank, ResourceType.Tool }, new[] { 1, 1 }, ResourceType.CompositeBoard, 10f) }
            };
        }

        public static Dictionary<int, MachineConfig> GetCopperEraMachines()
        {
            return new Dictionary<int, MachineConfig>
            {
                { 301, new MachineConfig(301, "组合机", new[] { ResourceType.Ingot, ResourceType.Plastic }, ResourceType.MechanicalComponent, 15f, 2, 2) },
                { 302, new MachineConfig(302, "高级锯木机", new[] { ResourceType.Plank, ResourceType.Tool }, ResourceType.CompositeBoard, 10f, 2, 2) }
            };
        }
    }

    public class MachineConfig
    {
        public int Id { get; }
        public string Name { get; }
        public ResourceType[] Inputs { get; }
        public ResourceType Output { get; }
        public float ProcessTimeSeconds { get; }
        public int Width { get; }
        public int Height { get; }

        public MachineConfig(int id, string name, ResourceType[] inputs, ResourceType output, float processTime, int width, int height)
        {
            Id = id;
            Name = name;
            Inputs = inputs;
            Output = output;
            ProcessTimeSeconds = processTime;
            Width = width;
            Height = height;
        }
    }
}
