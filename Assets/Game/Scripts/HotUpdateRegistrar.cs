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
using JulyGame.Activity;
using JulyGame.Task;
using JulyGame.RedDot;
using JulyGame.Guide;
using JulyGame.ABTest;
#if JULYGF_DEBUG
using JulyCore.Provider.GM;
#endif

namespace CozyYard
{
    public class HotUpdateRegistrar : IHotUpdateRegistrar, IAppArch
    {
        public IArchContext GetArchitecture() => AppArch.Context;

        public void Register(ArchContext ctx)
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

        private void RegisterStores(ArchContext ctx)
        {
            // JulyGame 通用业务 Store
            ctx.RegisterStore(new ActivityStore());
            ctx.RegisterStore(new TaskStore());
            ctx.RegisterStore(new RedDotStore());
            ctx.RegisterStore(new GuideStore());
            ctx.RegisterStore(new ABTestStore());

            // 项目业务 Store
            ctx.RegisterStore(new GridStore());
            ctx.RegisterStore(new TimeStore());
            ctx.RegisterStore(new InventoryStore());
            ctx.RegisterStore(new FarmStore());
            ctx.RegisterStore(new BuildStore());
            ctx.RegisterStore(new AnimalStore());
            ctx.RegisterStore(new CraftStore());
            ctx.RegisterStore(new VisitorStore());
            ctx.RegisterStore(new MilestoneStore());
            ctx.RegisterStore(new WeatherStore());
        }

        private void RegisterSystems(ArchContext ctx)
        {
            // JulyGame 通用业务 System
            ctx.RegisterSystem(new ActivitySystem());
            ctx.RegisterSystem(new TaskSystem());
            ctx.RegisterSystem(new RedDotSystem());
            ctx.RegisterSystem(new GuideSystem());
            ctx.RegisterSystem(new ABTestSystem());

            // 项目业务 System
            ctx.RegisterSystem(new GridSystem());
            ctx.RegisterSystem(new TimeSystem());
            ctx.RegisterSystem(new WeatherSystem());
            ctx.RegisterSystem(new InventorySystem());
            ctx.RegisterSystem(new FarmSystem());
            ctx.RegisterSystem(new BuildSystem());
            ctx.RegisterSystem(new AnimalSystem());
            ctx.RegisterSystem(new CraftSystem());
            ctx.RegisterSystem(new VisitorSystem());
            ctx.RegisterSystem(new MilestoneSystem());
            ctx.RegisterSystem(new SceneFlowSystem());
        }

        public async UniTask OnGameLaunch()
        {
            ConfigureUI();
            await GF.Scene.SwitchAsync("Main");
        }

        private static void ConfigureUI()
        {
            GF.UI.SetWindowConfig(new LubanUIWindowConfigProvider());
        }
    }
}
