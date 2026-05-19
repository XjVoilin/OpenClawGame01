using cfg;
using JulyCore;

namespace SpiritHealer
{
    public static class CfgTable
    {
        public static TbUIWindow UIWindow => GF.Config.GetTable<TbUIWindow>();
        public static TbTime Time => GF.Config.GetTable<TbTime>();
        public static TbReputation Reputation => GF.Config.GetTable<TbReputation>();
        public static TbHerb Herb => GF.Config.GetTable<TbHerb>();
        public static TbCause Cause => GF.Config.GetTable<TbCause>();
        public static TbSymptom Symptom => GF.Config.GetTable<TbSymptom>();
        public static TbVisitorTemplate VisitorTemplate => GF.Config.GetTable<TbVisitorTemplate>();
    }
}
