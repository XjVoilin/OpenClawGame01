using Luban;
using SimpleJSON;

namespace cfg
{
    public partial class Tables
    {
        public TbLanguage TbLanguage { get; }
        public TbUIWindow TbUIWindow { get; }
        public TbMachine TbMachine { get; }
        public TbRecipe TbRecipe { get; }
        public TbResource TbResource { get; }
        public TbMilestone TbMilestone { get; }
        public TbTilePrice TbTilePrice { get; }

        public Tables(System.Func<string, JSONNode> loader)
        {
            TbLanguage = new TbLanguage(loader("tblanguage"));
            TbUIWindow = new TbUIWindow(loader("tbuiwindow"));
            TbMachine = new TbMachine(loader("tbmachine"));
            TbRecipe = new TbRecipe(loader("tbrecipe"));
            TbResource = new TbResource(loader("tbresource"));
            TbMilestone = new TbMilestone(loader("tbmilestone"));
            TbTilePrice = new TbTilePrice(loader("tbtileprice"));
            ResolveRef();
        }

        private void ResolveRef()
        {
            TbLanguage.ResolveRef(this);
            TbUIWindow.ResolveRef(this);
            TbMachine.ResolveRef(this);
            TbRecipe.ResolveRef(this);
            TbResource.ResolveRef(this);
            TbMilestone.ResolveRef(this);
            TbTilePrice.ResolveRef(this);
        }
    }
}
