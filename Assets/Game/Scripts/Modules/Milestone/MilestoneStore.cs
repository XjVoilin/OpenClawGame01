using System.Collections.Generic;

namespace CozyYard
{
    public class MilestoneStore : SavableStoreBase<MilestoneData>
    {
        protected override string SaveKey => SaveKeys.MilestoneData;

        public IReadOnlyList<MilestoneProgress> Milestones => Data.Milestones;
        public int ExpansionLevel => Data.ExpansionLevel;

        public bool IsMilestoneCompleted(int milestoneId)
        {
            var p = GetProgress(milestoneId);
            return p != null && p.Completed;
        }

        public MilestoneProgress GetProgress(int milestoneId)
        {
            for (int i = 0; i < Data.Milestones.Count; i++)
            {
                if (Data.Milestones[i].MilestoneId == milestoneId) return Data.Milestones[i];
            }
            return null;
        }

        public MilestoneProgress GetOrCreateProgress(int milestoneId)
        {
            var p = GetProgress(milestoneId);
            if (p == null)
            {
                p = new MilestoneProgress { MilestoneId = milestoneId, CurrentCount = 0, Completed = false };
                Data.Milestones.Add(p);
                MarkDirty();
            }
            return p;
        }

        public void IncrementExpansion()
        {
            Data.ExpansionLevel++;
            MarkDirty();
        }

        public void MarkDirtyExplicit() => MarkDirty();
    }
}
