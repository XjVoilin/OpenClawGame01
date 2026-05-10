using System.Collections.Generic;
using UnityEngine;

namespace IsleWorks.Production
{
    /// <summary>
    /// 配方配置加载器，加载并缓存加工配方数据。
    /// </summary>
    public static class RecipeConfigLoader
    {
        private static Dictionary<int, RecipeConfig> _recipeConfigs;

        public static void LoadConfigs()
        {
            // TODO: 从 Luban 配表加载配方配置
            _recipeConfigs = new Dictionary<int, RecipeConfig>
            {
                { 1, new RecipeConfig(1, new[] { ResourceType.Wood }, new[] { 1 }, ResourceType.Plank, 5f) },
                { 2, new RecipeConfig(2, new[] { ResourceType.Ore }, new[] { 1 }, ResourceType.Ingot, 5f) },
                { 3, new RecipeConfig(3, new[] { ResourceType.Ingot, ResourceType.Coal }, new[] { 1, 1 }, ResourceType.Tool, 10f) }
            };

            Debug.Log("Recipe configs loaded.");

            foreach (var recipe in _recipeConfigs.Values)
            {
                if (recipe.Inputs == null || recipe.Inputs.Length == 0)
                {
                    Debug.LogError($"Invalid recipe: Missing inputs for Recipe ID {recipe.Id}");
                }

                if (recipe.Output == ResourceType.None)
                {
                    Debug.LogError($"Invalid recipe: Missing output for Recipe ID {recipe.Id}");
                }
            }
        }

        public static RecipeConfig GetRecipe(int recipeId)
        {
            if (_recipeConfigs != null && _recipeConfigs.TryGetValue(recipeId, out var config))
            {
                return config;
            }

            Debug.LogError($"Recipe config not found for ID: {recipeId}");
            return null;
        }
    }

    public class RecipeConfig
    {
        public int Id { get; }
        public ResourceType[] Inputs { get; }
        public int[] InputQuantities { get; }
        public ResourceType Output { get; }
        public float ProcessTime { get; }

        public RecipeConfig(int id, ResourceType[] inputs, int[] inputQuantities, ResourceType output, float processTime)
        {
            Id = id;
            Inputs = inputs;
            InputQuantities = inputQuantities;
            Output = output;
            ProcessTime = processTime;
        }
    }
}
