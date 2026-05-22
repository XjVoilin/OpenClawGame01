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
using JulyGame;
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
    public class HotUpdateRegistrar : IHotUpdateRegistrar, IArchNode
    {
        public IArchContext GetArchitecture() => GameArch.Context;

        public void Register()
        {
            RegisterProviders();
            RegisterStores();
            RegisterSystems();
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

        private void RegisterStores()
        {
            // JulyGame 通用业务 Store
            GameArch.Context.RegisterStore(new ActivityStore());
            GameArch.Context.RegisterStore(new TaskStore());
            GameArch.Context.RegisterStore(new RedDotStore());
            GameArch.Context.RegisterStore(new GuideStore());
            GameArch.Context.RegisterStore(new ABTestStore());

            // 项目业务 Store
            GameArch.Context.RegisterStore(new GridStore());
            GameArch.Context.RegisterStore(new TimeStore());
            GameArch.Context.RegisterStore(new InventoryStore());
            GameArch.Context.RegisterStore(new FarmStore());
            GameArch.Context.RegisterStore(new BuildStore());
            GameArch.Context.RegisterStore(new AnimalStore());
            GameArch.Context.RegisterStore(new CraftStore());
            GameArch.Context.RegisterStore(new VisitorStore());
            GameArch.Context.RegisterStore(new MilestoneStore());
            GameArch.Context.RegisterStore(new WeatherStore());
        }

        private void RegisterSystems()
        {
            // JulyGame 通用业务 System
            GameArch.Context.RegisterSystem(new ActivitySystem());
            GameArch.Context.RegisterSystem(new TaskSystem());
            GameArch.Context.RegisterSystem(new RedDotSystem());
            GameArch.Context.RegisterSystem(new GuideSystem());
            GameArch.Context.RegisterSystem(new ABTestSystem());

            // 项目业务 System
            GameArch.Context.RegisterSystem(new GridSystem());
            GameArch.Context.RegisterSystem(new TimeSystem());
            GameArch.Context.RegisterSystem(new WeatherSystem());
            GameArch.Context.RegisterSystem(new InventorySystem());
            GameArch.Context.RegisterSystem(new FarmSystem());
            GameArch.Context.RegisterSystem(new BuildSystem());
            GameArch.Context.RegisterSystem(new AnimalSystem());
            GameArch.Context.RegisterSystem(new CraftSystem());
            GameArch.Context.RegisterSystem(new VisitorSystem());
            GameArch.Context.RegisterSystem(new MilestoneSystem());
            GameArch.Context.RegisterSystem(new SceneFlowSystem());
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
