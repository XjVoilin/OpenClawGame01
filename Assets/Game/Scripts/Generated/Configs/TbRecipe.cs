using System.Collections.Generic;
using SimpleJSON;

namespace cfg
{
    public partial class TbRecipe
    {
        private readonly Dictionary<int, Recipe> _dataMap;
        private readonly List<Recipe> _dataList;

        public TbRecipe(JSONNode _buf)
        {
            _dataMap = new Dictionary<int, Recipe>();
            _dataList = new List<Recipe>();

            foreach (JSONNode _ele in _buf.Children)
            {
                var _v = Recipe.DeserializeRecipe(_ele);
                _dataList.Add(_v);
                _dataMap.Add(_v.Id, _v);
            }
        }

        public Dictionary<int, Recipe> DataMap => _dataMap;
        public List<Recipe> DataList => _dataList;

        public Recipe GetOrDefault(int key) => _dataMap.TryGetValue(key, out var v) ? v : null;
        public Recipe Get(int key) => _dataMap[key];
        public Recipe this[int key] => _dataMap[key];

        public void ResolveRef(Tables tables)
        {
            foreach (var _v in _dataList) _v.ResolveRef(tables);
        }
    }
}
