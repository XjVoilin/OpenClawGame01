using Luban;
using SimpleJSON;

namespace cfg
{
    public sealed partial class Milestone : Luban.BeanBase
    {
        public Milestone(JSONNode _buf)
        {
            { if(!_buf["id"].IsNumber) throw new SerializationException(); Id = _buf["id"]; }
            { if(!_buf["requiredValue"].IsNumber) throw new SerializationException(); RequiredValue = _buf["requiredValue"]; }
            { if(!_buf["unlockEra"].IsNumber) throw new SerializationException(); UnlockEra = _buf["unlockEra"]; }
            {
                var _machinesNode = _buf["unlockMachines"];
                UnlockMachines = new int[_machinesNode.Count];
                for (int i = 0; i < _machinesNode.Count; i++) UnlockMachines[i] = _machinesNode[i];
            }
            {
                var _recipesNode = _buf["unlockRecipes"];
                UnlockRecipes = new int[_recipesNode.Count];
                for (int i = 0; i < _recipesNode.Count; i++) UnlockRecipes[i] = _recipesNode[i];
            }
        }

        public static Milestone DeserializeMilestone(JSONNode _buf) => new Milestone(_buf);

        public readonly int Id;
        public readonly int RequiredValue;
        public readonly int UnlockEra;
        public readonly int[] UnlockMachines;
        public readonly int[] UnlockRecipes;

        public const int __ID__ = 4;
        public override int GetTypeId() => __ID__;

        public void ResolveRef(Tables tables) { }

        public override string ToString()
        {
            return "{ " + "id:" + Id + ",requiredValue:" + RequiredValue + ",unlockEra:" + UnlockEra + " }";
        }
    }
}
