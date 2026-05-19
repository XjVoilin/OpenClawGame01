using System.Collections.Generic;
using JulyArch;

namespace SpiritHealer
{
    public enum VisitorType { Commoner, Wanderer, SectDisciple, Elder, Mysterious }

    public class VisitorInstance
    {
        public int TemplateId;
        public string Name;
        public VisitorType Type;
        public int CauseId;
        public int TimeCost;
        public int BaseReputation;
        public int BaseCoin;
        public bool Treated;
        public float TreatmentScore;
        public HashSet<int> RevealedSymptomIds = new();
    }

    public class VisitorData
    {
        public List<VisitorInstance> WaitingQueue = new();
        public VisitorInstance CurrentVisitor;
        public int TotalTreated;
        public int TotalCured;
    }

    public class VisitorStore : StoreBase<VisitorData>
    {
        public IReadOnlyList<VisitorInstance> WaitingQueue => Data.WaitingQueue;
        public VisitorInstance CurrentVisitor => Data.CurrentVisitor;
        public int TotalTreated => Data.TotalTreated;
        public int TotalCured => Data.TotalCured;

        public void SetCurrentVisitor(VisitorInstance visitor) => Data.CurrentVisitor = visitor;
        public void AddToQueue(VisitorInstance visitor) => Data.WaitingQueue.Add(visitor);
        public void RemoveFromQueue(VisitorInstance visitor) => Data.WaitingQueue.Remove(visitor);
        public void ClearQueue() => Data.WaitingQueue.Clear();
        public void IncrementTreated() => Data.TotalTreated++;
        public void IncrementCured() => Data.TotalCured++;
    }
}
