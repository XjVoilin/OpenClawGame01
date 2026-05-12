using IsleWorks.Economy;
using IsleWorks.Tech;
using JulyArch;
using TMPro;
using UnityEngine;

namespace IsleWorks.Views
{
    /// <summary>
    /// 游戏 HUD —— 显示金币和时代信息。
    /// Prefab 结构：Root > GoldText(TMP) + EraText(TMP)
    /// </summary>
    public class GameHUD : GameUIView
    {
        [SerializeField] private TextMeshProUGUI _goldText;
        [SerializeField] private TextMeshProUGUI _eraText;

        private static readonly string[] EraNames = { "石器时代", "铜器时代", "蒸汽时代", "电气时代" };

        protected override void OnBeforeOpen()
        {
            base.OnBeforeOpen();

            this.Subscribe<GoldChangedEvent>(OnGoldChanged);
            this.Subscribe<EraChangedEvent>(OnEraChanged);

            RefreshGold();
            RefreshEra();
        }

        private void RefreshGold()
        {
            var inv = this.Query<IInventoryQueries>();
            _goldText.text = $"Gold: {inv.Gold}";
        }

        private void RefreshEra()
        {
            var tech = this.Query<ITechQueries>();
            int era = tech.CurrentEra;
            _eraText.text = era < EraNames.Length ? EraNames[era] : $"Era {era}";
        }

        private void OnGoldChanged(GoldChangedEvent e)
        {
            _goldText.text = $"Gold: {e.NewGold}";
        }

        private void OnEraChanged(EraChangedEvent e)
        {
            int era = e.NewEra;
            _eraText.text = era < EraNames.Length ? EraNames[era] : $"Era {era}";
        }
    }
}
