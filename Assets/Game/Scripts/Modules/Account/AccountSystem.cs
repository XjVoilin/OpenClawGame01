using cfg;
using JulyArch;
using JulyCore;

namespace CozyYard
{
    /// <summary>账号系统：管理玩家档案，编排新账号的数据初始化（只操作 Store，不调用 System 方法）。</summary>
    public class AccountSystem : GameSystemBase
    {
        private AccountStore _store;
        private GridStore _gridStore;
        private TimeStore _timeStore;
        private InventoryStore _inventoryStore;
        private CraftStore _craftStore;

        protected override void OnInitialize()
        {
            _store = GetStore<AccountStore>();
            _gridStore = GetStore<GridStore>();
            _timeStore = GetStore<TimeStore>();
            _inventoryStore = GetStore<InventoryStore>();
            _craftStore = GetStore<CraftStore>();
        }

        protected override void OnStart()
        {
            if (!_store.Initialized)
                SetupNewAccount();
            else
                _store.UpdateLoginInfo();
        }

        private void SetupNewAccount()
        {
            var cfg = GF.Config.GetTable<TbGameConfig>();
            if (cfg == null) return;

            _store.CreateProfile();

            _gridStore.InitializeGrid(cfg.GridWidth, cfg.GridHeight);
            _timeStore.SetInitialTime(cfg.StartSeasonIndex, cfg.StartMinuteOfDay,
                                      cfg.StartYear, cfg.StartDayInSeason);
            _inventoryStore.SetCapacity(cfg.InventoryCapacity);
            GrantStartingResources();
            GrantStarterRecipes(cfg);

            _store.SetInitialized();
        }

        private void GrantStartingResources()
        {
            var table = GF.Config.GetTable<TbStartingResource>();
            if (table == null) return;

            foreach (var entry in table.DataList)
            {
                if (entry.ItemId == 0)
                    _inventoryStore.AddCoins(entry.Quantity);
                else
                    _inventoryStore.AddItem(entry.ItemId, entry.Quantity);
            }
        }

        private void GrantStarterRecipes(TbGameConfig cfg)
        {
            foreach (var id in cfg.StarterRecipeIds)
                _craftStore.UnlockRecipe(id);
        }
    }
}
