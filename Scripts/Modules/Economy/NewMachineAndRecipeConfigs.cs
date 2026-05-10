using System.Collections.Generic;
using UnityEngine;

namespace IsleWorks.Configs
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
                { 4, new RecipeConfig(4, new[] { 202, 203 }, 401, 15f) }, // 金属锭 + 塑料 → 机械组件
                { 5, new RecipeConfig(5, new[] { 201, 301 }, 402, 10f) }  // 木板 + 工具 → 复合板
            };
        }

        public static Dictionary<int, MachineConfig> GetCopperEraMachines()
        {
            return new Dictionary<int, MachineConfig>
            {
                { 301, new MachineConfig(301, "组合机", new [] { 202, 203 }, 401, 15f, 2, 2) },
                { 302, new MachineConfig(302, "高级锯木机", new [] { 201, 301 }, 402, 10f, 2, 2) }
            };
        }
    }

    public class MachineConfig
    {
        public int Id { get; }
        public string Name { get; }
        public int[] Inputs { get; }
        public int Output { get; }
        public float ProcessTimeSeconds { get; }
        public int Width { get; } // 网格中尺寸宽
        public int Height { get; } // 网格中尺寸高

        public MachineConfig(int id, string name, int[] inputs, int output, float processTime, int width, int height)
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