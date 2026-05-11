namespace IsleWorks.Economy
{
    public readonly struct GoldChangedEvent
    {
        public readonly int NewGold;

        public GoldChangedEvent(int newGold)
        {
            NewGold = newGold;
        }
    }

    public readonly struct ProductSoldEvent
    {
        public readonly int Amount;

        public ProductSoldEvent(int amount)
        {
            Amount = amount;
        }
    }
}
