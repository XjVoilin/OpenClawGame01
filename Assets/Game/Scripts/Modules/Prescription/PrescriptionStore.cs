using System.Collections.Generic;
using System.Linq;
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
        public int CauseId;
        public List<PrescriptionSlot> Slots = new();
        public float EfficacyScore;
        public bool IsVerified;
    }

    public class HerbKnowledge
    {
        public int HerbConfigId;
        public bool KnowsNature;
        public bool KnowsFlavor;
        public bool KnowsMeridian;
        public bool KnowsToxicity;
    }

    public class PrescriptionData
    {
        public List<PrescriptionSlot> CurrentSlots = new();
        public List<PrescriptionRecord> Records = new();
        public Dictionary<int, HerbKnowledge> KnowledgeMap = new();
        public int NextRecordId = 1;
    }

    public class PrescriptionStore : StoreBase<PrescriptionData>
    {
        public IReadOnlyList<PrescriptionSlot> CurrentSlots => Data.CurrentSlots;
        public IReadOnlyList<PrescriptionRecord> Records => Data.Records;

        public HerbKnowledge GetKnowledge(int herbConfigId) =>
            Data.KnowledgeMap.TryGetValue(herbConfigId, out var k) ? k : null;

        public HerbKnowledge GetOrCreateKnowledge(int herbConfigId)
        {
            if (!Data.KnowledgeMap.TryGetValue(herbConfigId, out var k))
            {
                k = new HerbKnowledge { HerbConfigId = herbConfigId };
                Data.KnowledgeMap[herbConfigId] = k;
            }
            return k;
        }

        public void SetSlot(HerbRole role, int herbId, int quality)
        {
            var slot = Data.CurrentSlots.Find(s => s.Role == role);
            if (slot != null)
            {
                slot.HerbId = herbId;
                slot.Quality = quality;
            }
            else
            {
                Data.CurrentSlots.Add(new PrescriptionSlot { Role = role, HerbId = herbId, Quality = quality });
            }
        }

        public void ClearSlots() => Data.CurrentSlots.Clear();

        public PrescriptionSlot GetSlot(HerbRole role) =>
            Data.CurrentSlots.Find(s => s.Role == role);

        public int AllocateRecordId() => Data.NextRecordId++;

        public void AddRecord(PrescriptionRecord record) => Data.Records.Add(record);

        public PrescriptionRecord GetRecord(int id) => Data.Records.Find(r => r.Id == id);
    }
}
