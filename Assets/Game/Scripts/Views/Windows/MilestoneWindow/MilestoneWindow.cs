using System.Collections.Generic;
using cfg;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CozyYard
{
    public class MilestoneWindow : GameUIView
    {
        [SerializeField] private Transform _listContainer;
        [SerializeField] private GameObject _entryPrefab;
        [SerializeField] private TextMeshProUGUI _expansionText;
        [SerializeField] private Button _closeBtn;

        private readonly List<GameObject> _entries = new();

        protected override void OnViewEnable()
        {
            Subscribe<MilestoneAchievedEvent>(OnMilestoneAchieved);
            if (_closeBtn) _closeBtn.onClick.AddListener(OnClose);
            Refresh();
        }

        protected override void OnViewDisable()
        {
            if (_closeBtn) _closeBtn.onClick.RemoveAllListeners();
            ClearEntries();
        }

        private void OnMilestoneAchieved(MilestoneAchievedEvent e) => Refresh();

        private void Refresh()
        {
            ClearEntries();

            var milestoneStore = GetStore<MilestoneStore>();
            if (_expansionText) _expansionText.text = $"扩建等级: {milestoneStore.ExpansionLevel}";

            if (_entryPrefab == null || _listContainer == null) return;

            var tbMilestone = CfgTable.Tables?.TbMilestone;
            if (tbMilestone == null) return;

            foreach (var (id, cfg) in tbMilestone.DataMap)
            {
                var go = Object.Instantiate(_entryPrefab, _listContainer);
                go.SetActive(true);

                var progress = milestoneStore.GetProgress(id);
                int current = progress?.CurrentCount ?? 0;
                bool completed = progress?.Completed ?? false;
                string status = completed ? "已完成" : $"{current}/{cfg.ConditionCount}";

                var text = go.GetComponentInChildren<TextMeshProUGUI>();
                if (text) text.text = $"{cfg.Name}\n{cfg.Description}\n进度: {status}";

                _entries.Add(go);
            }
        }

        private void ClearEntries()
        {
            foreach (var go in _entries)
                Object.Destroy(go);
            _entries.Clear();
        }

        private void OnClose() => CloseWindow();
    }
}
