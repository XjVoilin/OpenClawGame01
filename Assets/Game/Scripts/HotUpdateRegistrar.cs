using Cysharp.Threading.Tasks;
using IsleWorks.Aot;
using IsleWorks.Economy;
using IsleWorks.Grid;
using IsleWorks.Island;
using IsleWorks.Production;
using IsleWorks.Tech;
using JulyArch;
using JulyCore;
using JulyCore.Provider.Config;
using JulyCore.Provider.Localization;
using JulyCore.Provider.Resource;
using JulyCore.Provider.Save;
using JulyCore.Provider.UI;
using JulyCore.Provider.Audio;
using JulyCore.Provider.Pool;
#if JULYGF_DEBUG
using JulyCore.Provider.GM;
#endif

namespace IsleWorks
{
    /// <summary>
    /// 热更程序集注册入口。
    /// 框架在加载热更 DLL 后通过反射发现此类并调用，所有业务类型注册集中在此完成。
    /// </summary>
    public class HotUpdateRegistrar : IHotUpdateRegistrar, IAppArch
    {
        public IGameContext GetArchitecture() => AppArch.Context;

        public void Register(GameContext ctx)
        {
            RegisterProviders();
            RegisterStores(ctx);
            RegisterSystems(ctx);
        }

        private void RegisterProviders()
        {
            var resourceProvider = GF.Resolve<IResourceProvider>();
            var poolProvider = GF.Resolve<IPoolProvider>();

            var saveProvider = new PlayerPrefsSaveProvider();
            GF.RegisterProvider<ISaveProvider>(saveProvider);

            var configProvider = new LubanConfigProvider(resourceProvider);
            GF.RegisterProvider<IConfigProvider>(configProvider);

            GF.RegisterProvider<ILocalizationProvider>(new LubanLocalizationProvider(configProvider));
            GF.RegisterProvider<IUIProvider>(new UIProvider(resourceProvider, poolProvider));
            GF.RegisterProvider<IAudioProvider>(new UnityAudioProvider(resourceProvider, poolProvider));

#if JULYGF_DEBUG
            RegisterGMCommands();
#endif
        }

#if JULYGF_DEBUG
        private static void RegisterGMCommands()
        {

        }
#endif

        private void RegisterStores(GameContext ctx)
        {
            ctx.RegisterStore(new GridStore());
            ctx.RegisterStore(new InventoryStore());
            ctx.RegisterStore(new TechStore());
        }

        private void RegisterSystems(GameContext ctx)
        {
            ctx.RegisterSystem(new BuildSystem());
            ctx.RegisterSystem(new EconomySystem());
            ctx.RegisterSystem(new IslandSystem());
            ctx.RegisterSystem(new ProductionSystem());
            ctx.RegisterSystem(new ConveyorSimSystem());
            ctx.RegisterSystem(new TechSystem());
        }

        public async UniTask OnGameLaunch()
        {
            ConfigureUI();

            await GF.Scene.SwitchAsync("Main");

            GF.UI.Open(UIWindowId.GameHUD);
            GF.UI.Open(UIWindowId.BuildWindow);
        }

        private static void ConfigureUI()
        {
            GF.UI.SetWindowConfig(new LubanUIWindowConfigProvider());
        }
    }
}
