using Luban;
using SimpleJSON;

namespace cfg
{
    public sealed partial class Recipe : Luban.BeanBase
    {
        public Recipe(JSONNode _buf)
        {
            { if(!_buf["id"].IsNumber) throw new SerializationException(); Id = _buf["id"]; }
            {
                var _inputsNode = _buf["inputs"];
                Inputs = new int[_inputsNode.Count];
                for (int i = 0; i < _inputsNode.Count; i++) Inputs[i] = _inputsNode[i];
            }
            {
                var _qtyNode = _buf["inputQuantities"];
                InputQuantities = new int[_qtyNode.Count];
                for (int i = 0; i < _qtyNode.Count; i++) InputQuantities[i] = _qtyNode[i];
            }
            { if(!_buf["output"].IsNumber) throw new SerializationException(); Output = _buf["output"]; }
            { if(!_buf["processTime"].IsNumber) throw new SerializationException(); ProcessTime = _buf["processTime"]; }
            { if(!_buf["requiredEra"].IsNumber) throw new SerializationException(); RequiredEra = _buf["requiredEra"]; }
        }

        public static Recipe DeserializeRecipe(JSONNode _buf) => new Recipe(_buf);

        public readonly int Id;
        public readonly int[] Inputs;
        public readonly int[] InputQuantities;
        public readonly int Output;
        public readonly float ProcessTime;
        public readonly int RequiredEra;

        public const int __ID__ = 2;
        public override int GetTypeId() => __ID__;

        public void ResolveRef(Tables tables) { }

        public override string ToString()
        {
            return "{ " + "id:" + Id + ",output:" + Output + ",processTime:" + ProcessTime
                + ",requiredEra:" + RequiredEra + " }";
        }
    }
}
