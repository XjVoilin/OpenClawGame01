using System.Collections.Generic;
using SimpleJSON;

namespace cfg
{
    public partial class TbMachine
    {
        private readonly Dictionary<int, Machine> _dataMap;
        private readonly List<Machine> _dataList;

        public TbMachine(JSONNode _buf)
        {
            _dataMap = new Dictionary<int, Machine>();
            _dataList = new List<Machine>();

            foreach (JSONNode _ele in _buf.Children)
            {
                var _v = Machine.DeserializeMachine(_ele);
                _dataList.Add(_v);
                _dataMap.Add(_v.Id, _v);
            }
        }

        public Dictionary<int, Machine> DataMap => _dataMap;
        public List<Machine> DataList => _dataList;

        public Machine GetOrDefault(int key) => _dataMap.TryGetValue(key, out var v) ? v : null;
        public Machine Get(int key) => _dataMap[key];
        public Machine this[int key] => _dataMap[key];

        public void ResolveRef(Tables tables)
        {
            foreach (var _v in _dataList) _v.ResolveRef(tables);
        }
    }
}
