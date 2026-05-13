using Cysharp.Threading.Tasks;
using OffTrail.Aot;
using OffTrail.Crafting;
using OffTrail.Inventory;
using OffTrail.Knowledge;
using OffTrail.Survival;
using OffTrail.World;
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

namespace OffTrail
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
            ctx.RegisterStore(new TimeStore());
            ctx.RegisterStore(new WorldStore());
            ctx.RegisterStore(new SurvivalStore());
            ctx.RegisterStore(new KnowledgeStore());
            ctx.RegisterStore(new InventoryStore());
        }

        private void RegisterSystems(GameContext ctx)
        {
            ctx.RegisterSystem(new DayNightSystem());
            ctx.RegisterSystem(new SurvivalSystem());
            ctx.RegisterSystem(new KnowledgeSystem());
            ctx.RegisterSystem(new CraftingSystem());
        }

        public async UniTask OnGameLaunch()
        {
            ConfigureUI();

            await GF.Scene.SwitchAsync("Main");

            GF.UI.Open(UIWindowId.SurvivalHUD);
        }

        private static void ConfigureUI()
        {
            GF.UI.SetWindowConfig(new LubanUIWindowConfigProvider());
        }
    }
}
