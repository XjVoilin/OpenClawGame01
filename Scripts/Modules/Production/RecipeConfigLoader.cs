using System.Collections.Generic;
using UnityEngine;

namespace IsleWorks.Configs
{
    /// <summary>
    /// 配方配置加载器，加载并缓存加工配方数据。
    /// </summary>
    public static class RecipeConfigLoader
    {
        private static Dictionary<int, RecipeConfig> _recipeConfigs;

        /// <summary>
        /// 初始化配方配置。
        /// </summary>
        public static void LoadConfigs()
        {
            // TODO: 从 Luban 配表加载配方配置
            _recipeConfigs = new Dictionary<int, RecipeConfig>
            {
                { 1, new RecipeConfig(1, new[] { 101 }, 201, 5f) }, // 木材 → 木板
                { 2, new RecipeConfig(2, new[] { 102 }, 202, 5f) }, // 矿石 → 金属锭
                { 3, new RecipeConfig(3, new[] { 102, 103 }, 301, 10f) } // 金属锭+煤炭 → 工具
            };

            Debug.Log("Recipe configs loaded.");
        }

        /// <summary>
        /// 获取配方配置。
        /// </summary>
        public static RecipeConfig GetRecipe(int recipeId)
        {
            if (_recipeConfigs.TryGetValue(recipeId, out var config))
            {
                return config;
            }

            Debug.LogError($"Recipe config not found for ID: {recipeId}");
            return null;
        }
    }

    /// <summary>
    /// 单个配方配置。
    /// </summary>
    public class RecipeConfig
    {
        public int Id { get; }
        public int[] Inputs { get; }
        public int Output { get; }
        public float Time { get; }

        public RecipeConfig(int id, int[] inputs, int output, float time)
        {
            Id = id;
            Inputs = inputs;
            Output = output;
            Time = time;
        }
    }
}