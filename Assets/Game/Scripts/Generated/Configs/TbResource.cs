using System.Collections.Generic;
using SimpleJSON;

namespace cfg
{
    public partial class TbResource
    {
        private readonly Dictionary<int, Resource> _dataMap;
        private readonly List<Resource> _dataList;

        public TbResource(JSONNode _buf)
        {
            _dataMap = new Dictionary<int, Resource>();
            _dataList = new List<Resource>();

            foreach (JSONNode _ele in _buf.Children)
            {
                var _v = Resource.DeserializeResource(_ele);
                _dataList.Add(_v);
                _dataMap.Add(_v.Id, _v);
            }
        }

        public Dictionary<int, Resource> DataMap => _dataMap;
        public List<Resource> DataList => _dataList;

        public Resource GetOrDefault(int key) => _dataMap.TryGetValue(key, out var v) ? v : null;
        public Resource Get(int key) => _dataMap[key];
        public Resource this[int key] => _dataMap[key];

        public void ResolveRef(Tables tables)
        {
            foreach (var _v in _dataList) _v.ResolveRef(tables);
        }
    }
}
