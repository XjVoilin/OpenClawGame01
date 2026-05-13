using cfg;
using JulyCore;

namespace OffTrail
{
    public static class CfgTable
    {
        public static TbUIWindow UIWindow => GF.Config.GetTable<TbUIWindow>();
        public static TbItem Item => GF.Config.GetTable<TbItem>();
        public static TbRecipe Recipe => GF.Config.GetTable<TbRecipe>();
        public static TbKnowledge Knowledge => GF.Config.GetTable<TbKnowledge>();
        public static TbRegion Region => GF.Config.GetTable<TbRegion>();
        public static TbCraftStation CraftStation => GF.Config.GetTable<TbCraftStation>();
    }
}
