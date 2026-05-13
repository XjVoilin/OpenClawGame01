namespace OffTrail.Survival
{
    public struct SurvivalStatChanged
    {
        public string StatName;
        public float OldValue;
        public float NewValue;
    }

    public struct PlayerDied
    {
    }

    public struct FoodConsumed
    {
        public int ItemId;
        public float HungerRestored;
        public float ThirstRestored;
    }
}
