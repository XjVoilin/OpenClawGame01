using System;
using System.Collections.Generic;

namespace cfg
{
    public partial class Tables
    {
        public static readonly string[] TableNames =
        {
            "tblanguage", "tbuiwindow",
            "tbmachine", "tbrecipe", "tbresource", "tbmilestone", "tbtileprice"
        };

        public void RegisterTo(Dictionary<Type, object> registry)
        {
            registry[typeof(TbLanguage)] = TbLanguage;
            registry[typeof(TbUIWindow)] = TbUIWindow;
            registry[typeof(TbMachine)] = TbMachine;
            registry[typeof(TbRecipe)] = TbRecipe;
            registry[typeof(TbResource)] = TbResource;
            registry[typeof(TbMilestone)] = TbMilestone;
            registry[typeof(TbTilePrice)] = TbTilePrice;
        }
    }
}
