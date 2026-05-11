using System.Collections.Generic;
using IsleWorks.Production;
using IsleWorks.Tech;
using JulyArch;
using JulyCore;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace IsleWorks.Views
{
    /// <summary>
    /// 建造面板视图 —— 底部按钮面板，根据当前时代显示可用机器。
    /// </summary>
    public class BuildPanelView : GameView
    {
        public override IGameContext GetArchitecture() => AppArch.Context;

        private GridView _gridView;
        private Transform _panelContainer;
        private readonly List<GameObject> _buttons = new List<GameObject>();

        protected override void OnEnable()
        {
            base.OnEnable();
            this.Subscribe<EraChangedEvent>(OnEraChanged);
        }

        protected override void OnDisable()
        {
            this.Unsubscribe<EraChangedEvent>(OnEraChanged);
            base.OnDisable();
        }

        public void Initialize(GridView gridView)
        {
            _gridView = gridView;
            CreateUI();
            RefreshButtons();
            GF.Log("BuildPanelView initialized.");
        }

        private void CreateUI()
        {
            var canvasObj = new GameObject("BuildPanelCanvas");
            canvasObj.transform.SetParent(transform);
            var canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 101;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();

            var panelObj = new GameObject("BuildPanel");
            panelObj.transform.SetParent(canvasObj.transform, false);
            var panelRt = panelObj.AddComponent<RectTransform>();
            panelRt.anchorMin = new Vector2(0.5f, 0);
            panelRt.anchorMax = new Vector2(0.5f, 0);
            panelRt.pivot = new Vector2(0.5f, 0);
            panelRt.anchoredPosition = new Vector2(0, 10);
            panelRt.sizeDelta = new Vector2(600, 60);

            var hlg = panelObj.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 10;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.padding = new RectOffset(10, 10, 5, 5);

            _panelContainer = panelObj.transform;
        }

        private void RefreshButtons()
        {
            // Clear existing buttons
            for (int i = _buttons.Count - 1; i >= 0; i--)
            {
                if (_buttons[i] != null) Destroy(_buttons[i]);
            }
            _buttons.Clear();

            var tech = this.Query<ITechQueries>();

            // Always show Conveyor
            AddButton("Conveyor", (int)MachineType.Conveyor, PlaceholderVisuals.ConveyorColor);

            // Show unlocked machines (skip Conveyor=3 and Port=4)
            var machineIds = new int[] { 1, 2, 5, 6, 7, 8 };
            for (int i = 0; i < machineIds.Length; i++)
            {
                int id = machineIds[i];
                if (!tech.IsMachineUnlocked(id)) continue;

                var machineConfig = CfgTable.Machine.GetOrDefault(id);
                if (machineConfig == null) continue;

                var name = machineConfig.Name;
                var color = PlaceholderVisuals.GetMachineColor(id);
                AddButton(name, id, color);
            }
        }

        private void AddButton(string label, int machineTypeId, Color color)
        {
            var btnObj = new GameObject($"Btn_{label}");
            btnObj.transform.SetParent(_panelContainer, false);

            var rt = btnObj.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(90, 40);

            var image = btnObj.AddComponent<Image>();
            image.color = color;

            var button = btnObj.AddComponent<Button>();
            int typeId = machineTypeId;
            button.onClick.AddListener(() =>
            {
                _gridView.SetSelectedMachineType(typeId);
                GF.Log($"Selected: {label} ({typeId})");
            });

            var textObj = new GameObject("Label");
            textObj.transform.SetParent(btnObj.transform, false);
            var textRt = textObj.AddComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = Vector2.zero;
            textRt.offsetMax = Vector2.zero;

            var text = textObj.AddComponent<TextMeshProUGUI>();
            text.text = label;
            text.fontSize = 16;
            text.color = Color.white;
            text.alignment = TextAlignmentOptions.Center;

            _buttons.Add(btnObj);
        }

        private void OnEraChanged(EraChangedEvent e)
        {
            RefreshButtons();
        }
    }
}
