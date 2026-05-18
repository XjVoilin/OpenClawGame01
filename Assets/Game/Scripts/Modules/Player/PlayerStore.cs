using JulyArch;

namespace SpiritHealer
{
    public class PlayerData
    {
        /// <summary>
        /// 名声值
        /// </summary>
        public int Reputation;
        /// <summary>
        /// 碎银
        /// </summary>
        public int Coins;
    }

    public class PlayerStore : StoreBase<PlayerData>
    {
        public int Reputation => Data.Reputation;
        public int Coins => Data.Coins;

        public void AddReputation(int amount) => Data.Reputation += amount;
        public void AddCoins(int amount) => Data.Coins += amount;
    }
}
