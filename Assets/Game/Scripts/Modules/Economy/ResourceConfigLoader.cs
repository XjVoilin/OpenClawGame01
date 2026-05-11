using cfg;
using JulyCore;
using JulyCore.Provider.Config;

namespace IsleWorks.Economy
{
    /// <summary>
    /// 资源配置加载器，从 Luban 配表读取资源数据。
    /// </summary>
    public static class ResourceConfigLoader
    {
        private static TbResource _table;

        public static void LoadConfigs()
        {
            if (GF.TryResolve<IConfigProvider>(out var provider) && provider.TryGetTable(out TbResource table))
            {
                _table = table;
                GF.Log($"Resource configs loaded: {table.DataList.Count} resources");
            }
            else
            {
                GF.LogError("Failed to load resource config table");
            }
        }

        public static ResourceConfig GetConfig(int resourceId)
        {
            var row = _table?.GetOrDefault(resourceId);
            if (row == null)
            {
                GF.LogError($"Resource config not found for ID: {resourceId}");
                return null;
            }
            return new ResourceConfig(row.Id, row.Name, row.SellPrice);
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
