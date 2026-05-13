namespace OffTrail.Crafting
{
    public struct CraftingStarted
    {
        public int RecipeId;
    }

    public struct CraftingCompleted
    {
        public int RecipeId;
        public int ResultItemId;
    }
}
