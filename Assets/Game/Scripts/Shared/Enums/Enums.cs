namespace CozyYard
{
    public enum Season
    {
        Spring = 0,
        Summer = 1,
        Autumn = 2,
        Winter = 3
    }

    public enum TimePhase
    {
        Dawn,
        Morning,
        Noon,
        Afternoon,
        Evening,
        Night
    }

    public enum CellState
    {
        Unexplored,
        Obstacle,
        Empty,
        Soil,
        Water,
        Paved
    }

    public enum BuildingCategory
    {
        House,
        Production,
        Livestock,
        Functional,
        Decoration
    }

    public enum AnimalType
    {
        Poultry,
        Aquatic,
        Pet
    }

    public enum ItemType
    {
        Material,
        Seed,
        Product,
        Tool
    }
}
