using cfg;
using JulyCore;
using JulyCore.Provider.Config;

namespace IsleWorks.Tech
{
    /// <summary>
    /// 里程碑配置加载器，从 Luban 配表读取里程碑数据。
    /// </summary>
    public static class MilestoneConfigLoader
    {
        private static TbMilestone _table;

        public static void LoadConfigs()
        {
            if (GF.TryResolve<IConfigProvider>(out var provider) && provider.TryGetTable(out TbMilestone table))
            {
                _table = table;
                GF.Log($"Milestone configs loaded: {table.DataList.Count} milestones");
            }
            else
            {
                GF.LogError("Failed to load milestone config table");
            }
        }

        public static Milestone GetMilestone(int milestoneId)
        {
            return _table?.GetOrDefault(milestoneId);
        }

        public static Milestone GetMilestoneForEra(int era)
        {
            if (_table == null) return null;
            foreach (var m in _table.DataList)
            {
                if (m.UnlockEra == era) return m;
            }
            return null;
        }

        public static int GetRequiredValueForEra(int era)
        {
            var m = GetMilestoneForEra(era);
            return m?.RequiredValue ?? int.MaxValue;
        }
    }
}
