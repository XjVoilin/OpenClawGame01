using Luban;
using SimpleJSON;

namespace cfg
{
    public sealed partial class TilePrice : Luban.BeanBase
    {
        public TilePrice(JSONNode _buf)
        {
            { if(!_buf["index"].IsNumber) throw new SerializationException(); Index = _buf["index"]; }
            { if(!_buf["price"].IsNumber) throw new SerializationException(); Price = _buf["price"]; }
        }

        public static TilePrice DeserializeTilePrice(JSONNode _buf) => new TilePrice(_buf);

        public readonly int Index;
        public readonly int Price;

        public const int __ID__ = 5;
        public override int GetTypeId() => __ID__;

        public void ResolveRef(Tables tables) { }

        public override string ToString()
        {
            return "{ " + "index:" + Index + ",price:" + Price + " }";
        }
    }
}
