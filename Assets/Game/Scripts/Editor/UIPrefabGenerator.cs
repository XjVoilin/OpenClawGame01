#if UNITY_EDITOR
using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace SpiritHealer.Editor
{
    /// <summary>
    /// 一键生成灵药师 UI 预制体（竖屏 1080×1920）。
    /// 每个面板生成到各自子文件夹：UI/{Name}/{Name}.prefab
    /// </summary>
    public static class UIPrefabGenerator
    {
        private const string PrefabRoot = "Assets/Game/Res/Prefabs/UI";

        [MenuItem("SpiritHealer/生成所有 UI 预制体", false, 200)]
        public static void GenerateAll()
        {
            GenerateUITipItem();
            GenerateGameHUD();
            GenerateVisitorWindow();
            GeneratePrescriptionWindow();
            GenerateTreatmentResultWindow();
            AssetDatabase.Refresh();
            Debug.Log("[UIPrefabGenerator] 所有 UI 预制体已生成完毕 (1080×1920 竖屏)");
        }

        [MenuItem("SpiritHealer/生成所有 UI 预制体", true)]
        private static bool GenerateAllValidate() => !Application.isPlaying;

        // ══════════════════════════════════════════════
        //  UITipItem
        // ══════════════════════════════════════════════

        private static void GenerateUITipItem()
        {
            if (PrefabExists("UITipItem", "UITipItem")) return;

            var go = CreatePanelRoot("UITipItem", new Vector2(600, 60));

            var bg = AddFullStretchChild(go, "Bg");
            var bgImg = bg.AddComponent<Image>();
            bgImg.color = new Color(0.15f, 0.15f, 0.2f, 0.9f);

            var textGo = AddFullStretchChild(go, "Text", new RectOffset(20, 20, 8, 8));
            var text = textGo.AddComponent<TextMeshProUGUI>();
            text.text = "提示";
            text.fontSize = 24;
            text.color = Color.white;
            text.alignment = TextAlignmentOptions.Center;

            go.AddComponent<CanvasGroup>();
            var csf = go.AddComponent<ContentSizeFitter>();
            csf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;

            go.AddComponent<UITipItem>();

            SavePrefab(go, "UITipItem", "UITipItem");
        }

        // ══════════════════════════════════════════════
        //  GameHUD — 常驻主界面 (全屏 1080×1920)
        // ══════════════════════════════════════════════

        private static void GenerateGameHUD()
        {
            if (PrefabExists("GameHUD", "GameHUD")) return;

            var root = CreatePanelRoot("GameHUD", new Vector2(1080, 1920));
            StretchToParent(root);

            // ── 顶部信息栏 ──
            var topBar = AddChild(root, "TopBar");
            SetAnchors(topBar, new Vector2(0, 1), new Vector2(1, 1), new Vector2(20, -100), new Vector2(-20, -10));
            AddVerticalLayout(topBar, 8, TextAnchor.UpperCenter);

            var timeRow = AddChild(topBar, "TimeRow");
            SetSize(timeRow, 0, 40);
            AddHorizontalLayout(timeRow, 15, TextAnchor.MiddleCenter);

            var dayText = AddText(timeRow, "DayText", "第 1 天", 26);
            SetSize(dayText.gameObject, 180, 40);
            var seasonText = AddText(timeRow, "SeasonText", "春", 26);
            SetSize(seasonText.gameObject, 60, 40);
            var phaseText = AddText(timeRow, "PhaseText", "早晨", 26);
            SetSize(phaseText.gameObject, 100, 40);
            var timeText = AddText(timeRow, "TimeText", "08:00", 26);
            SetSize(timeText.gameObject, 120, 40);

            var resourceRow = AddChild(topBar, "ResourceRow");
            SetSize(resourceRow, 0, 36);
            AddHorizontalLayout(resourceRow, 30, TextAnchor.MiddleCenter);

            var coinsText = AddText(resourceRow, "CoinsText", "碎银: 0", 22);
            SetSize(coinsText.gameObject, 160, 36);
            var repText = AddText(resourceRow, "ReputationText", "声望: 0", 22);
            SetSize(repText.gameObject, 160, 36);

            // ── 中央区域 ──
            var center = AddChild(root, "CenterArea");
            SetAnchors(center, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            SetSize(center, 600, 200);
            AddVerticalLayout(center, 20, TextAnchor.MiddleCenter);

            var queueText = AddText(center, "QueueText", "等候: 0人", 28);
            SetSize(queueText.gameObject, 300, 50);

            // ── 底部按钮栏 ──
            var bottomBar = AddChild(root, "BottomBar");
            SetAnchors(bottomBar, new Vector2(0, 0), new Vector2(1, 0), new Vector2(40, 60), new Vector2(-40, 180));
            AddHorizontalLayout(bottomBar, 30, TextAnchor.MiddleCenter);

            var acceptBtn = AddButton(bottomBar, "AcceptVisitorBtn", "接  诊", new Vector2(260, 80), 28);
            var endDayBtn = AddButton(bottomBar, "EndDayBtn", "结束当天", new Vector2(260, 80), 28);

            // ── 绑定组件 ──
            var hud = root.AddComponent<GameHUD>();
            Bind(hud, "_dayText", dayText);
            Bind(hud, "_seasonText", seasonText);
            Bind(hud, "_phaseText", phaseText);
            Bind(hud, "_timeText", timeText);
            Bind(hud, "_coinsText", coinsText);
            Bind(hud, "_reputationText", repText);
            Bind(hud, "_queueText", queueText);
            Bind(hud, "_acceptVisitorBtn", acceptBtn);
            Bind(hud, "_endDayBtn", endDayBtn);

            SavePrefab(root, "GameHUD", "GameHUD");
        }

        // ══════════════════════════════════════════════
        //  VisitorWindow — 来客面板
        // ══════════════════════════════════════════════

        private static void GenerateVisitorWindow()
        {
            if (PrefabExists("VisitorWindow", "VisitorWindow")) return;

            var root = CreatePanelRoot("VisitorWindow", new Vector2(960, 1500));
            AddBg(root);

            // ── 标题 ──
            var title = AddText(root, "Title", "来  客", 30);
            SetAnchors(title.gameObject, new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(0, -60), new Vector2(0, -10));

            // ── 来客信息 ──
            var infoArea = AddChild(root, "InfoArea");
            SetAnchors(infoArea, new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(30, -160), new Vector2(-30, -70));
            AddVerticalLayout(infoArea, 6, TextAnchor.UpperLeft);

            var visitorName = AddText(infoArea, "VisitorName", "来客名", 24);
            SetSize(visitorName.gameObject, 0, 34);
            visitorName.alignment = TextAlignmentOptions.Left;
            var visitorType = AddText(infoArea, "VisitorType", "凡人", 20);
            SetSize(visitorType.gameObject, 0, 30);
            visitorType.alignment = TextAlignmentOptions.Left;
            visitorType.color = new Color(0.7f, 0.7f, 0.6f);
            var causeHint = AddText(infoArea, "CauseHint", "主诉……", 18);
            SetSize(causeHint.gameObject, 0, 28);
            causeHint.alignment = TextAlignmentOptions.Left;
            causeHint.color = new Color(0.8f, 0.8f, 0.7f);

            // ── 四诊区域 (2×2 网格) ──
            var diagArea = AddChild(root, "DiagnosisArea");
            SetAnchors(diagArea, new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(30, -400), new Vector2(-30, -175));
            var diagGrid = diagArea.AddComponent<GridLayoutGroup>();
            diagGrid.cellSize = new Vector2(400, 100);
            diagGrid.spacing = new Vector2(15, 12);
            diagGrid.childAlignment = TextAnchor.MiddleCenter;
            diagGrid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            diagGrid.constraintCount = 2;

            var wangG = CreateDiagGroup(diagArea, "WangGroup", "望诊");
            var wenG = CreateDiagGroup(diagArea, "WenGroup", "闻诊");
            var wen2G = CreateDiagGroup(diagArea, "Wen2Group", "问诊");
            var qieG = CreateDiagGroup(diagArea, "QieGroup", "切诊");

            // ── 诊断结果 ──
            var diagResult = AddText(root, "DiagnosisResult", "", 20);
            SetAnchors(diagResult.gameObject, new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(30, -440), new Vector2(-30, -405));
            diagResult.alignment = TextAlignmentOptions.Left;

            // ── 症状列表 (滚动区域) ──
            var symptomScroll = AddChild(root, "SymptomScroll");
            SetAnchors(symptomScroll, new Vector2(0, 0), new Vector2(1, 1),
                new Vector2(20, 120), new Vector2(-20, -460));

            var scrollRect = symptomScroll.AddComponent<ScrollRect>();
            var viewport = AddFullStretchChild(symptomScroll, "Viewport");
            viewport.AddComponent<RectMask2D>();
            var content = AddChild(viewport, "Content");
            SetAnchors(content, new Vector2(0, 1), new Vector2(1, 1), Vector2.zero, Vector2.zero);
            AddVerticalLayout(content, 6, TextAnchor.UpperLeft);
            var csf = content.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scrollRect.viewport = viewport.GetComponent<RectTransform>();
            scrollRect.content = content.GetComponent<RectTransform>();
            scrollRect.horizontal = false;

            var symptomItem = AddChild(content, "SymptomItem");
            SetSize(symptomItem, 0, 36);
            var stText = symptomItem.AddComponent<TextMeshProUGUI>();
            stText.text = "[望] 面色苍白";
            stText.fontSize = 20;
            stText.color = new Color(0.9f, 0.9f, 0.8f);
            stText.alignment = TextAlignmentOptions.Left;
            symptomItem.SetActive(false);

            // ── 底部按钮 ──
            var btnArea = AddChild(root, "ButtonArea");
            SetAnchors(btnArea, new Vector2(0, 0), new Vector2(1, 0),
                new Vector2(30, 20), new Vector2(-30, 100));
            AddHorizontalLayout(btnArea, 30, TextAnchor.MiddleCenter);

            var prescribeBtn = AddButton(btnArea, "PrescribeBtn", "开  方", new Vector2(280, 70), 26);
            var dismissBtn = AddButton(btnArea, "DismissBtn", "送  走", new Vector2(280, 70), 26);

            // ── 绑定 ──
            var panel = root.AddComponent<VisitorWindow>();
            Bind(panel, "_visitorName", visitorName);
            Bind(panel, "_visitorType", visitorType);
            Bind(panel, "_causeHint", causeHint);
            Bind(panel, "_wangBtn", wangG.btn);
            Bind(panel, "_wenBtn", wenG.btn);
            Bind(panel, "_wen2Btn", wen2G.btn);
            Bind(panel, "_qieBtn", qieG.btn);
            Bind(panel, "_wangLevel", wangG.levelText);
            Bind(panel, "_wenLevel", wenG.levelText);
            Bind(panel, "_wen2Level", wen2G.levelText);
            Bind(panel, "_qieLevel", qieG.levelText);
            Bind(panel, "_diagnosisResult", diagResult);
            Bind(panel, "_symptomListRoot", content.transform);
            Bind(panel, "_symptomItemPrefab", symptomItem);
            Bind(panel, "_prescribeBtn", prescribeBtn);
            Bind(panel, "_dismissBtn", dismissBtn);

            SavePrefab(root, "VisitorWindow", "VisitorWindow");
        }

        // ══════════════════════════════════════════════
        //  PrescriptionWindow — 处方面板
        // ══════════════════════════════════════════════

        private static void GeneratePrescriptionWindow()
        {
            if (PrefabExists("PrescriptionWindow", "PrescriptionWindow")) return;

            var root = CreatePanelRoot("PrescriptionWindow", new Vector2(960, 1600));
            AddBg(root);

            var title = AddText(root, "Title", "处  方", 30);
            SetAnchors(title.gameObject, new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(0, -60), new Vector2(0, -10));

            // ── 处方槽位 (2×2 网格) ──
            var slotsArea = AddChild(root, "SlotsArea");
            SetAnchors(slotsArea, new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(30, -440), new Vector2(-30, -80));
            var slotsGrid = slotsArea.AddComponent<GridLayoutGroup>();
            slotsGrid.cellSize = new Vector2(410, 160);
            slotsGrid.spacing = new Vector2(15, 15);
            slotsGrid.childAlignment = TextAnchor.MiddleCenter;
            slotsGrid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            slotsGrid.constraintCount = 2;

            var junSlot = CreateSlot(slotsArea, "JunSlot");
            var chenSlot = CreateSlot(slotsArea, "ChenSlot");
            var zuoSlot = CreateSlot(slotsArea, "ZuoSlot");
            var shiSlot = CreateSlot(slotsArea, "ShiSlot");

            // ── 药材列表 (滚动区域) ──
            var herbScroll = AddChild(root, "HerbScroll");
            SetAnchors(herbScroll, new Vector2(0, 0), new Vector2(1, 1),
                new Vector2(20, 110), new Vector2(-20, -460));

            var scrollRect = herbScroll.AddComponent<ScrollRect>();
            var viewport = AddFullStretchChild(herbScroll, "Viewport");
            viewport.AddComponent<RectMask2D>();
            var content = AddChild(viewport, "Content");
            SetAnchors(content, new Vector2(0, 1), new Vector2(1, 1), Vector2.zero, Vector2.zero);
            AddVerticalLayout(content, 8, TextAnchor.UpperLeft);
            var csf = content.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scrollRect.viewport = viewport.GetComponent<RectTransform>();
            scrollRect.content = content.GetComponent<RectTransform>();
            scrollRect.horizontal = false;

            var herbItem = CreateHerbItem(content);

            // ── 底部按钮 ──
            var btnArea = AddChild(root, "ButtonArea");
            SetAnchors(btnArea, new Vector2(0, 0), new Vector2(1, 0),
                new Vector2(20, 15), new Vector2(-20, 95));
            AddHorizontalLayout(btnArea, 15, TextAnchor.MiddleCenter);

            var confirmBtn = AddButton(btnArea, "ConfirmBtn", "确认开方", new Vector2(240, 65), 24);
            var clearBtn = AddButton(btnArea, "ClearBtn", "清  空", new Vector2(180, 65), 24);
            var closeBtn = AddButton(btnArea, "CloseBtn", "返  回", new Vector2(180, 65), 24);

            // ── 绑定 ──
            var panel = root.AddComponent<PrescriptionWindow>();
            Bind(panel, "_junSlot", junSlot);
            Bind(panel, "_chenSlot", chenSlot);
            Bind(panel, "_zuoSlot", zuoSlot);
            Bind(panel, "_shiSlot", shiSlot);
            Bind(panel, "_herbListRoot", content.transform);
            Bind(panel, "_herbItemPrefab", herbItem);
            Bind(panel, "_confirmBtn", confirmBtn);
            Bind(panel, "_clearBtn", clearBtn);
            Bind(panel, "_closeBtn", closeBtn);

            SavePrefab(root, "PrescriptionWindow", "PrescriptionWindow");
        }

        // ══════════════════════════════════════════════
        //  TreatmentResultWindow — 结算弹窗
        // ══════════════════════════════════════════════

        private static void GenerateTreatmentResultWindow()
        {
            if (PrefabExists("TreatmentResultWindow", "TreatmentResultWindow")) return;

            var root = CreatePanelRoot("TreatmentResultWindow", new Vector2(700, 600));
            AddBg(root);

            var contentArea = AddChild(root, "Content");
            SetAnchors(contentArea, new Vector2(0, 0), new Vector2(1, 1),
                new Vector2(30, 90), new Vector2(-30, -30));
            AddVerticalLayout(contentArea, 18, TextAnchor.MiddleCenter);

            var scoreText = AddText(contentArea, "ScoreText", "85", 56);
            SetSize(scoreText.gameObject, 200, 70);
            var gradeText = AddText(contentArea, "GradeText", "见效", 36);
            SetSize(gradeText.gameObject, 300, 50);
            var descText = AddText(contentArea, "DescriptionText", "症状有所缓解……", 20);
            SetSize(descText.gameObject, 600, 60);
            descText.color = new Color(0.85f, 0.85f, 0.8f);
            var rewardText = AddText(contentArea, "RewardText", "声望 +5  碎银 +10", 22);
            SetSize(rewardText.gameObject, 400, 40);
            rewardText.color = new Color(1f, 0.85f, 0.3f);

            var confirmBtn = AddButton(root, "ConfirmBtn", "确  认", new Vector2(240, 65), 26);
            SetAnchors(confirmBtn.gameObject, new Vector2(0.5f, 0), new Vector2(0.5f, 0),
                new Vector2(-120, 15), new Vector2(120, 80));

            var panel = root.AddComponent<TreatmentResultWindow>();
            Bind(panel, "_scoreText", scoreText);
            Bind(panel, "_gradeText", gradeText);
            Bind(panel, "_descriptionText", descText);
            Bind(panel, "_rewardText", rewardText);
            Bind(panel, "_confirmBtn", confirmBtn);

            SavePrefab(root, "TreatmentResultWindow", "TreatmentResultWindow");
        }

        // ════════════════════════════════════════
        //  构建辅助
        // ════════════════════════════════════════

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

        // ── Text ──

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

        // ── Button ──

        private static UISmartButton AddButton(GameObject parent, string name, string label, Vector2 size, int fontSize = 22)
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

        // ── Layout ──

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

        // ── Diagnosis Group ──

        private struct DiagGroup { public UISmartButton btn; public TextMeshProUGUI levelText; }

        private static DiagGroup CreateDiagGroup(GameObject parent, string name, string label)
        {
            var go = AddChild(parent, name);
            AddVerticalLayout(go, 6, TextAnchor.MiddleCenter);

            var levelText = AddText(go, "Level", $"{label} Lv.1", 16);
            SetSize(levelText.gameObject, 0, 24);
            var btn = AddButton(go, "Btn", label, new Vector2(200, 55), 22);

            return new DiagGroup { btn = btn, levelText = levelText };
        }

        // ── Prescription Slot ──

        private static PrescriptionSlotUI CreateSlot(GameObject parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent.transform, false);

            var bg = go.GetComponent<Image>();
            bg.color = new Color(0.16f, 0.16f, 0.2f);

            AddVerticalLayout(go, 6, TextAnchor.MiddleCenter);

            var roleLabel = AddText(go, "RoleLabel", "君", 26);
            SetSize(roleLabel.gameObject, 0, 34);
            var herbName = AddText(go, "HerbName", "空", 20);
            SetSize(herbName.gameObject, 0, 30);
            var qualityText = AddText(go, "QualityText", "", 16);
            SetSize(qualityText.gameObject, 0, 24);
            qualityText.color = new Color(0.7f, 0.7f, 0.6f);

            var slotBtn = go.AddComponent<UISmartButton>();

            var clearBtn = AddButton(go, "ClearBtn", "×", new Vector2(40, 34), 18);
            clearBtn.gameObject.SetActive(false);

            var highlight = new GameObject("Highlight", typeof(RectTransform), typeof(Image));
            highlight.transform.SetParent(go.transform, false);
            highlight.transform.SetAsFirstSibling();
            var hrt = highlight.GetComponent<RectTransform>();
            hrt.anchorMin = Vector2.zero;
            hrt.anchorMax = Vector2.one;
            hrt.offsetMin = new Vector2(-3, -3);
            hrt.offsetMax = new Vector2(3, 3);
            var himg = highlight.GetComponent<Image>();
            himg.color = new Color(1f, 0.8f, 0.2f, 0.5f);
            himg.raycastTarget = false;
            himg.enabled = false;

            var comp = go.AddComponent<PrescriptionSlotUI>();
            Bind(comp, "_roleLabel", roleLabel);
            Bind(comp, "_herbName", herbName);
            Bind(comp, "_qualityText", qualityText);
            Bind(comp, "_slotBtn", slotBtn);
            Bind(comp, "_clearBtn", clearBtn);
            Bind(comp, "_highlight", himg);

            return comp;
        }

        // ── Herb Item ──

        private static GameObject CreateHerbItem(GameObject parent)
        {
            var go = new GameObject("HerbItem", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent.transform, false);
            SetSize(go, 0, 60);

            var bg = go.GetComponent<Image>();
            bg.color = new Color(0.18f, 0.18f, 0.22f, 0.8f);

            AddHorizontalLayout(go, 12, TextAnchor.MiddleLeft);
            var hlg = go.GetComponent<HorizontalLayoutGroup>();
            hlg.padding = new RectOffset(15, 15, 6, 6);

            var nameText = AddText(go, "NameText", "甘草", 20);
            nameText.alignment = TextAlignmentOptions.Left;
            SetSize(nameText.gameObject, 120, 48);

            var countText = AddText(go, "CountText", "x5", 18);
            countText.alignment = TextAlignmentOptions.Center;
            SetSize(countText.gameObject, 50, 48);

            var infoText = AddText(go, "InfoText", "未知", 16);
            infoText.alignment = TextAlignmentOptions.Left;
            infoText.color = new Color(0.7f, 0.7f, 0.6f);
            SetSize(infoText.gameObject, 300, 48);

            var selectBtn = go.AddComponent<UISmartButton>();

            var comp = go.AddComponent<HerbItemUI>();
            Bind(comp, "_nameText", nameText);
            Bind(comp, "_countText", countText);
            Bind(comp, "_infoText", infoText);
            Bind(comp, "_selectBtn", selectBtn);

            go.SetActive(false);
            return go;
        }

        // ── Save / Bind ──

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
