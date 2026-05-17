using System.Collections.Generic;
using JulyArch;

namespace SpiritHealer
{
    public enum HerbRole { Jun, Chen, Zuo, Shi }

    public class PrescriptionSlot
    {
        public HerbRole Role;
        public int HerbId;
        public int Quality;
    }

    public class PrescriptionRecord
    {
        public int Id;
        public List<PrescriptionSlot> Slots = new();
        public float EfficacyScore;
        public bool IsVerified;
        public string Note;
    }

    public class HerbKnowledge
    {
        public int HerbConfigId;
        public bool KnowsNature;
        public bool KnowsFlavor;
        public bool KnowsMeridian;
        public bool KnowsToxicity;
        public bool KnowsSpecial;
    }

    public class PrescriptionData
    {
        public List<PrescriptionSlot> CurrentSlots = new();
        public List<PrescriptionRecord> Records = new();
        public Dictionary<int, HerbKnowledge> KnowledgeMap = new();
    }

    public interface IPrescriptionQueries : IStoreQueries
    {
        IReadOnlyList<PrescriptionSlot> CurrentSlots { get; }
        IReadOnlyList<PrescriptionRecord> Records { get; }
        HerbKnowledge GetKnowledge(int herbConfigId);
    }

    public class PrescriptionStore : StoreBase<PrescriptionData>, IPrescriptionQueries
    {
        public IReadOnlyList<PrescriptionSlot> CurrentSlots => Data.CurrentSlots;
        public IReadOnlyList<PrescriptionRecord> Records => Data.Records;

        public HerbKnowledge GetKnowledge(int herbConfigId) =>
            Data.KnowledgeMap.TryGetValue(herbConfigId, out var k) ? k : null;

        public void SetSlot(HerbRole role, int herbId, int quality)
        {
            var slot = Data.CurrentSlots.Find(s => s.Role == role);
            if (slot != null)
            {
                slot.HerbId = herbId;
                slot.Quality = quality;
            }
        }

        public void AddRecord(PrescriptionRecord record) => Data.Records.Add(record);

        public void RevealKnowledge(int herbConfigId, HerbKnowledge knowledge) =>
            Data.KnowledgeMap[herbConfigId] = knowledge;
    }
}
