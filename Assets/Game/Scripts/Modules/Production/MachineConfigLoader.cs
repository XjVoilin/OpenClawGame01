using cfg;
using JulyCore;
using JulyCore.Provider.Config;
using UnityEngine;

namespace IsleWorks.Production
{
    /// <summary>
    /// 机器配置加载器，从 Luban 配表读取机器数据。
    /// </summary>
    public static class MachineConfigLoader
    {
        private static TbMachine _table;

        public static void LoadConfigs()
        {
            if (GF.TryResolve<IConfigProvider>(out var provider) && provider.TryGetTable(out TbMachine table))
            {
                _table = table;
                GF.Log($"Machine configs loaded: {table.DataList.Count} machines");
            }
            else
            {
                GF.LogError("Failed to load machine config table");
            }
        }

        public static Machine GetConfig(int machineTypeId)
        {
            return _table?.GetOrDefault(machineTypeId);
        }

        public static Vector2Int GetSize(int machineTypeId)
        {
            var config = GetConfig(machineTypeId);
            return config != null ? new Vector2Int(config.SizeX, config.SizeY) : Vector2Int.one;
        }

        public static int GetCost(int machineTypeId)
        {
            var config = GetConfig(machineTypeId);
            return config?.Cost ?? 0;
        }

        public static float GetRefundRatio(int machineTypeId)
        {
            var config = GetConfig(machineTypeId);
            return config?.RefundRatio ?? 0.5f;
        }

        public static int GetRecipeId(int machineTypeId)
        {
            var config = GetConfig(machineTypeId);
            return config?.RecipeId ?? 0;
        }

        public static int GetInputSlotSize(int machineTypeId)
        {
            var config = GetConfig(machineTypeId);
            return config?.InputSlotSize ?? 0;
        }
    }
}
