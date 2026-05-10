using System.Collections.Generic;
using UnityEngine;

namespace IsleWorks.Configs
{
    /// <summary>
    /// 资源配置加载器，加载并缓存资源数据。
    /// </summary>
    public static class ResourceConfigLoader
    {
        private static Dictionary<int, ResourceConfig> _resourceConfigs;

        /// <summary>
        /// 初始化资源配置。
        /// </summary>
        public static void LoadConfigs()
        {
            // TODO: 从 Luban 配表加载资源配置
            _resourceConfigs = new Dictionary<int, ResourceConfig>
            {
                { 101, new ResourceConfig(101, "Wood", 10) },
                { 102, new ResourceConfig(102, "Ore", 15) },
                { 103, new ResourceConfig(103, "Coal", 20) },
                { 201, new ResourceConfig(201, "Plank", 25) },
                { 202, new ResourceConfig(202, "Ingot", 30) }
            };

            Debug.Log("Resource configs loaded.");
        }

        /// <summary>
        /// 获取资源配置。
        /// </summary>
        public static ResourceConfig GetConfig(int resourceId)
        {
            if (_resourceConfigs.TryGetValue(resourceId, out var config))
            {
                return config;
            }

            Debug.LogError($"Resource config not found for ID: {resourceId}");
            return null;
        }
    }

    /// <summary>
    /// 单个资源配置。
    /// </summary>
    public class ResourceConfig
    {
        public int Id { get; }
        public string Name { get; }
        public int SellPrice { get; }

        public ResourceConfig(int id, string name, int sellPrice)
        {
            Id = id;
            Name = name;
            SellPrice = sellPrice;
        }
    }
}