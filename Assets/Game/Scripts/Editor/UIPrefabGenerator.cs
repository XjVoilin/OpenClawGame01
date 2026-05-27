#if UNITY_EDITOR
using System.IO;
using JulyCore;
using JulyToolkit;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace CozyYard.Editor
{
    /// <summary>
    /// 一键生成 CozyYard UI 预制体（横屏 1920×1080）
    /// Sprout Lands 田园像素风皮肤
    /// </summary>
    public static class UIPrefabGenerator
    {
        private const string PrefabRoot = "Assets/Game/Res/Prefabs/UI";
        private const string ArtPrefabRoot = "Assets/Game/Arts/Prefabs/UI";
        private const string UISpriteRoot = "Assets/Game/Res/Sprites/UI";
        private static readonly Vector2 LandscapeSize = new(1920, 1080);

        private static readonly Color TitleColor = new(0.35f, 0.25f, 0.15f);
        private static readonly Color BodyColor = new(0.50f, 0.38f, 0.22f);
        private static readonly Color BtnTextColor = new(0.95f, 0.90f, 0.80f);
        private static readonly Color GoldColor = new(1f, 0.85f, 0.3f);
        private static readonly Color HintColor = new(0.60f, 0.50f, 0.30f);
        private static readonly Color EntryTint = new(1f, 0.96f, 0.88f, 0.95f);

        private const float PanelPUM = 3f;
        private const float BtnPUM = 2f;

        [MenuItem("CozyYard/生成所有 UI 预制体", false, 200)]
        public static void GenerateAll()
        {
            GenerateGameHUD();
            GenerateTimeHUD();
            GenerateWeatherHUD();
            GenerateInventoryWindow();
            GenerateBuildWindow();
            GenerateCraftWindow();
            GenerateVisitorWindow();
            GenerateMilestoneWindow();
            GenerateRecipeBookWindow();
            GeneratePhoneWindow();
            GenerateShopWindow();
            AssetDatabase.Refresh();
            Debug.Log("[UIPrefabGenerator] CozyYard UI 预制体已生成完毕 (1920×1080 横屏, Sprout Lands 皮肤)");
        }

        [MenuItem("CozyYard/生成所有 UI 预制体", true)]
        private static bool GenerateAllValidate() => !Application.isPlaying;

        // ══════════════════════════════════════════════
        //  GameHUD — 常驻主界面 (全屏 1920×1080)
        // ══════════════════════════════════════════════

        private static void GenerateGameHUD()
        {
            var root = CreatePanelRoot("GameHUD", LandscapeSize);
            StretchToParent(root);

            // ── 右上角：大门状态 ──
            var topRight = AddChild(root, "TopRight");
            SetAnchors(topRight, new Vector2(1, 1), new Vector2(1, 1),
                new Vector2(-320, -90), new Vector2(-20, -20));
            AddVerticalLayout(topRight, 8, TextAnchor.UpperRight);

            var gateText = AddText(topRight, "GateText", "大门: 开", 22, "gate_open");
            SetSize(gateText.gameObject, 280, 36);
            gateText.alignment = TextAlignmentOptions.Right;
            gateText.color = BodyColor;

            var gateToggleBtn = AddButton(topRight, "GateToggleBtn", "切换大门", new Vector2(160, 44), 20, "gate_toggle");

            // ── 来客角标 ──
            var visitorBadgeText = AddText(root, "VisitorBadgeText", "1", 18);
            SetAnchors(visitorBadgeText.gameObject, new Vector2(0.5f, 0), new Vector2(0.5f, 0),
                new Vector2(200, 95), new Vector2(230, 125));
            visitorBadgeText.color = GoldColor;

            // ── 底部导航栏 ──
            var bottomBar = AddChild(root, "BottomBar");
            SetAnchors(bottomBar, new Vector2(0, 0), new Vector2(1, 0),
                new Vector2(40, 12), new Vector2(-40, 92));
            var bottomBg = bottomBar.AddComponent<Image>();
            ApplyPanel(bottomBg);
            bottomBg.color = new Color(1f, 0.98f, 0.92f, 0.85f);
            AddHorizontalLayout(bottomBar, 16, TextAnchor.MiddleCenter);
            var barLayout = bottomBar.GetComponent<HorizontalLayoutGroup>();
            barLayout.padding = new RectOffset(20, 20, 6, 6);

            var inventoryBtn = AddButton(bottomBar, "InventoryBtn", "背包", new Vector2(120, 56), 20, "btn_inventory");
            var buildBtn = AddButton(bottomBar, "BuildBtn", "建造", new Vector2(120, 56), 20, "btn_build");
            var craftBtn = AddButton(bottomBar, "CraftBtn", "制作", new Vector2(120, 56), 20, "btn_craft");
            var visitorBtn = AddButton(bottomBar, "VisitorBtn", "来客", new Vector2(120, 56), 20, "btn_visitor");
            var milestoneBtn = AddButton(bottomBar, "MilestoneBtn", "里程碑", new Vector2(120, 56), 20, "btn_milestone");
            var recipeBookBtn = AddButton(bottomBar, "RecipeBookBtn", "配方本", new Vector2(120, 56), 20, "btn_recipe_book");
            var phoneBtn = AddButton(bottomBar, "PhoneBtn", "问妈", new Vector2(120, 56), 20, "btn_phone");
            var shopBtn = AddButton(bottomBar, "ShopBtn", "商店", new Vector2(120, 56), 20, "btn_shop");

            var hud = root.AddComponent<GameHUD>();
            Bind(hud, "_inventoryBtn", inventoryBtn);
            Bind(hud, "_buildBtn", buildBtn);
            Bind(hud, "_craftBtn", craftBtn);
            Bind(hud, "_visitorBtn", visitorBtn);
            Bind(hud, "_milestoneBtn", milestoneBtn);
            Bind(hud, "_recipeBookBtn", recipeBookBtn);
            Bind(hud, "_phoneBtn", phoneBtn);
            Bind(hud, "_shopBtn", shopBtn);
            Bind(hud, "_gateToggleBtn", gateToggleBtn);
            Bind(hud, "_gateText", gateText);
            Bind(hud, "_visitorBadgeText", visitorBadgeText);

            SavePrefab(root, "GameHUD", "GameHUD");
        }

        // ══════════════════════════════════════════════
        //  TimeHUD — 时间信息显示 (常驻, 左上角)
        // ══════════════════════════════════════════════

        private static void GenerateTimeHUD()
        {
            var root = CreatePanelRoot("TimeHUD", LandscapeSize);
            StretchToParent(root);

            var container = AddChild(root, "Container");
            SetAnchors(container, new Vector2(0, 1), new Vector2(0, 1),
                new Vector2(20, -200), new Vector2(380, -20));
            var containerBg = container.AddComponent<Image>();
            ApplyPanel(containerBg);
            AddVerticalLayout(container, 6, TextAnchor.UpperLeft);
            var containerLayout = container.GetComponent<VerticalLayoutGroup>();
            containerLayout.padding = new RectOffset(12, 12, 10, 10);

            var dayText = AddText(container, "DayText", "第 1 天", 30, "day_format");
            SetSize(dayText.gameObject, 340, 42);
            dayText.alignment = TextAlignmentOptions.Left;
            dayText.color = TitleColor;

            var seasonText = AddText(container, "SeasonText", "春", 26, "season_spring");
            SetSize(seasonText.gameObject, 340, 36);
            seasonText.alignment = TextAlignmentOptions.Left;
            seasonText.color = BodyColor;

            var timeText = AddText(container, "TimeText", "06:00", 34);
            SetSize(timeText.gameObject, 340, 46);
            timeText.alignment = TextAlignmentOptions.Left;
            timeText.color = TitleColor;

            var phaseText = AddText(container, "PhaseText", "清晨", 24, "phase_dawn");
            SetSize(phaseText.gameObject, 340, 34);
            phaseText.alignment = TextAlignmentOptions.Left;
            phaseText.color = HintColor;

            // ── 速度控制按钮 ──
            var speedBar = AddChild(root, "SpeedBar");
            SetAnchors(speedBar, new Vector2(0, 1), new Vector2(0, 1),
                new Vector2(20, -260), new Vector2(380, -205));
            var speedBarBg = speedBar.AddComponent<Image>();
            ApplyPanel(speedBarBg);
            speedBarBg.color = EntryTint;
            AddHorizontalLayout(speedBar, 10, TextAnchor.MiddleLeft);
            var speedBarLayout = speedBar.GetComponent<HorizontalLayoutGroup>();
            speedBarLayout.padding = new RectOffset(10, 10, 4, 4);

            var speed1Btn = AddNativeButton(speedBar, "Speed1Btn", "×1", new Vector2(72, 48), 22);
            var speed2Btn = AddNativeButton(speedBar, "Speed2Btn", "×2", new Vector2(72, 48), 22);
            var speed3Btn = AddNativeButton(speedBar, "Speed3Btn", "×3", new Vector2(72, 48), 22);
            var endDayBtn = AddNativeButton(speedBar, "EndDayBtn", "结束", new Vector2(96, 48), 22);

            var hud = root.AddComponent<TimeHUD>();
            Bind(hud, "_dayText", dayText);
            Bind(hud, "_seasonText", seasonText);
            Bind(hud, "_timeText", timeText);
            Bind(hud, "_phaseText", phaseText);
            Bind(hud, "_speed1Btn", speed1Btn);
            Bind(hud, "_speed2Btn", speed2Btn);
            Bind(hud, "_speed3Btn", speed3Btn);
            Bind(hud, "_endDayBtn", endDayBtn);

            SavePrefab(root, "TimeHUD", "TimeHUD");
        }

        // ══════════════════════════════════════════════
        //  WeatherHUD — 天气信息显示 (常驻, 左上偏右)
        // ══════════════════════════════════════════════

        private static void GenerateWeatherHUD()
        {
            var root = CreatePanelRoot("WeatherHUD", LandscapeSize);
            StretchToParent(root);

            var container = AddChild(root, "Container");
            SetAnchors(container, new Vector2(0, 1), new Vector2(0, 1),
                new Vector2(20, -320), new Vector2(320, -265));
            var weatherBg = container.AddComponent<Image>();
            ApplyPanel(weatherBg);
            AddHorizontalLayout(container, 10, TextAnchor.MiddleLeft);
            var weatherLayout = container.GetComponent<HorizontalLayoutGroup>();
            weatherLayout.padding = new RectOffset(10, 10, 4, 4);

            var weatherIcon = AddText(container, "WeatherIcon", "☀", 36);
            SetSize(weatherIcon.gameObject, 52, 52);
            weatherIcon.alignment = TextAlignmentOptions.Center;
            weatherIcon.color = TitleColor;

            var weatherText = AddText(container, "WeatherText", "晴天", 28, "weather_sunny");
            SetSize(weatherText.gameObject, 220, 48);
            weatherText.alignment = TextAlignmentOptions.Left;
            weatherText.color = BodyColor;

            var hud = root.AddComponent<WeatherHUD>();
            Bind(hud, "_weatherText", weatherText);
            Bind(hud, "_weatherIcon", weatherIcon);

            SavePrefab(root, "WeatherHUD", "WeatherHUD");
        }

        // ══════════════════════════════════════════════
        //  InventoryWindow — 背包
        // ══════════════════════════════════════════════

        private static void GenerateInventoryWindow()
        {
            var root = CreatePanelRoot("InventoryWindow", new Vector2(840, 900));
            AddBg(root);

            var title = AddText(root, "Title", "背  包", 36, "title_inventory");
            SetAnchors(title.gameObject, new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(0, -56), new Vector2(0, -12));
            title.color = TitleColor;

            // ── 状态栏 (容量 + 金币) ──
            var statusBar = AddChild(root, "StatusBar");
            SetAnchors(statusBar, new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(24, -100), new Vector2(-24, -60));
            AddHorizontalLayout(statusBar, 24, TextAnchor.MiddleLeft);

            var capacityText = AddText(statusBar, "CapacityText", "0/30", 24);
            SetSize(capacityText.gameObject, 160, 36);
            capacityText.alignment = TextAlignmentOptions.Left;
            capacityText.color = BodyColor;

            var coinIcon = AddCoinIcon(statusBar, 32);
            var coinsText = AddText(statusBar, "CoinsText", "0", 24);
            SetSize(coinsText.gameObject, 120, 36);
            coinsText.alignment = TextAlignmentOptions.Left;
            coinsText.color = GoldColor;

            // ── 分类标签页 ──
            var tabBar = AddChild(root, "CategoryTabs");
            SetAnchors(tabBar, new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(24, -148), new Vector2(-24, -104));
            AddHorizontalLayout(tabBar, 4, TextAnchor.MiddleLeft);

            var toggleGroup = tabBar.AddComponent<UIToggleGroup>();
            var tabNames = new[] { "全部", "材料", "种子", "产品" };
            var tabLocKeys = new[] { "tab_all", "tab_material", "tab_seed", "tab_product" };
            var toggleItems = new UIToggleItem[tabNames.Length];

            for (int i = 0; i < tabNames.Length; i++)
                toggleItems[i] = CreateToggleTab(tabBar, tabNames[i], tabLocKeys[i], i);

            var toggleGroupSo = new SerializedObject(toggleGroup);
            var itemsProp = toggleGroupSo.FindProperty("m_Items");
            itemsProp.arraySize = tabNames.Length;
            for (int i = 0; i < tabNames.Length; i++)
                itemsProp.GetArrayElementAtIndex(i).objectReferenceValue = toggleItems[i];
            toggleGroupSo.FindProperty("m_SelectedIndex").intValue = 0;
            toggleGroupSo.ApplyModifiedPropertiesWithoutUndo();

            // ── 格子区域 ──
            var scrollArea = AddChild(root, "ScrollArea");
            SetAnchors(scrollArea, new Vector2(0, 0), new Vector2(1, 1),
                new Vector2(24, 220), new Vector2(-24, -156));
            var itemsContainerGo = CreateScrollContent(scrollArea, "ItemsContainer", out _).gameObject;
            Object.DestroyImmediate(itemsContainerGo.GetComponent<VerticalLayoutGroup>());
            var grid = itemsContainerGo.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(120, 120);
            grid.spacing = new Vector2(8, 8);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 5;
            grid.childAlignment = TextAnchor.UpperLeft;
            grid.padding = new RectOffset(12, 12, 8, 8);

            var itemSlotPrefab = CreateItemSlotPrefab();

            // ── 底部详情面板 ──
            var detailPanel = AddChild(root, "DetailPanel");
            SetAnchors(detailPanel, new Vector2(0, 0), new Vector2(1, 0),
                new Vector2(24, 64), new Vector2(-24, 212));
            var detailBg = detailPanel.AddComponent<Image>();
            ApplyPanel(detailBg);
            detailBg.color = EntryTint;

            var detailIcon = AddChild(detailPanel, "DetailIcon");
            SetAnchors(detailIcon, new Vector2(0, 0.5f), new Vector2(0, 0.5f),
                new Vector2(16, -36), new Vector2(88, 36));
            var detailIconImg = detailIcon.AddComponent<Image>();
            detailIconImg.color = new Color(0.8f, 0.7f, 0.5f, 0.4f);

            var detailName = AddText(detailPanel, "DetailName", "物品名称", 26);
            SetAnchors(detailName.gameObject, new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(100, -44), new Vector2(-16, -8));
            detailName.alignment = TextAlignmentOptions.Left;
            detailName.color = TitleColor;

            var detailDesc = AddText(detailPanel, "DetailDesc", "物品描述...", 20);
            SetAnchors(detailDesc.gameObject, new Vector2(0, 0), new Vector2(1, 1),
                new Vector2(100, 8), new Vector2(-140, -48));
            detailDesc.alignment = TextAlignmentOptions.TopLeft;
            detailDesc.color = HintColor;

            var useBtn = AddButton(detailPanel, "UseBtn", "使用", new Vector2(100, 40), 20, "btn_use");
            SetAnchors(useBtn.gameObject, new Vector2(1, 0), new Vector2(1, 0),
                new Vector2(-228, 12), new Vector2(-132, 52));

            var discardBtn = AddButton(detailPanel, "DiscardBtn", "丢弃", new Vector2(100, 40), 20, "btn_discard");
            SetAnchors(discardBtn.gameObject, new Vector2(1, 0), new Vector2(1, 0),
                new Vector2(-120, 12), new Vector2(-24, 52));

            // ── 关闭按钮 ──
            var closeBtn = AddCloseButton(root);

            var panel = root.AddComponent<InventoryWindow>();
            Bind(panel, "_itemsContainer", itemsContainerGo.transform);
            Bind(panel, "_itemSlotPrefab", itemSlotPrefab);
            Bind(panel, "_capacityText", capacityText);
            Bind(panel, "_coinsText", coinsText);
            Bind(panel, "_closeBtn", closeBtn);
            Bind(panel, "_categoryTabs", toggleGroup);
            Bind(panel, "_detailPanel", (Object)detailPanel);
            Bind(panel, "_detailIcon", detailIconImg);
            Bind(panel, "_detailName", detailName);
            Bind(panel, "_detailDesc", detailDesc);
            Bind(panel, "_useBtn", useBtn);
            Bind(panel, "_discardBtn", discardBtn);

            SavePrefab(root, "InventoryWindow", "InventoryWindow");
        }

        private static UIToggleItem CreateToggleTab(GameObject parent, string label, string locKey, int index)
        {
            var go = new GameObject($"Tab_{index}", typeof(RectTransform));
            go.layer = parent.layer;
            go.transform.SetParent(parent.transform, false);
            SetSize(go, 120, 40);

            var emptyGraphic = go.AddComponent<UIEmptyGraphic>();
            emptyGraphic.raycastTarget = true;

            var normal = AddFullStretchChild(go, "Normal");
            var normalImg = normal.AddComponent<Image>();
            ApplyBtn(normalImg, "SL_UI_Btn_Light");
            normalImg.raycastTarget = false;
            var normalText = AddText(normal, "Text", label, 22, locKey);
            normalText.color = HintColor;
            SetAnchors(normalText.gameObject, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            var selected = AddFullStretchChild(go, "Selected");
            var selectedImg = selected.AddComponent<Image>();
            ApplyBtn(selectedImg, "SL_UI_Btn_Medium");
            selectedImg.raycastTarget = false;
            var selectedText = AddText(selected, "Text", label, 22, locKey);
            selectedText.color = BtnTextColor;
            SetAnchors(selectedText.gameObject, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            selected.SetActive(index == 0);

            var item = go.AddComponent<UIToggleItem>();
            item.targetGraphic = emptyGraphic;

            var itemSo = new SerializedObject(item);
            itemSo.FindProperty("m_Normal").objectReferenceValue = normal;
            itemSo.FindProperty("m_Selected").objectReferenceValue = selected;
            itemSo.ApplyModifiedPropertiesWithoutUndo();

            return item;
        }

        // ══════════════════════════════════════════════
        //  BuildWindow — 建造面板
        // ══════════════════════════════════════════════

        private static void GenerateBuildWindow()
        {
            var root = CreatePanelRoot("BuildWindow", new Vector2(800, 880));
            AddBg(root);

            var title = AddText(root, "Title", "建  造", 36, "title_build");
            SetAnchors(title.gameObject, new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(0, -72), new Vector2(0, -12));
            title.color = TitleColor;

            var scrollArea = AddChild(root, "ScrollArea");
            SetAnchors(scrollArea, new Vector2(0, 0), new Vector2(1, 1),
                new Vector2(24, 90), new Vector2(-24, -80));
            var listContainer = CreateScrollContent(scrollArea, "ListContainer", out _);

            var entryPrefab = CreateBuildEntryPrefab();

            var closeBtn = AddCloseButton(root);

            var panel = root.AddComponent<BuildWindow>();
            Bind(panel, "_listContainer", listContainer);
            Bind(panel, "_entryPrefab", entryPrefab);
            Bind(panel, "_closeBtn", closeBtn);

            SavePrefab(root, "BuildWindow", "BuildWindow");
        }

        // ══════════════════════════════════════════════
        //  CraftWindow — 制作界面
        // ══════════════════════════════════════════════

        private static void GenerateCraftWindow()
        {
            var root = CreatePanelRoot("CraftWindow", new Vector2(880, 900));
            AddBg(root);

            var title = AddText(root, "Title", "制  作", 36, "title_craft");
            SetAnchors(title.gameObject, new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(0, -72), new Vector2(0, -12));
            title.color = TitleColor;

            var scrollArea = AddChild(root, "ScrollArea");
            SetAnchors(scrollArea, new Vector2(0, 0), new Vector2(1, 1),
                new Vector2(24, 90), new Vector2(-24, -80));
            var listContainer = CreateScrollContent(scrollArea, "ListContainer", out _);

            var entryPrefab = CreateCraftEntryPrefab();

            var closeBtn = AddCloseButton(root);

            var panel = root.AddComponent<CraftWindow>();
            Bind(panel, "_listContainer", listContainer);
            Bind(panel, "_entryPrefab", entryPrefab);
            Bind(panel, "_closeBtn", closeBtn);

            SavePrefab(root, "CraftWindow", "CraftWindow");
        }

        // ══════════════════════════════════════════════
        //  VisitorWindow — 来客对话
        // ══════════════════════════════════════════════

        private static void GenerateVisitorWindow()
        {
            var root = CreatePanelRoot("VisitorWindow", new Vector2(960, 860));
            AddBg(root);

            var title = AddText(root, "Title", "来  客", 36, "title_visitor");
            SetAnchors(title.gameObject, new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(0, -72), new Vector2(0, -12));
            title.color = TitleColor;

            var gateRow = AddChild(root, "GateRow");
            SetAnchors(gateRow, new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(32, -135), new Vector2(-32, -80));
            AddHorizontalLayout(gateRow, 24, TextAnchor.MiddleLeft);

            var gateText = AddText(gateRow, "GateText", "大门: 开", 28, "gate_open");
            SetSize(gateText.gameObject, 280, 52);
            gateText.alignment = TextAlignmentOptions.Left;
            gateText.color = BodyColor;

            var gateToggleBtn = AddButton(gateRow, "GateToggleBtn", "切换大门", new Vector2(200, 60), 26, "gate_toggle");

            var scrollArea = AddChild(root, "ScrollArea");
            SetAnchors(scrollArea, new Vector2(0, 0), new Vector2(1, 1),
                new Vector2(24, 90), new Vector2(-24, -145));
            var listContainer = CreateScrollContent(scrollArea, "ListContainer", out _);

            var entryPrefab = CreateVisitorEntryPrefab();

            var closeBtn = AddCloseButton(root);

            var panel = root.AddComponent<VisitorWindow>();
            Bind(panel, "_listContainer", listContainer);
            Bind(panel, "_entryPrefab", entryPrefab);
            Bind(panel, "_gateToggleBtn", gateToggleBtn);
            Bind(panel, "_gateText", gateText);
            Bind(panel, "_closeBtn", closeBtn);

            SavePrefab(root, "VisitorWindow", "VisitorWindow");
        }

        // ══════════════════════════════════════════════
        //  MilestoneWindow — 里程碑
        // ══════════════════════════════════════════════

        private static void GenerateMilestoneWindow()
        {
            var root = CreatePanelRoot("MilestoneWindow", new Vector2(880, 900));
            AddBg(root);

            var title = AddText(root, "Title", "里程碑", 36, "title_milestone");
            SetAnchors(title.gameObject, new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(0, -72), new Vector2(0, -12));
            title.color = TitleColor;

            var expansionText = AddText(root, "ExpansionText", "扩建等级: 0", 28, "expansion_level");
            SetAnchors(expansionText.gameObject, new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(32, -135), new Vector2(-32, -82));
            expansionText.alignment = TextAlignmentOptions.Left;
            expansionText.color = BodyColor;

            var scrollArea = AddChild(root, "ScrollArea");
            SetAnchors(scrollArea, new Vector2(0, 0), new Vector2(1, 1),
                new Vector2(24, 90), new Vector2(-24, -145));
            var listContainer = CreateScrollContent(scrollArea, "ListContainer", out _);

            var entryPrefab = CreateMilestoneEntryPrefab();

            var closeBtn = AddCloseButton(root);

            var panel = root.AddComponent<MilestoneWindow>();
            Bind(panel, "_listContainer", listContainer);
            Bind(panel, "_entryPrefab", entryPrefab);
            Bind(panel, "_expansionText", expansionText);
            Bind(panel, "_closeBtn", closeBtn);

            SavePrefab(root, "MilestoneWindow", "MilestoneWindow");
        }

        // ══════════════════════════════════════════════
        //  RecipeBookWindow — 配方本
        // ══════════════════════════════════════════════

        private static void GenerateRecipeBookWindow()
        {
            var root = CreatePanelRoot("RecipeBookWindow", new Vector2(880, 900));
            AddBg(root);

            var title = AddText(root, "Title", "配方本", 36, "title_recipe_book");
            SetAnchors(title.gameObject, new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(0, -72), new Vector2(0, -12));
            title.color = TitleColor;

            var scrollArea = AddChild(root, "ScrollArea");
            SetAnchors(scrollArea, new Vector2(0, 0), new Vector2(1, 1),
                new Vector2(24, 90), new Vector2(-24, -80));
            var listContainer = CreateScrollContent(scrollArea, "ListContainer", out _);

            var entryPrefab = CreateRecipeBookEntryPrefab();

            var closeBtn = AddCloseButton(root);

            var panel = root.AddComponent<RecipeBookWindow>();
            Bind(panel, "_listContainer", listContainer);
            Bind(panel, "_entryPrefab", entryPrefab);
            Bind(panel, "_closeBtn", closeBtn);

            SavePrefab(root, "RecipeBookWindow", "RecipeBookWindow");
        }

        // ══════════════════════════════════════════════
        //  PhoneWindow — 问妈
        // ══════════════════════════════════════════════

        private static void GeneratePhoneWindow()
        {
            var root = CreatePanelRoot("PhoneWindow", new Vector2(880, 900));
            AddBg(root);

            var title = AddText(root, "Title", "问  妈", 36, "title_phone");
            SetAnchors(title.gameObject, new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(0, -72), new Vector2(0, -12));
            title.color = TitleColor;

            var hintText = AddText(root, "HintText", "告诉妈妈你有什么材料，她可能知道配方", 26, "mom_hint");
            SetAnchors(hintText.gameObject, new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(32, -135), new Vector2(-32, -82));
            hintText.alignment = TextAlignmentOptions.Left;
            hintText.color = HintColor;

            var asksRemainingText = AddText(root, "AsksRemainingText", "今日剩余询问: 1/1", 26);
            SetAnchors(asksRemainingText.gameObject, new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(32, -185), new Vector2(-32, -140));
            asksRemainingText.alignment = TextAlignmentOptions.Left;
            asksRemainingText.color = BodyColor;

            var scrollArea = AddChild(root, "ItemScrollArea");
            SetAnchors(scrollArea, new Vector2(0, 0), new Vector2(1, 1),
                new Vector2(24, 170), new Vector2(-24, -195));
            var itemsContainer = CreateScrollContent(scrollArea, "ItemsContainer", out _);

            var itemEntryPrefab = CreatePhoneItemEntryPrefab();

            var resultText = AddText(root, "ResultText", "", 26);
            SetAnchors(resultText.gameObject, new Vector2(0, 0), new Vector2(1, 0),
                new Vector2(32, 95), new Vector2(-32, 155));
            resultText.alignment = TextAlignmentOptions.Left;
            resultText.color = new Color(0.65f, 0.45f, 0.15f);

            var btnArea = AddChild(root, "ButtonArea");
            SetAnchors(btnArea, new Vector2(0, 0), new Vector2(1, 0),
                new Vector2(32, 16), new Vector2(-32, 84));
            AddHorizontalLayout(btnArea, 30, TextAnchor.MiddleCenter);

            var askBtn = AddGrayButton(btnArea, "AskBtn", "询问妈妈", new Vector2(220, 64), 26, "ask_mom");
            var closeBtn = AddCloseButton(btnArea, false);

            var panel = root.AddComponent<PhoneWindow>();
            Bind(panel, "_hintText", hintText);
            Bind(panel, "_asksRemainingText", asksRemainingText);
            Bind(panel, "_resultText", resultText);
            Bind(panel, "_itemsContainer", itemsContainer);
            Bind(panel, "_itemEntryPrefab", itemEntryPrefab);
            Bind(panel, "_askBtn", askBtn);
            Bind(panel, "_closeBtn", closeBtn);

            SavePrefab(root, "PhoneWindow", "PhoneWindow");
        }

        // ══════════════════════════════════════════════
        //  ShopWindow — 商店
        // ══════════════════════════════════════════════

        private static void GenerateShopWindow()
        {
            var root = CreatePanelRoot("ShopWindow", new Vector2(880, 780));
            AddBg(root);

            var title = AddText(root, "Title", "商  店", 36, "title_shop");
            SetAnchors(title.gameObject, new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(0, -72), new Vector2(0, -12));
            title.color = TitleColor;

            var coinsRow = AddChild(root, "CoinsRow");
            SetAnchors(coinsRow, new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(32, -125), new Vector2(-32, -82));
            AddHorizontalLayout(coinsRow, 8, TextAnchor.MiddleLeft);

            var coinIcon = AddCoinIcon(coinsRow, 36);
            var coinsText = AddText(coinsRow, "CoinsText", "0", 28);
            coinsText.alignment = TextAlignmentOptions.Left;
            coinsText.color = GoldColor;
            SetSize(coinsText.gameObject, 200, 40);

            var scrollArea = AddChild(root, "ScrollArea");
            SetAnchors(scrollArea, new Vector2(0, 0), new Vector2(1, 1),
                new Vector2(24, 90), new Vector2(-24, -135));
            var listContainer = CreateScrollContent(scrollArea, "ListContainer", out _);

            var entryPrefab = CreateShopEntryPrefab();

            var closeBtn = AddCloseButton(root);

            var panel = root.AddComponent<ShopWindow>();
            Bind(panel, "_listContainer", listContainer);
            Bind(panel, "_entryPrefab", entryPrefab);
            Bind(panel, "_coinsText", coinsText);
            Bind(panel, "_closeBtn", closeBtn);

            SavePrefab(root, "ShopWindow", "ShopWindow");
        }

        // ══════════════════════════════════════════════
        //  Entry Prefabs
        // ══════════════════════════════════════════════

        private static InventorySlotEntry CreateItemSlotPrefab()
        {
            var go = new GameObject("InventorySlotEntry", typeof(RectTransform), typeof(Image));
            go.layer = LayerMask.NameToLayer("UI");
            SetSize(go, 120, 120);

            var bg = go.GetComponent<Image>();
            ApplyBtn(bg, "SL_UI_Btn_Lightest");

            var filledRoot = AddChild(go, "FilledRoot");
            SetAnchors(filledRoot, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            var iconGo = AddChild(filledRoot, "Icon");
            SetAnchors(iconGo, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(-38, -38), new Vector2(38, 38));
            var iconImg = iconGo.AddComponent<Image>();
            iconImg.color = new Color(0.8f, 0.7f, 0.5f, 0.4f);
            iconImg.preserveAspect = true;

            var qtyGo = AddChild(filledRoot, "Quantity");
            SetAnchors(qtyGo, new Vector2(1, 0), new Vector2(1, 0),
                new Vector2(-44, 2), new Vector2(-2, 26));
            var qtyText = qtyGo.AddComponent<TextMeshProUGUI>();
            qtyText.text = "99";
            qtyText.fontSize = 18;
            qtyText.color = TitleColor;
            qtyText.alignment = TextAlignmentOptions.BottomRight;

            var emptyRoot = AddChild(go, "EmptyRoot");
            SetAnchors(emptyRoot, Vector2.zero, Vector2.one,
                new Vector2(4, 4), new Vector2(-4, -4));
            var emptyImg = emptyRoot.AddComponent<Image>();
            ApplyBtn(emptyImg, "SL_UI_Btn_Lightest_Dark");
            emptyImg.color = new Color(0.9f, 0.85f, 0.75f, 0.4f);
            emptyRoot.SetActive(false);

            var selectedFrame = AddChild(go, "SelectedFrame");
            SetAnchors(selectedFrame, Vector2.zero, Vector2.one,
                new Vector2(-2, -2), new Vector2(2, 2));
            var frameImg = selectedFrame.AddComponent<Image>();
            ApplyBtn(frameImg, "SL_UI_Btn_Medium");
            frameImg.color = GoldColor;
            frameImg.raycastTarget = false;
            var frameMask = AddFullStretchChild(selectedFrame, "Inner", new RectOffset(4, 4, 4, 4));
            var frameMaskImg = frameMask.AddComponent<Image>();
            ApplyBtn(frameMaskImg, "SL_UI_Btn_Lightest");
            frameMaskImg.raycastTarget = false;
            selectedFrame.SetActive(false);

            var slot = go.AddComponent<UIItemSlot>();
            var slotSo = new SerializedObject(slot);
            slotSo.FindProperty("_icon").objectReferenceValue = iconImg;
            slotSo.FindProperty("_quantityText").objectReferenceValue = qtyText;
            slotSo.FindProperty("_selectedFrame").objectReferenceValue = selectedFrame;
            slotSo.FindProperty("_emptyRoot").objectReferenceValue = emptyRoot;
            slotSo.FindProperty("_filledRoot").objectReferenceValue = filledRoot;
            slotSo.ApplyModifiedPropertiesWithoutUndo();

            var entry = go.AddComponent<InventorySlotEntry>();
            Bind(entry, "_slot", slot);

            go.SetActive(false);
            return SaveArtPrefab(go, "InventorySlotEntry").GetComponent<InventorySlotEntry>();
        }

        private static BuildEntry CreateBuildEntryPrefab()
        {
            var go = new GameObject("BuildEntry", typeof(RectTransform), typeof(Image));
            go.layer = LayerMask.NameToLayer("UI");
            SetSize(go, 0, 80);

            var bg = go.GetComponent<Image>();
            ApplyPanel(bg);
            bg.color = EntryTint;

            AddHorizontalLayout(go, 16, TextAnchor.MiddleLeft);
            var hlg = go.GetComponent<HorizontalLayoutGroup>();
            hlg.padding = new RectOffset(20, 20, 8, 8);

            var iconImg = CreateEntryIcon(go, 56);

            var infoText = AddText(go, "InfoText", "", 26);
            infoText.alignment = TextAlignmentOptions.Left;
            infoText.color = BodyColor;
            SetSize(infoText.gameObject, 420, 64);

            var buildBtn = AddGrayButton(go, "BuildBtn", "建造", new Vector2(140, 56), 26, "btn_build_action");

            var mono = go.AddComponent<BuildEntry>();
            Bind(mono, "_icon", iconImg);
            Bind(mono, "_nameText", infoText);
            Bind(mono, "_buildBtn", buildBtn);

            go.SetActive(false);
            return SaveArtPrefab(go, "BuildEntry").GetComponent<BuildEntry>();
        }

        private static CraftEntry CreateCraftEntryPrefab()
        {
            var go = new GameObject("CraftEntry", typeof(RectTransform), typeof(Image));
            go.layer = LayerMask.NameToLayer("UI");
            SetSize(go, 0, 110);

            var bg = go.GetComponent<Image>();
            ApplyPanel(bg);
            bg.color = EntryTint;

            AddHorizontalLayout(go, 16, TextAnchor.MiddleLeft);
            var hlg = go.GetComponent<HorizontalLayoutGroup>();
            hlg.padding = new RectOffset(20, 20, 8, 8);

            var iconImg = CreateEntryIcon(go, 72);

            var infoText = AddText(go, "InfoText", "", 24);
            infoText.alignment = TextAlignmentOptions.Left;
            infoText.color = BodyColor;
            SetSize(infoText.gameObject, 460, 94);

            var craftBtn = AddGrayButton(go, "CraftBtn", "制作", new Vector2(140, 56), 26, "btn_craft_action");

            var mono = go.AddComponent<CraftEntry>();
            Bind(mono, "_icon", iconImg);
            Bind(mono, "_nameText", infoText);
            Bind(mono, "_craftBtn", craftBtn);

            go.SetActive(false);
            return SaveArtPrefab(go, "CraftEntry").GetComponent<CraftEntry>();
        }

        private static ShopEntry CreateShopEntryPrefab()
        {
            var go = new GameObject("ShopEntry", typeof(RectTransform), typeof(Image));
            go.layer = LayerMask.NameToLayer("UI");
            SetSize(go, 0, 90);

            var bg = go.GetComponent<Image>();
            ApplyPanel(bg);
            bg.color = EntryTint;

            AddHorizontalLayout(go, 14, TextAnchor.MiddleLeft);
            var hlg = go.GetComponent<HorizontalLayoutGroup>();
            hlg.padding = new RectOffset(20, 20, 8, 8);

            var iconImg = CreateEntryIcon(go, 64);

            var nameText = AddText(go, "NameText", "", 26);
            nameText.alignment = TextAlignmentOptions.Left;
            nameText.color = BodyColor;
            SetSize(nameText.gameObject, 310, 74);

            var priceText = AddText(go, "PriceText", "0", 24);
            priceText.alignment = TextAlignmentOptions.Center;
            priceText.color = GoldColor;
            SetSize(priceText.gameObject, 120, 74);

            var buyBtn = AddGrayButton(go, "BuyBtn", "购买", new Vector2(120, 56), 24, "btn_buy");

            var mono = go.AddComponent<ShopEntry>();
            Bind(mono, "_icon", iconImg);
            Bind(mono, "_nameText", nameText);
            Bind(mono, "_priceText", priceText);
            Bind(mono, "_buyBtn", buyBtn);

            go.SetActive(false);
            return SaveArtPrefab(go, "ShopEntry").GetComponent<ShopEntry>();
        }

        private static VisitorEntry CreateVisitorEntryPrefab()
        {
            var go = new GameObject("VisitorEntry", typeof(RectTransform), typeof(Image));
            go.layer = LayerMask.NameToLayer("UI");
            SetSize(go, 0, 120);

            var bg = go.GetComponent<Image>();
            ApplyPanel(bg);
            bg.color = EntryTint;

            AddHorizontalLayout(go, 14, TextAnchor.MiddleLeft);
            var hlg = go.GetComponent<HorizontalLayoutGroup>();
            hlg.padding = new RectOffset(20, 20, 8, 8);

            var infoText = AddText(go, "InfoText", "", 24);
            infoText.alignment = TextAlignmentOptions.Left;
            infoText.color = BodyColor;
            SetSize(infoText.gameObject, 480, 104);

            var fulfillBtn = AddButton(go, "FulfillBtn", "交付", new Vector2(120, 56), 24, "deliver");
            var dismissBtn = AddButton(go, "DismissBtn", "送走", new Vector2(120, 56), 24, "dismiss");

            var mono = go.AddComponent<VisitorEntry>();
            Bind(mono, "_infoText", infoText);
            Bind(mono, "_fulfillBtn", fulfillBtn);
            Bind(mono, "_dismissBtn", dismissBtn);

            go.SetActive(false);
            return SaveArtPrefab(go, "VisitorEntry").GetComponent<VisitorEntry>();
        }

        private static MilestoneEntry CreateMilestoneEntryPrefab()
        {
            var go = new GameObject("MilestoneEntry", typeof(RectTransform), typeof(Image));
            go.layer = LayerMask.NameToLayer("UI");
            SetSize(go, 0, 100);

            var bg = go.GetComponent<Image>();
            ApplyPanel(bg);
            bg.color = EntryTint;

            var textGo = AddFullStretchChild(go, "Text", new RectOffset(20, 20, 10, 10));
            var text = textGo.AddComponent<TextMeshProUGUI>();
            text.text = "";
            text.fontSize = 24;
            text.color = BodyColor;
            text.alignment = TextAlignmentOptions.Left;

            var mono = go.AddComponent<MilestoneEntry>();
            Bind(mono, "_infoText", text);

            go.SetActive(false);
            return SaveArtPrefab(go, "MilestoneEntry").GetComponent<MilestoneEntry>();
        }

        private static RecipeBookEntry CreateRecipeBookEntryPrefab()
        {
            var go = new GameObject("RecipeBookEntry", typeof(RectTransform), typeof(Image));
            go.layer = LayerMask.NameToLayer("UI");
            SetSize(go, 0, 100);

            var bg = go.GetComponent<Image>();
            ApplyPanel(bg);
            bg.color = EntryTint;

            var textGo = AddFullStretchChild(go, "Text", new RectOffset(20, 20, 10, 10));
            var text = textGo.AddComponent<TextMeshProUGUI>();
            text.text = "";
            text.fontSize = 24;
            text.color = BodyColor;
            text.alignment = TextAlignmentOptions.Left;

            var mono = go.AddComponent<RecipeBookEntry>();
            Bind(mono, "_nameText", text);

            go.SetActive(false);
            return SaveArtPrefab(go, "RecipeBookEntry").GetComponent<RecipeBookEntry>();
        }

        private static PhoneItemEntry CreatePhoneItemEntryPrefab()
        {
            var go = new GameObject("PhoneItemEntry", typeof(RectTransform), typeof(Image));
            go.layer = LayerMask.NameToLayer("UI");
            SetSize(go, 0, 64);

            var bg = go.GetComponent<Image>();
            ApplyPanel(bg);
            bg.color = EntryTint;

            AddHorizontalLayout(go, 12, TextAnchor.MiddleLeft);
            var hlg = go.GetComponent<HorizontalLayoutGroup>();
            hlg.padding = new RectOffset(16, 16, 6, 6);

            var infoText = AddText(go, "InfoText", "", 24);
            infoText.alignment = TextAlignmentOptions.Left;
            infoText.color = BodyColor;
            SetSize(infoText.gameObject, 520, 52);

            var selectBtn = go.AddComponent<UISmartButton>();

            var mono = go.AddComponent<PhoneItemEntry>();
            Bind(mono, "_nameText", infoText);
            Bind(mono, "_selectBtn", selectBtn);

            go.SetActive(false);
            return SaveArtPrefab(go, "PhoneItemEntry").GetComponent<PhoneItemEntry>();
        }

        // ══════════════════════════════════════════════
        //  构建辅助
        // ══════════════════════════════════════════════

        private static Transform CreateScrollContent(GameObject scrollArea, string contentName, out ScrollRect scrollRect)
        {
            scrollRect = scrollArea.AddComponent<ScrollRect>();
            var viewport = AddFullStretchChild(scrollArea, "Viewport");
            viewport.AddComponent<RectMask2D>();

            var content = AddChild(viewport, contentName);
            SetAnchors(content, new Vector2(0, 1), new Vector2(1, 1), Vector2.zero, Vector2.zero);
            AddVerticalLayout(content, 8, TextAnchor.UpperLeft);
            var csf = content.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scrollRect.viewport = viewport.GetComponent<RectTransform>();
            scrollRect.content = content.GetComponent<RectTransform>();
            scrollRect.horizontal = false;

            return content.transform;
        }

        private static GameObject CreatePanelRoot(string name, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasGroup));
            go.layer = LayerMask.NameToLayer("UI");
            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = size;
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            return go;
        }

        private static void StretchToParent(GameObject go)
        {
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        private static void AddBg(GameObject parent)
        {
            var bg = AddFullStretchChild(parent, "Bg");
            var img = bg.AddComponent<Image>();
            ApplyPanel(img);
        }

        private static Sprite LoadUISprite(string name)
        {
            var path = $"{UISpriteRoot}/{name}.png";
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private static void ApplyPanel(Image img)
        {
            var sprite = LoadUISprite("SL_UI_Panel");
            if (sprite != null)
            {
                img.sprite = sprite;
                img.type = Image.Type.Sliced;
                img.pixelsPerUnitMultiplier = PanelPUM;
            }
            img.color = Color.white;
        }

        private static void ApplyBtn(Image img, string spriteName = "SL_UI_Btn_Medium")
        {
            var sprite = LoadUISprite(spriteName);
            if (sprite != null)
            {
                img.sprite = sprite;
                img.type = Image.Type.Sliced;
                img.pixelsPerUnitMultiplier = BtnPUM;
            }
            img.color = Color.white;
        }

        private static Image AddCoinIcon(GameObject parent, float size)
        {
            var coinGo = AddChild(parent, "CoinIcon");
            SetSize(coinGo, size, size);
            var img = coinGo.AddComponent<Image>();
            var sprite = LoadUISprite("SL_UI_Coin");
            if (sprite != null)
            {
                img.sprite = sprite;
                img.preserveAspect = true;
            }
            img.color = Color.white;
            img.raycastTarget = false;
            return img;
        }

        private static UISmartButton AddCloseButton(GameObject parent, bool anchored = true)
        {
            var go = new GameObject("CloseBtn", typeof(RectTransform), typeof(Image), typeof(UISmartButton));
            go.layer = parent.layer;
            go.transform.SetParent(parent.transform, false);

            if (anchored)
            {
                SetAnchors(go, new Vector2(0.5f, 0), new Vector2(0.5f, 0),
                    new Vector2(-100, 16), new Vector2(100, 80));
            }
            else
            {
                go.GetComponent<RectTransform>().sizeDelta = new Vector2(200, 64);
            }

            var bg = go.GetComponent<Image>();
            ApplyBtn(bg, "SL_UI_Btn_Dark");

            var closeIcon = AddChild(go, "XIcon");
            SetAnchors(closeIcon, new Vector2(0, 0.5f), new Vector2(0, 0.5f),
                new Vector2(16, -14), new Vector2(44, 14));
            var xImg = closeIcon.AddComponent<Image>();
            var xSprite = LoadUISprite("SL_UI_Close");
            if (xSprite != null)
            {
                xImg.sprite = xSprite;
                xImg.preserveAspect = true;
            }
            xImg.color = BtnTextColor;
            xImg.raycastTarget = false;

            var textGo = AddFullStretchChild(go, "Text");
            var tmp = textGo.AddComponent<TextMeshProUGUI>();
            tmp.text = "关 闭";
            tmp.fontSize = 24;
            tmp.color = BtnTextColor;
            tmp.alignment = TextAlignmentOptions.Center;
            BindLocKey(textGo, "btn_close");

            return go.GetComponent<UISmartButton>();
        }

        private static GameObject AddChild(GameObject parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.layer = parent.layer;
            go.transform.SetParent(parent.transform, false);
            return go;
        }

        private static GameObject AddFullStretchChild(GameObject parent, string name, RectOffset padding = null)
        {
            var go = AddChild(parent, name);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            if (padding != null)
            {
                rt.offsetMin = new Vector2(padding.left, padding.bottom);
                rt.offsetMax = new Vector2(-padding.right, -padding.top);
            }
            else
            {
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
            }
            return go;
        }

        private static void SetAnchors(GameObject go, Vector2 anchorMin, Vector2 anchorMax,
            Vector2 offsetMin, Vector2 offsetMax)
        {
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = offsetMin;
            rt.offsetMax = offsetMax;
        }

        private static Image CreateEntryIcon(GameObject parent, float size)
        {
            var iconGo = AddChild(parent, "Icon");
            SetSize(iconGo, size, size);
            var img = iconGo.AddComponent<Image>();
            img.color = new Color(0.8f, 0.7f, 0.5f, 0.4f);
            img.preserveAspect = true;
            img.enabled = false;
            return img;
        }

        private static void SetSize(GameObject go, float w, float h)
        {
            go.GetComponent<RectTransform>().sizeDelta = new Vector2(w, h);
        }

        private static TextMeshProUGUI AddText(GameObject parent, string name, string text, int fontSize = 22,
            string locKey = null)
        {
            var go = AddChild(parent, name);
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.color = BodyColor;
            tmp.alignment = TextAlignmentOptions.Center;
            if (!string.IsNullOrEmpty(locKey))
                BindLocKey(go, locKey);
            return tmp;
        }

        private static UISmartButton AddButton(GameObject parent, string name, string label, Vector2 size,
            int fontSize = 22, string locKey = null)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(UISmartButton));
            go.layer = parent.layer;
            go.transform.SetParent(parent.transform, false);
            go.GetComponent<RectTransform>().sizeDelta = size;

            var img = go.GetComponent<Image>();
            ApplyBtn(img, "SL_UI_Btn_Medium");

            var textGo = AddFullStretchChild(go, "Text");
            var tmp = textGo.AddComponent<TextMeshProUGUI>();
            tmp.text = label;
            tmp.fontSize = fontSize;
            tmp.color = BtnTextColor;
            tmp.alignment = TextAlignmentOptions.Center;
            if (!string.IsNullOrEmpty(locKey))
                BindLocKey(textGo, locKey);

            return go.GetComponent<UISmartButton>();
        }

        private static UISmartButtonGray AddGrayButton(GameObject parent, string name, string label, Vector2 size,
            int fontSize = 22, string locKey = null)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(UISmartButton), typeof(UISmartButtonGray));
            go.layer = parent.layer;
            go.transform.SetParent(parent.transform, false);
            go.GetComponent<RectTransform>().sizeDelta = size;

            var img = go.GetComponent<Image>();
            ApplyBtn(img, "SL_UI_Btn_Light");

            var textGo = AddFullStretchChild(go, "Text");
            var tmp = textGo.AddComponent<TextMeshProUGUI>();
            tmp.text = label;
            tmp.fontSize = fontSize;
            tmp.color = BodyColor;
            tmp.alignment = TextAlignmentOptions.Center;
            if (!string.IsNullOrEmpty(locKey))
                BindLocKey(textGo, locKey);

            return go.GetComponent<UISmartButtonGray>();
        }

        private static Button AddNativeButton(GameObject parent, string name, string label, Vector2 size,
            int fontSize = 22, string locKey = null)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            go.layer = parent.layer;
            go.transform.SetParent(parent.transform, false);
            go.GetComponent<RectTransform>().sizeDelta = size;

            var img = go.GetComponent<Image>();
            ApplyBtn(img, "SL_UI_Btn_Medium_Dark");

            var textGo = AddFullStretchChild(go, "Text");
            var tmp = textGo.AddComponent<TextMeshProUGUI>();
            tmp.text = label;
            tmp.fontSize = fontSize;
            tmp.color = BtnTextColor;
            tmp.alignment = TextAlignmentOptions.Center;
            if (!string.IsNullOrEmpty(locKey))
                BindLocKey(textGo, locKey);

            return go.GetComponent<Button>();
        }

        private static void BindLocKey(GameObject go, string key)
        {
            var loc = go.AddComponent<UILocalizedText>();
            Bind(loc, "_key", key);
        }

        private static void Bind(Object target, string field, string value)
        {
            var so = new SerializedObject(target);
            var prop = so.FindProperty(field);
            if (prop != null)
            {
                prop.stringValue = value;
                so.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        private static void AddHorizontalLayout(GameObject go, int spacing, TextAnchor align)
        {
            var hlg = go.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = spacing;
            hlg.childAlignment = align;
            hlg.childControlWidth = false;
            hlg.childControlHeight = false;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;
        }

        private static void AddVerticalLayout(GameObject go, int spacing, TextAnchor align)
        {
            var vlg = go.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = spacing;
            vlg.childAlignment = align;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
        }

        private static void SavePrefab(GameObject go, string folder, string name)
        {
            var dir = $"{PrefabRoot}/{folder}";
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
                AssetDatabase.Refresh();
            }

            var path = $"{dir}/{name}.prefab";
            PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            Debug.Log($"[UIPrefabGenerator] -> {path}");
        }

        private static GameObject SaveArtPrefab(GameObject go, string name)
        {
            if (!Directory.Exists(ArtPrefabRoot))
            {
                Directory.CreateDirectory(ArtPrefabRoot);
                AssetDatabase.Refresh();
            }

            var path = $"{ArtPrefabRoot}/{name}.prefab";
            var saved = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            Debug.Log($"[UIPrefabGenerator] (art) -> {path}");
            return saved;
        }

        private static void Bind(Object target, string field, Object value)
        {
            var so = new SerializedObject(target);
            var prop = so.FindProperty(field);
            if (prop != null)
            {
                prop.objectReferenceValue = value;
                so.ApplyModifiedPropertiesWithoutUndo();
            }
        }
    }
}
#endif
