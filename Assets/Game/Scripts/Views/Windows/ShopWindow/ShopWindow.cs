using System.Collections.Generic;
using cfg;
using JulyCore;
using TMPro;
using UnityEngine;

namespace CozyYard
{
    public class ShopWindow : GameUIView
    {
        [SerializeField] private Transform _listContainer;
        [SerializeField] private ShopEntry _entryPrefab;
        [SerializeField] private TextMeshProUGUI _coinsText;
        [SerializeField] private UISmartButton _closeBtn;

        private readonly List<ShopEntry> _entries = new();

        protected override void OnViewEnable()
        {
            Subscribe<ShopPurchaseEvent>(OnPurchased);
            Subscribe<InventoryChangedEvent>(OnInventoryChanged);
            if (_closeBtn) _closeBtn.onClick.AddListener(OnClose);
            Refresh();
        }

        protected override void OnViewDisable()
        {
            if (_closeBtn) _closeBtn.onClick.RemoveAllListeners();
            ClearEntries();
        }

        private void OnPurchased(ShopPurchaseEvent e) => Refresh();
        private void OnInventoryChanged(InventoryChangedEvent e) => Refresh();

        private void Refresh()
        {
            ClearEntries();

            var invStore = GetStore<InventoryStore>();
            if (_coinsText) _coinsText.text = invStore.Coins.ToString();

            if (_entryPrefab == null || _listContainer == null) return;

            var tbShop = GF.Config.GetTable<TbShop>();
            var tbItem = GF.Config.GetTable<TbItem>();
            if (tbShop == null) return;

            var shopSystem = GetSystem<ShopSystem>();
            int playerCoins = invStore.Coins;

            foreach (var shopItem in tbShop.DataList)
            {
                var entry = Object.Instantiate(_entryPrefab, _listContainer);
                entry.gameObject.SetActive(true);

                var itemCfg = tbItem?.GetOrDefault(shopItem.ItemId);
                string itemName = itemCfg != null ? GF.Localization.Get(itemCfg.NameKey) : $"#{shopItem.ItemId}";
                bool canAfford = playerCoins >= shopItem.Price;

                int shopId = shopItem.Id;
                entry.Setup(itemName, shopItem.Price, canAfford, () => shopSystem.TryPurchase(shopId));
                _entries.Add(entry);
            }
        }

        private void ClearEntries()
        {
            foreach (var entry in _entries)
            {
                entry.Cleanup();
                Object.Destroy(entry.gameObject);
            }
            _entries.Clear();
        }

        private void OnClose() => CloseWindow();
    }
}
