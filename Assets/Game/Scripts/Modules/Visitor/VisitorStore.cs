using System.Collections.Generic;
using JulyArch;

namespace SpiritHealer
{
    public enum VisitorType { Commoner, Wanderer, SectDisciple, Elder, Mysterious }

    public class VisitorInstance
    {
        public int ConfigId;
        public VisitorType Type;
        public int SymptomId;
        public bool Diagnosed;
        public bool Treated;
        public float TreatmentScore;
    }

    public class VisitorData
    {
        public List<VisitorInstance> WaitingQueue = new();
        public VisitorInstance CurrentVisitor;
        public int TotalTreated;
        public int TotalCured;
    }

    public interface IVisitorQueries : IStoreQueries
    {
        IReadOnlyList<VisitorInstance> WaitingQueue { get; }
        VisitorInstance CurrentVisitor { get; }
        int TotalTreated { get; }
        int TotalCured { get; }
    }

    public class VisitorStore : StoreBase<VisitorData>, IVisitorQueries
    {
        public IReadOnlyList<VisitorInstance> WaitingQueue => Data.WaitingQueue;
        public VisitorInstance CurrentVisitor => Data.CurrentVisitor;
        public int TotalTreated => Data.TotalTreated;
        public int TotalCured => Data.TotalCured;

        public void SetCurrentVisitor(VisitorInstance visitor) => Data.CurrentVisitor = visitor;
        public void AddToQueue(VisitorInstance visitor) => Data.WaitingQueue.Add(visitor);
        public void RemoveFromQueue(VisitorInstance visitor) => Data.WaitingQueue.Remove(visitor);
        public void IncrementTreated() => Data.TotalTreated++;
        public void IncrementCured() => Data.TotalCured++;
    }
}
