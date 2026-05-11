using IsleWorks.Economy;
using IsleWorks.Tech;
using JulyArch;
using JulyCore;
using TMPro;
using UnityEngine;

namespace IsleWorks.Views
{
    /// <summary>
    /// HUD 视图 —— 显示金币和时代信息。
    /// </summary>
    public class HudView : GameView
    {
        public override IGameContext GetArchitecture() => AppArch.Context;

        private TextMeshProUGUI _goldText;
        private TextMeshProUGUI _eraText;

        private static readonly string[] EraNames = { "石器时代", "铜器时代", "蒸汽时代", "电气时代" };

        protected override void OnEnable()
        {
            base.OnEnable();
            this.Subscribe<GoldChangedEvent>(OnGoldChanged);
            this.Subscribe<EraChangedEvent>(OnEraChanged);
        }

        protected override void OnDisable()
        {
            this.Unsubscribe<GoldChangedEvent>(OnGoldChanged);
            this.Unsubscribe<EraChangedEvent>(OnEraChanged);
            base.OnDisable();
        }

        public void Initialize()
        {
            CreateUI();
            RefreshGold();
            RefreshEra();
            GF.Log("HudView initialized.");
        }

        private void CreateUI()
        {
            var canvasObj = new GameObject("HudCanvas");
            canvasObj.transform.SetParent(transform);
            var canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();

            _goldText = CreateTextElement(canvasObj.transform, "GoldText", new Vector2(10, -10), new Vector2(0, 1));
            _goldText.text = "Gold: 500";
            _goldText.fontSize = 24;

            _eraText = CreateTextElement(canvasObj.transform, "EraText", new Vector2(-10, -10), new Vector2(1, 1));
            _eraText.text = EraNames[0];
            _eraText.fontSize = 24;
            _eraText.alignment = TextAlignmentOptions.TopRight;
        }

        private TextMeshProUGUI CreateTextElement(Transform parent, string name, Vector2 anchoredPos, Vector2 anchor)
        {
            var obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            var rt = obj.AddComponent<RectTransform>();
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.pivot = anchor;
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = new Vector2(200, 40);

            var text = obj.AddComponent<TextMeshProUGUI>();
            text.fontSize = 20;
            text.color = Color.white;
            text.enableAutoSizing = false;
            text.overflowMode = TextOverflowModes.Ellipsis;

            return text;
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
