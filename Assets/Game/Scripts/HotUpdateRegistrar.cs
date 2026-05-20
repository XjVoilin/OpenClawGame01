using Cysharp.Threading.Tasks;
using CozyYard.Aot;
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

namespace CozyYard
{
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
            ctx.RegisterStore(new TimeStore());
            ctx.RegisterStore(new InventoryStore());
            ctx.RegisterStore(new FarmStore());
            ctx.RegisterStore(new BuildStore());
            ctx.RegisterStore(new AnimalStore());
        }

        private void RegisterSystems(GameContext ctx)
        {
            ctx.RegisterSystem(new GridSystem());
            ctx.RegisterSystem(new TimeSystem());
            ctx.RegisterSystem(new InventorySystem());
            ctx.RegisterSystem(new FarmSystem());
            ctx.RegisterSystem(new BuildSystem());
            ctx.RegisterSystem(new AnimalSystem());
        }

        public async UniTask OnGameLaunch()
        {
            ConfigureUI();

            await GF.Scene.SwitchAsync("Main");

            var gridSystem = AppArch.Context.GetSystem<GridSystem>();
            if (gridSystem.Width == 0)
            {
                gridSystem.InitializeNewGrid(12, 12);
            }

            var timeSystem = AppArch.Context.GetSystem<TimeSystem>();
            timeSystem.EnsureDayStarted();

            GF.UI.Open(UIWindowId.GameHUD);
        }

        private static void ConfigureUI()
        {
            GF.UI.SetWindowConfig(new LubanUIWindowConfigProvider());
        }
    }
}
