using Cysharp.Threading.Tasks;
using SpiritHealer.Aot;
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

namespace SpiritHealer
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
            ctx.RegisterStore(new DiagnosisStore());
            ctx.RegisterStore(new GardenStore());
            ctx.RegisterStore(new PrescriptionStore());
            ctx.RegisterStore(new VisitorStore());
            ctx.RegisterStore(new PlayerStore());
            ctx.RegisterStore(new InventoryStore());
        }

        private void RegisterSystems(GameContext ctx)
        {
            ctx.RegisterSystem(new DiagnosisSystem());
            ctx.RegisterSystem(new GardenSystem());
            ctx.RegisterSystem(new PrescriptionSystem());
            ctx.RegisterSystem(new VisitorSystem());
            ctx.RegisterSystem(new TimeSystem());
            ctx.RegisterSystem(new EncounterSystem());
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
