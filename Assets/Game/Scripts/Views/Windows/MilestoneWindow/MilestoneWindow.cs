using System.Collections.Generic;
using cfg;
using JulyCore;
using TMPro;
using UnityEngine;

namespace CozyYard
{
    public class MilestoneWindow : GameUIView
    {
        [SerializeField] private Transform _listContainer;
        [SerializeField] private MilestoneEntry _entryPrefab;
        [SerializeField] private TextMeshProUGUI _expansionText;
        [SerializeField] private UISmartButton _closeBtn;

        private readonly List<MilestoneEntry> _entries = new();

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
            if (_expansionText) _expansionText.text = string.Format(GF.Localization.Get("expansion_level"), milestoneStore.ExpansionLevel);

            if (_entryPrefab == null || _listContainer == null) return;

            var tbMilestone = GF.Config.GetTable<TbMilestone>();
            if (tbMilestone == null) return;

            foreach (var (id, cfg) in tbMilestone.DataMap)
            {
                var entry = Object.Instantiate(_entryPrefab, _listContainer);
                entry.gameObject.SetActive(true);

                var progress = milestoneStore.GetProgress(id);
                int current = progress?.CurrentCount ?? 0;
                bool completed = progress?.Completed ?? false;
                string status = completed
                    ? GF.Localization.Get("completed")
                    : string.Format(GF.Localization.Get("progress"), $"{current}/{cfg.ConditionCount}");

                string info = $"{GF.Localization.Get(cfg.NameKey)}\n{GF.Localization.Get(cfg.DescKey)}\n{status}";
                entry.Setup(info);

                _entries.Add(entry);
            }
        }

        private void ClearEntries()
        {
            foreach (var entry in _entries)
                Object.Destroy(entry.gameObject);
            _entries.Clear();
        }

        private void OnClose() => CloseWindow();
    }
}
