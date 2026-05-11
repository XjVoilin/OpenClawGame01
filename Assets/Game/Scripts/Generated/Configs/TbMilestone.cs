using System.Collections.Generic;
using SimpleJSON;

namespace cfg
{
    public partial class TbMilestone
    {
        private readonly Dictionary<int, Milestone> _dataMap;
        private readonly List<Milestone> _dataList;

        public TbMilestone(JSONNode _buf)
        {
            _dataMap = new Dictionary<int, Milestone>();
            _dataList = new List<Milestone>();

            foreach (JSONNode _ele in _buf.Children)
            {
                var _v = Milestone.DeserializeMilestone(_ele);
                _dataList.Add(_v);
                _dataMap.Add(_v.Id, _v);
            }
        }

        public Dictionary<int, Milestone> DataMap => _dataMap;
        public List<Milestone> DataList => _dataList;

        public Milestone GetOrDefault(int key) => _dataMap.TryGetValue(key, out var v) ? v : null;
        public Milestone Get(int key) => _dataMap[key];
        public Milestone this[int key] => _dataMap[key];

        public void ResolveRef(Tables tables)
        {
            foreach (var _v in _dataList) _v.ResolveRef(tables);
        }
    }
}
