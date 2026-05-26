using System;
using JulyCore.Data.Save;

namespace CozyYard
{
    [Serializable]
    public class AccountData : ISaveData
    {
        public string UserId;
        public string DisplayName;
        public int AvatarId;
        public long CreatedAt;
        public long LastLoginAt;
        public int TotalLoginDays;
        public bool Initialized;

        public SaveImportance Importance => SaveImportance.Critical;
    }
}
