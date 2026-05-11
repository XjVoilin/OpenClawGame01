using Luban;
using SimpleJSON;
using UnityEngine;

namespace cfg
{
    public sealed partial class Machine : Luban.BeanBase
    {
        public Machine(JSONNode _buf)
        {
            { if(!_buf["id"].IsNumber) throw new SerializationException(); Id = _buf["id"]; }
            { if(!_buf["name"].IsString) throw new SerializationException(); Name = _buf["name"]; }
            { if(!_buf["sizeX"].IsNumber) throw new SerializationException(); SizeX = _buf["sizeX"]; }
            { if(!_buf["sizeY"].IsNumber) throw new SerializationException(); SizeY = _buf["sizeY"]; }
            { if(!_buf["recipeId"].IsNumber) throw new SerializationException(); RecipeId = _buf["recipeId"]; }
            { if(!_buf["cost"].IsNumber) throw new SerializationException(); Cost = _buf["cost"]; }
            { if(!_buf["refundRatio"].IsNumber) throw new SerializationException(); RefundRatio = _buf["refundRatio"]; }
            { if(!_buf["requiredEra"].IsNumber) throw new SerializationException(); RequiredEra = _buf["requiredEra"]; }
            { if(!_buf["inputSlotSize"].IsNumber) throw new SerializationException(); InputSlotSize = _buf["inputSlotSize"]; }
        }

        public static Machine DeserializeMachine(JSONNode _buf) => new Machine(_buf);

        public readonly int Id;
        public readonly string Name;
        public readonly int SizeX;
        public readonly int SizeY;
        public readonly int RecipeId;
        public readonly int Cost;
        public readonly float RefundRatio;
        public readonly int RequiredEra;
        public readonly int InputSlotSize;

        public Vector2Int Size => new Vector2Int(SizeX, SizeY);

        public const int __ID__ = 1;
        public override int GetTypeId() => __ID__;

        public void ResolveRef(Tables tables) { }

        public override string ToString()
        {
            return "{ " + "id:" + Id + ",name:" + Name + ",sizeX:" + SizeX + ",sizeY:" + SizeY
                + ",recipeId:" + RecipeId + ",cost:" + Cost + ",refundRatio:" + RefundRatio
                + ",requiredEra:" + RequiredEra + ",inputSlotSize:" + InputSlotSize + " }";
        }
    }
}
