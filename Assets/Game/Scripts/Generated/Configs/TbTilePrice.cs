using System.Collections.Generic;
using SimpleJSON;

namespace cfg
{
    public partial class TbTilePrice
    {
        private readonly Dictionary<int, TilePrice> _dataMap;
        private readonly List<TilePrice> _dataList;

        public TbTilePrice(JSONNode _buf)
        {
            _dataMap = new Dictionary<int, TilePrice>();
            _dataList = new List<TilePrice>();

            foreach (JSONNode _ele in _buf.Children)
            {
                var _v = TilePrice.DeserializeTilePrice(_ele);
                _dataList.Add(_v);
                _dataMap.Add(_v.Index, _v);
            }
        }

        public Dictionary<int, TilePrice> DataMap => _dataMap;
        public List<TilePrice> DataList => _dataList;

        public TilePrice GetOrDefault(int key) => _dataMap.TryGetValue(key, out var v) ? v : null;
        public TilePrice Get(int key) => _dataMap[key];
        public TilePrice this[int key] => _dataMap[key];

        public void ResolveRef(Tables tables)
        {
            foreach (var _v in _dataList) _v.ResolveRef(tables);
        }
    }
}
