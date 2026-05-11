using Luban;
using SimpleJSON;

namespace cfg
{
    public sealed partial class Resource : Luban.BeanBase
    {
        public Resource(JSONNode _buf)
        {
            { if(!_buf["id"].IsNumber) throw new SerializationException(); Id = _buf["id"]; }
            { if(!_buf["name"].IsString) throw new SerializationException(); Name = _buf["name"]; }
            { if(!_buf["sellPrice"].IsNumber) throw new SerializationException(); SellPrice = _buf["sellPrice"]; }
            { if(!_buf["depth"].IsNumber) throw new SerializationException(); Depth = _buf["depth"]; }
        }

        public static Resource DeserializeResource(JSONNode _buf) => new Resource(_buf);

        public readonly int Id;
        public readonly string Name;
        public readonly int SellPrice;
        public readonly int Depth;

        public const int __ID__ = 3;
        public override int GetTypeId() => __ID__;

        public void ResolveRef(Tables tables) { }

        public override string ToString()
        {
            return "{ " + "id:" + Id + ",name:" + Name + ",sellPrice:" + SellPrice + ",depth:" + Depth + " }";
        }
    }
}
