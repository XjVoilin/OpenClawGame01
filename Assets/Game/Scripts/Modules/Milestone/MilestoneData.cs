using System;
using System.Collections.Generic;
using JulyCore.Data.Save;

namespace CozyYard
{
    [Serializable]
    public class MilestoneProgress
    {
        public int MilestoneId;
        public int CurrentCount;
        public bool Completed;
    }

    [Serializable]
    public class MilestoneData : ISaveData
    {
        public List<MilestoneProgress> Milestones = new();
        public int ExpansionLevel;

        public SaveImportance Importance => SaveImportance.Normal;
    }
}
