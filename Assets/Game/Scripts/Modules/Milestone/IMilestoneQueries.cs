using System.Collections.Generic;
using JulyArch;

namespace CozyYard
{
    public interface IMilestoneQueries : IStoreQueries
    {
        IReadOnlyList<MilestoneProgress> Milestones { get; }
        int ExpansionLevel { get; }
        bool IsMilestoneCompleted(int milestoneId);
        MilestoneProgress GetProgress(int milestoneId);
    }
}
