#if UNITY_EDITOR
using System.IO;
using JulyCore;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace CozyYard.Editor
{
    /// <summary>
    /// 一键生成 CozyYard UI 预制体（横屏 1920×1080）
    /// </summary>
    public static class UIPrefabGenerator
    {
        private const string PrefabRoot = "Assets/Game/Res/Prefabs/UI";
        private static readonly Vector2 LandscapeSize = new(1920, 1080);

        [MenuItem("CozyYard/生成所有 UI 预制体", false, 200)]
        public static void GenerateAll()
        {
            GenerateGameHUD();
            GenerateInventoryWindow();
            GenerateBuildWindow();
            GenerateCraftWindow();
            GenerateVisitorWindow();
            GenerateMilestoneWindow();
            GenerateRecipeBookWindow();
            GeneratePhoneWindow();
            AssetDatabase.Refresh();
            Debug.Log("[UIPrefabGenerator] CozyYard UI 预制体已生成完毕 (1920×1080 横屏)");
        }

        [MenuItem("CozyYard/生成所有 UI 预制体", true)]
        private static bool GenerateAllValidate() => !Application.isPlaying;

        // ══════════════════════════════════════════════
        //  GameHUD — 常驻主界面 (全屏 1920×1080)
        // ══════════════════════════════════════════════

        private static void GenerateGameHUD()
        {
            if (PrefabExists("GameHUD", "GameHUD")) return;

            var root = CreatePanelRoot("GameHUD", LandscapeSize);
            StretchToParent(root);

            // ── 右上角：大门状态 ──
            var topRight = AddChild(root, "TopRight");
            SetAnchors(topRight, new Vector2(1, 1), new Vector2(1, 1),
                new Vector2(-320, -90), new Vector2(-20, -20));
            AddVerticalLayout(topRight, 8, TextAnchor.UpperRight);

            var gateText = AddText(topRight, "GateText", "大门: 开", 22);
            SetSize(gateText.gameObject, 280, 36);
            gateText.alignment = TextAlignmentOptions.Right;

            var gateToggleBtn = AddButton(topRight, "GateToggleBtn", "切换大门", new Vector2(160, 44), 20);

            // ── 来客角标（访客按钮上显示） ──
            var visitorBadgeText = AddText(root, "VisitorBadgeText", "1", 18);
            SetAnchors(visitorBadgeText.gameObject, new Vector2(0.5f, 0), new Vector2(0.5f, 0),
                new Vector2(200, 95), new Vector2(230, 125));
            visitorBadgeText.color = new Color(1f, 0.85f, 0.3f);

            // ── 底部导航栏 ──
            var bottomBar = AddChild(root, "BottomBar");
            SetAnchors(bottomBar, new Vector2(0, 0), new Vector2(1, 0),
                new Vector2(40, 20), new Vector2(-40, 100));
            AddHorizontalLayout(bottomBar, 20, TextAnchor.MiddleCenter);

            var inventoryBtn = AddButton(bottomBar, "InventoryBtn", "背包", new Vector2(120, 56), 20);
            var buildBtn = AddButton(bottomBar, "BuildBtn", "建造", new Vector2(120, 56), 20);
            var craftBtn = AddButton(bottomBar, "CraftBtn", "制作", new Vector2(120, 56), 20);
            var visitorBtn = AddButton(bottomBar, "VisitorBtn", "来客", new Vector2(120, 56), 20);
            var milestoneBtn = AddButton(bottomBar, "MilestoneBtn", "里程碑", new Vector2(120, 56), 20);
            var recipeBookBtn = AddButton(bottomBar, "RecipeBookBtn", "配方本", new Vector2(120, 56), 20);
            var phoneBtn = AddButton(bottomBar, "PhoneBtn", "问妈", new Vector2(120, 56), 20);

            var hud = root.AddComponent<GameHUD>();
            Bind(hud, "_inventoryBtn", inventoryBtn);
            Bind(hud, "_buildBtn", buildBtn);
            Bind(hud, "_craftBtn", craftBtn);
            Bind(hud, "_visitorBtn", visitorBtn);
            Bind(hud, "_milestoneBtn", milestoneBtn);
            Bind(hud, "_recipeBookBtn", recipeBookBtn);
            Bind(hud, "_phoneBtn", phoneBtn);
            Bind(hud, "_gateToggleBtn", gateToggleBtn);
            Bind(hud, "_gateText", gateText);
            Bind(hud, "_visitorBadgeText", visitorBadgeText);

            SavePrefab(root, "GameHUD", "GameHUD");
        }

        // ══════════════════════════════════════════════
        //  InventoryWindow — 背包
        // ══════════════════════════════════════════════

        private static void GenerateInventoryWindow()
        {
            if (PrefabExists("InventoryWindow", "InventoryWindow")) return;

            var root = CreatePanelRoot("InventoryWindow", new Vector2(600, 500));
            AddBg(root);

            var title = AddText(root, "Title", "背  包", 28);
            SetAnchors(title.gameObject, new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(0, -50), new Vector2(0, -8));

            var statusBar = AddChild(root, "StatusBar");
            SetAnchors(statusBar, new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(20, -95), new Vector2(-20, -55));
            AddHorizontalLayout(statusBar, 30, TextAnchor.MiddleLeft);

            var capacityText = AddText(statusBar, "CapacityText", "0/20", 20);
            SetSize(capacityText.gameObject, 160, 36);
            capacityText.alignment = TextAlignmentOptions.Left;
            var coinsText = AddText(statusBar, "CoinsText", "0", 20);
            SetSize(coinsText.gameObject, 160, 36);
            coinsText.alignment = TextAlignmentOptions.Left;
            coinsText.color = new Color(1f, 0.85f, 0.3f);

            var scrollArea = AddChild(root, "ScrollArea");
            SetAnchors(scrollArea, new Vector2(0, 0), new Vector2(1, 1),
                new Vector2(20, 60), new Vector2(-20, -100));
            var itemsContainerGo = CreateScrollContent(scrollArea, "ItemsContainer", out _).gameObject;
            Object.DestroyImmediate(itemsContainerGo.GetComponent<VerticalLayoutGroup>());
            var grid = itemsContainerGo.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(120, 50);
            grid.spacing = new Vector2(8, 8);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 4;
            grid.childAlignment = TextAnchor.UpperLeft;

            var itemSlotPrefab = CreateItemSlot(itemsContainerGo);

            var closeBtn = AddButton(root, "CloseBtn", "关  闭", new Vector2(140, 44), 20);
            SetAnchors(closeBtn.gameObject, new Vector2(0.5f, 0), new Vector2(0.5f, 0),
                new Vector2(-70, 12), new Vector2(70, 56));

            var panel = root.AddComponent<InventoryWindow>();
            Bind(panel, "_itemsContainer", itemsContainerGo.transform);
            Bind(panel, "_itemSlotPrefab", itemSlotPrefab);
            Bind(panel, "_capacityText", capacityText);
            Bind(panel, "_coinsText", coinsText);
            Bind(panel, "_closeBtn", closeBtn);

            SavePrefab(root, "InventoryWindow", "InventoryWindow");
        }

        // ══════════════════════════════════════════════
        //  BuildWindow — 建造面板
        // ══════════════════════════════════════════════

        private static void GenerateBuildWindow()
        {
            if (PrefabExists("BuildWindow", "BuildWindow")) return;

            var root = CreatePanelRoot("BuildWindow", new Vector2(500, 600));
            AddBg(root);

            var title = AddText(root, "Title", "建  造", 28);
            SetAnchors(title.gameObject, new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(0, -50), new Vector2(0, -8));

            var scrollArea = AddChild(root, "ScrollArea");
            SetAnchors(scrollArea, new Vector2(0, 0), new Vector2(1, 1),
                new Vector2(16, 60), new Vector2(-16, -58));
            var listContainer = CreateScrollContent(scrollArea, "ListContainer", out _);

            var entryPrefab = CreateBuildEntry(listContainer);

            var closeBtn = AddButton(root, "CloseBtn", "关  闭", new Vector2(140, 44), 20);
            SetAnchors(closeBtn.gameObject, new Vector2(0.5f, 0), new Vector2(0.5f, 0),
                new Vector2(-70, 12), new Vector2(70, 56));

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
            if (PrefabExists("CraftWindow", "CraftWindow")) return;

            var root = CreatePanelRoot("CraftWindow", new Vector2(560, 620));
            AddBg(root);

            var title = AddText(root, "Title", "制  作", 28);
            SetAnchors(title.gameObject, new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(0, -50), new Vector2(0, -8));

            var scrollArea = AddChild(root, "ScrollArea");
            SetAnchors(scrollArea, new Vector2(0, 0), new Vector2(1, 1),
                new Vector2(16, 60), new Vector2(-16, -58));
            var listContainer = CreateScrollContent(scrollArea, "ListContainer", out _);

            var entryPrefab = CreateCraftEntry(listContainer);

            var closeBtn = AddButton(root, "CloseBtn", "关  闭", new Vector2(140, 44), 20);
            SetAnchors(closeBtn.gameObject, new Vector2(0.5f, 0), new Vector2(0.5f, 0),
                new Vector2(-70, 12), new Vector2(70, 56));

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
            if (PrefabExists("VisitorWindow", "VisitorWindow")) return;

            var root = CreatePanelRoot("VisitorWindow", new Vector2(640, 580));
            AddBg(root);

            var title = AddText(root, "Title", "来  客", 28);
            SetAnchors(title.gameObject, new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(0, -50), new Vector2(0, -8));

            var gateRow = AddChild(root, "GateRow");
            SetAnchors(gateRow, new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(20, -95), new Vector2(-20, -55));
            AddHorizontalLayout(gateRow, 16, TextAnchor.MiddleLeft);

            var gateText = AddText(gateRow, "GateText", "大门: 开", 20);
            SetSize(gateText.gameObject, 180, 36);
            gateText.alignment = TextAlignmentOptions.Left;
            var gateToggleBtn = AddButton(gateRow, "GateToggleBtn", "切换大门", new Vector2(140, 40), 18);

            var scrollArea = AddChild(root, "ScrollArea");
            SetAnchors(scrollArea, new Vector2(0, 0), new Vector2(1, 1),
                new Vector2(16, 60), new Vector2(-16, -100));
            var listContainer = CreateScrollContent(scrollArea, "ListContainer", out _);

            var entryPrefab = CreateVisitorEntry(listContainer);

            var closeBtn = AddButton(root, "CloseBtn", "关  闭", new Vector2(140, 44), 20);
            SetAnchors(closeBtn.gameObject, new Vector2(0.5f, 0), new Vector2(0.5f, 0),
                new Vector2(-70, 12), new Vector2(70, 56));

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
            if (PrefabExists("MilestoneWindow", "MilestoneWindow")) return;

            var root = CreatePanelRoot("MilestoneWindow", new Vector2(560, 620));
            AddBg(root);

            var title = AddText(root, "Title", "里程碑", 28);
            SetAnchors(title.gameObject, new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(0, -50), new Vector2(0, -8));

            var expansionText = AddText(root, "ExpansionText", "扩建等级: 0", 20);
            SetAnchors(expansionText.gameObject, new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(20, -95), new Vector2(-20, -58));
            expansionText.alignment = TextAlignmentOptions.Left;

            var scrollArea = AddChild(root, "ScrollArea");
            SetAnchors(scrollArea, new Vector2(0, 0), new Vector2(1, 1),
                new Vector2(16, 60), new Vector2(-16, -100));
            var listContainer = CreateScrollContent(scrollArea, "ListContainer", out _);

            var entryPrefab = CreateMilestoneEntry(listContainer);

            var closeBtn = AddButton(root, "CloseBtn", "关  闭", new Vector2(140, 44), 20);
            SetAnchors(closeBtn.gameObject, new Vector2(0.5f, 0), new Vector2(0.5f, 0),
                new Vector2(-70, 12), new Vector2(70, 56));

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
            if (PrefabExists("RecipeBookWindow", "RecipeBookWindow")) return;

            var root = CreatePanelRoot("RecipeBookWindow", new Vector2(560, 620));
            AddBg(root);

            var title = AddText(root, "Title", "配方本", 28);
            SetAnchors(title.gameObject, new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(0, -50), new Vector2(0, -8));

            var scrollArea = AddChild(root, "ScrollArea");
            SetAnchors(scrollArea, new Vector2(0, 0), new Vector2(1, 1),
                new Vector2(16, 60), new Vector2(-16, -58));
            var listContainer = CreateScrollContent(scrollArea, "ListContainer", out _);

            var entryPrefab = CreateRecipeBookEntry(listContainer);

            var closeBtn = AddButton(root, "CloseBtn", "关  闭", new Vector2(140, 44), 20);
            SetAnchors(closeBtn.gameObject, new Vector2(0.5f, 0), new Vector2(0.5f, 0),
                new Vector2(-70, 12), new Vector2(70, 56));

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
            if (PrefabExists("PhoneWindow", "PhoneWindow")) return;

            var root = CreatePanelRoot("PhoneWindow", new Vector2(560, 620));
            AddBg(root);

            var title = AddText(root, "Title", "问  妈", 28);
            SetAnchors(title.gameObject, new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(0, -50), new Vector2(0, -8));

            var hintText = AddText(root, "HintText", "告诉妈妈你有什么材料，她可能知道配方", 18);
            SetAnchors(hintText.gameObject, new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(20, -95), new Vector2(-20, -58));
            hintText.alignment = TextAlignmentOptions.Left;
            hintText.color = new Color(0.85f, 0.85f, 0.8f);

            var asksRemainingText = AddText(root, "AsksRemainingText", "今日剩余询问: 1/1", 18);
            SetAnchors(asksRemainingText.gameObject, new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(20, -130), new Vector2(-20, -98));
            asksRemainingText.alignment = TextAlignmentOptions.Left;

            var scrollArea = AddChild(root, "ItemScrollArea");
            SetAnchors(scrollArea, new Vector2(0, 0), new Vector2(1, 1),
                new Vector2(16, 120), new Vector2(-16, -140));
            var itemsContainer = CreateScrollContent(scrollArea, "ItemsContainer", out _);

            var itemEntryPrefab = CreatePhoneItemEntry(itemsContainer);

            var resultText = AddText(root, "ResultText", "", 18);
            SetAnchors(resultText.gameObject, new Vector2(0, 0), new Vector2(1, 0),
                new Vector2(20, 68), new Vector2(-20, 108));
            resultText.alignment = TextAlignmentOptions.Left;
            resultText.color = new Color(0.9f, 0.9f, 0.75f);

            var btnArea = AddChild(root, "ButtonArea");
            SetAnchors(btnArea, new Vector2(0, 0), new Vector2(1, 0),
                new Vector2(20, 12), new Vector2(-20, 60));
            AddHorizontalLayout(btnArea, 20, TextAnchor.MiddleCenter);

            var askBtn = AddButton(btnArea, "AskBtn", "询问妈妈", new Vector2(160, 44), 20);
            var closeBtn = AddButton(btnArea, "CloseBtn", "关  闭", new Vector2(140, 44), 20);

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
        //  Entry Prefabs
        // ══════════════════════════════════════════════

        private static GameObject CreateItemSlot(GameObject parent)
        {
            var go = new GameObject("ItemSlotPrefab", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent.transform, false);
            SetSize(go, 120, 50);

            var bg = go.GetComponent<Image>();
            bg.color = new Color(0.18f, 0.18f, 0.22f, 0.85f);

            var textGo = AddFullStretchChild(go, "Text", new RectOffset(8, 8, 4, 4));
            var text = textGo.AddComponent<TextMeshProUGUI>();
            text.text = "#1001 x5";
            text.fontSize = 16;
            text.color = Color.white;
            text.alignment = TextAlignmentOptions.Center;

            go.SetActive(false);
            return go;
        }

        private static GameObject CreateBuildEntry(GameObject parent)
        {
            var go = new GameObject("EntryPrefab", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent.transform, false);
            SetSize(go, 0, 56);

            var bg = go.GetComponent<Image>();
            bg.color = new Color(0.18f, 0.18f, 0.22f, 0.85f);

            AddHorizontalLayout(go, 12, TextAnchor.MiddleLeft);
            var hlg = go.GetComponent<HorizontalLayoutGroup>();
            hlg.padding = new RectOffset(12, 12, 6, 6);

            var infoText = AddText(go, "InfoText", "茅草屋 (#1003×20)", 18);
            infoText.alignment = TextAlignmentOptions.Left;
            SetSize(infoText.gameObject, 280, 44);

            AddButton(go, "BuildBtn", "建造", new Vector2(90, 40), 18);

            go.SetActive(false);
            return go;
        }

        private static GameObject CreateCraftEntry(GameObject parent)
        {
            var go = new GameObject("EntryPrefab", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent.transform, false);
            SetSize(go, 0, 80);

            var bg = go.GetComponent<Image>();
            bg.color = new Color(0.18f, 0.18f, 0.22f, 0.85f);

            AddHorizontalLayout(go, 12, TextAnchor.MiddleLeft);
            var hlg = go.GetComponent<HorizontalLayoutGroup>();
            hlg.padding = new RectOffset(12, 12, 6, 6);

            var infoText = AddText(go, "InfoText", "桂花干\n材料: #3006×3\n产出: #4001×2", 16);
            infoText.alignment = TextAlignmentOptions.Left;
            SetSize(infoText.gameObject, 340, 68);

            AddButton(go, "CraftBtn", "制作", new Vector2(90, 40), 18);

            go.SetActive(false);
            return go;
        }

        private static GameObject CreateVisitorEntry(GameObject parent)
        {
            var go = new GameObject("EntryPrefab", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent.transform, false);
            SetSize(go, 0, 90);

            var bg = go.GetComponent<Image>();
            bg.color = new Color(0.18f, 0.18f, 0.22f, 0.85f);

            AddHorizontalLayout(go, 10, TextAnchor.MiddleLeft);
            var hlg = go.GetComponent<HorizontalLayoutGroup>();
            hlg.padding = new RectOffset(12, 12, 6, 6);

            var infoText = AddText(go, "InfoText", "张阿婆\n需要: #5001×1\n奖励: 30 金币", 16);
            infoText.alignment = TextAlignmentOptions.Left;
            SetSize(infoText.gameObject, 300, 78);

            AddButton(go, "FulfillBtn", "交付", new Vector2(80, 40), 18);
            AddButton(go, "DismissBtn", "送走", new Vector2(80, 40), 18);

            go.SetActive(false);
            return go;
        }

        private static GameObject CreateMilestoneEntry(GameObject parent)
        {
            var go = new GameObject("EntryPrefab", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent.transform, false);
            SetSize(go, 0, 72);

            var bg = go.GetComponent<Image>();
            bg.color = new Color(0.18f, 0.18f, 0.22f, 0.85f);

            var textGo = AddFullStretchChild(go, "Text", new RectOffset(12, 12, 6, 6));
            var text = textGo.AddComponent<TextMeshProUGUI>();
            text.text = "初次播种\n播种第一株作物\n进度: 0/1";
            text.fontSize = 16;
            text.color = Color.white;
            text.alignment = TextAlignmentOptions.Left;

            go.SetActive(false);
            return go;
        }

        private static GameObject CreateRecipeBookEntry(GameObject parent)
        {
            var go = new GameObject("EntryPrefab", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent.transform, false);
            SetSize(go, 0, 72);

            var bg = go.GetComponent<Image>();
            bg.color = new Color(0.18f, 0.18f, 0.22f, 0.85f);

            var textGo = AddFullStretchChild(go, "Text", new RectOffset(12, 12, 6, 6));
            var text = textGo.AddComponent<TextMeshProUGUI>();
            text.text = "桂花干\n材料: #3006×3\n产出: #4001×2";
            text.fontSize = 16;
            text.color = Color.white;
            text.alignment = TextAlignmentOptions.Left;

            go.SetActive(false);
            return go;
        }

        private static GameObject CreatePhoneItemEntry(GameObject parent)
        {
            var go = new GameObject("ItemEntryPrefab", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent.transform, false);
            SetSize(go, 0, 44);

            var bg = go.GetComponent<Image>();
            bg.color = new Color(0.18f, 0.18f, 0.22f, 0.85f);

            AddHorizontalLayout(go, 8, TextAnchor.MiddleLeft);
            var hlg = go.GetComponent<HorizontalLayoutGroup>();
            hlg.padding = new RectOffset(10, 10, 4, 4);

            var infoText = AddText(go, "InfoText", "#1001 ×5", 16);
            infoText.alignment = TextAlignmentOptions.Left;
            SetSize(infoText.gameObject, 320, 36);

            go.AddComponent<UISmartButton>();

            go.SetActive(false);
            return go;
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
            img.color = new Color(0.1f, 0.1f, 0.13f, 0.95f);
        }

        private static GameObject AddChild(GameObject parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform));
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

        private static void SetSize(GameObject go, float w, float h)
        {
            go.GetComponent<RectTransform>().sizeDelta = new Vector2(w, h);
        }

        private static TextMeshProUGUI AddText(GameObject parent, string name, string text, int fontSize = 22)
        {
            var go = AddChild(parent, name);
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.color = Color.white;
            tmp.alignment = TextAlignmentOptions.Center;
            return tmp;
        }

        private static Button AddButton(GameObject parent, string name, string label, Vector2 size, int fontSize = 22)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(UISmartButton));
            go.transform.SetParent(parent.transform, false);
            go.GetComponent<RectTransform>().sizeDelta = size;

            var img = go.GetComponent<Image>();
            img.color = new Color(0.22f, 0.22f, 0.28f);

            var textGo = AddFullStretchChild(go, "Text");
            var tmp = textGo.AddComponent<TextMeshProUGUI>();
            tmp.text = label;
            tmp.fontSize = fontSize;
            tmp.color = Color.white;
            tmp.alignment = TextAlignmentOptions.Center;

            return go.GetComponent<UISmartButton>();
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

        private static bool PrefabExists(string folder, string name)
        {
            return File.Exists($"{PrefabRoot}/{folder}/{name}.prefab");
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
