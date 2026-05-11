using cfg;
using JulyCore;

namespace IsleWorks
{
    /// <summary>
    /// 常用配置表快捷入口，避免反复写 GF.Config.GetTable
    /// </summary>
    public static class CfgTable
    {
        public static TbMachine Machine => GF.Config.GetTable<TbMachine>();
        public static TbRecipe Recipe => GF.Config.GetTable<TbRecipe>();
        public static TbResource Resource => GF.Config.GetTable<TbResource>();
        public static TbMilestone Milestone => GF.Config.GetTable<TbMilestone>();
        public static TbTilePrice TilePrice => GF.Config.GetTable<TbTilePrice>();
        public static TbUIWindow UIWindow => GF.Config.GetTable<TbUIWindow>();
    }
}
