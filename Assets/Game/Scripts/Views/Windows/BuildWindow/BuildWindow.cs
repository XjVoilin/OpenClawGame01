using System.Collections.Generic;
using IsleWorks.Grid;
using IsleWorks.Tech;
using JulyArch;
using JulyCore;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace IsleWorks.Views
{
    /// <summary>
    /// 建造面板 —— 底部按钮面板，根据当前时代显示可用机器。
    /// 点击按钮后通过 MachineSelectedEvent 通知 GridView。
    /// Prefab 结构：Root > PanelContainer(HorizontalLayoutGroup)
    /// </summary>
    public class BuildWindow : GameUIView
    {
        [SerializeField] private Transform _panelContainer;

        private readonly List<ButtonEntry> _buttonPool = new List<ButtonEntry>();

        protected override void OnBeforeOpen()
        {
            base.OnBeforeOpen();
            this.Subscribe<EraChangedEvent>(OnEraChanged);
            RefreshButtons();
        }

        private void RefreshButtons()
        {
            var buildable = this.Query<ITechQueries>().GetBuildableMachineIds();

            while (_buttonPool.Count < buildable.Count)
                _buttonPool.Add(CreateButtonEntry());

            for (int i = 0; i < buildable.Count; i++)
            {
                int machineId = buildable[i];
                var config = CfgTable.Machine.GetOrDefault(machineId);
                if (config == null) continue;

                var entry = _buttonPool[i];
                entry.Label.text = config.Name;
                entry.Image.color = PlaceholderVisuals.GetMachineColor(machineId);
                entry.MachineTypeId = machineId;
                entry.Root.SetActive(true);
            }

            for (int i = buildable.Count; i < _buttonPool.Count; i++)
                _buttonPool[i].Root.SetActive(false);
        }

        private ButtonEntry CreateButtonEntry()
        {
            var btnObj = new GameObject("BuildBtn");
            btnObj.transform.SetParent(_panelContainer, false);

            var rt = btnObj.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(90, 40);

            var image = btnObj.AddComponent<Image>();
            var button = btnObj.AddComponent<Button>();

            var textObj = new GameObject("Label");
            textObj.transform.SetParent(btnObj.transform, false);

            var textRt = textObj.AddComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = Vector2.zero;
            textRt.offsetMax = Vector2.zero;

            var text = textObj.AddComponent<TextMeshProUGUI>();
            text.fontSize = 16;
            text.color = Color.white;
            text.alignment = TextAlignmentOptions.Center;

            var entry = new ButtonEntry { Root = btnObj, Image = image, Label = text };
            button.onClick.AddListener(() =>
            {
                this.Publish(new MachineSelectedEvent(entry.MachineTypeId));
            });

            return entry;
        }

        private void OnEraChanged(EraChangedEvent e)
        {
            RefreshButtons();
        }

        private class ButtonEntry
        {
            public GameObject Root;
            public Image Image;
            public TextMeshProUGUI Label;
            public int MachineTypeId;
        }
    }
}
