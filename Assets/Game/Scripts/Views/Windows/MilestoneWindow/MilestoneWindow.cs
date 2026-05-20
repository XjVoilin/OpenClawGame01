using System.Collections.Generic;
using JulyArch;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CozyYard
{
    public class MilestoneWindow : GameUIView
    {
        private static readonly int[] MilestoneIds = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

        private static readonly Dictionary<int, string> MilestoneNames = new()
        {
            { 1, "初次播种" }, { 2, "初次收获" }, { 3, "安家落户" }, { 4, "养鸡达人" },
            { 5, "初学厨艺" }, { 6, "远亲近邻" }, { 7, "小有规模" }, { 8, "丰收之秋" },
            { 9, "食谱收藏家" }, { 10, "远近闻名" },
        };

        private static readonly Dictionary<int, string> MilestoneDescriptions = new()
        {
            { 1, "播种第一株作物" }, { 2, "收获第一次作物" }, { 3, "建造茅草屋" },
            { 4, "收养第一只动物" }, { 5, "完成第一次烹饪" }, { 6, "完成第一笔订单" },
            { 7, "建造3座建筑" }, { 8, "累计收获10次作物" }, { 9, "解锁5个食谱" },
            { 10, "累计完成5笔订单" },
        };

        private static readonly Dictionary<int, int> MilestoneRequired = new()
        {
            { 1, 1 }, { 2, 1 }, { 3, 1 }, { 4, 1 }, { 5, 1 },
            { 6, 1 }, { 7, 3 }, { 8, 10 }, { 9, 5 }, { 10, 5 },
        };

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

            var q = this.Query<IMilestoneQueries>();
            if (_expansionText) _expansionText.text = $"扩建等级: {q.ExpansionLevel}";

            if (_entryPrefab == null || _listContainer == null) return;

            foreach (int id in MilestoneIds)
            {
                var go = Object.Instantiate(_entryPrefab, _listContainer);
                go.SetActive(true);

                var progress = q.GetProgress(id);
                int current = progress?.CurrentCount ?? 0;
                bool completed = progress?.Completed ?? false;
                int required = MilestoneRequired.TryGetValue(id, out var req) ? req : 1;

                string name = MilestoneNames.TryGetValue(id, out var n) ? n : $"#{id}";
                string desc = MilestoneDescriptions.TryGetValue(id, out var d) ? d : "";
                string status = completed ? "已完成" : $"{current}/{required}";

                var text = go.GetComponentInChildren<TextMeshProUGUI>();
                if (text) text.text = $"{name}\n{desc}\n进度: {status}";

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
