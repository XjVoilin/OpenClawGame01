using System;

namespace CozyYard
{
    public class AccountStore : SavableStoreBase<AccountData>
    {
        protected override string SaveKey => SaveKeys.AccountData;

        public bool Initialized => Data.Initialized;
        public string UserId => Data.UserId;
        public string DisplayName => Data.DisplayName;
        public int AvatarId => Data.AvatarId;
        public long CreatedAt => Data.CreatedAt;
        public long LastLoginAt => Data.LastLoginAt;
        public int TotalLoginDays => Data.TotalLoginDays;

        public void CreateProfile()
        {
            Data.UserId = Guid.NewGuid().ToString("N");
            Data.CreatedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            Data.LastLoginAt = Data.CreatedAt;
            Data.TotalLoginDays = 1;
            MarkDirty();
        }

        public void SetInitialized()
        {
            Data.Initialized = true;
            MarkDirty();
        }

        public void UpdateLoginInfo()
        {
            Data.LastLoginAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            Data.TotalLoginDays++;
            MarkDirty();
        }

        public void SetDisplayName(string name)
        {
            Data.DisplayName = name;
            MarkDirty();
        }

        public void SetAvatarId(int avatarId)
        {
            Data.AvatarId = avatarId;
            MarkDirty();
        }
    }
}
