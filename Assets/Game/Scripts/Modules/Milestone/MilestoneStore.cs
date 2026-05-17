using System.Collections.Generic;
using JulyArch;

namespace SpiritHealer
{
    public class MilestoneProgress
    {
        public int MilestoneId;
        public bool Unlocked;
        public float Progress;
    }

    public class MilestoneData
    {
        public List<MilestoneProgress> Milestones = new();
    }

    public class MilestoneStore : StoreBase<MilestoneData>
    {
        public IReadOnlyList<MilestoneProgress> Milestones => Data.Milestones;
    }
}