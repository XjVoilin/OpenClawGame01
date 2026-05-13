using System.Collections.Generic;
using JulyArch;

namespace OffTrail.Knowledge
{
    public sealed class KnowledgeData
    {
        public readonly HashSet<int> UnlockedKnowledgeIds = new();
        public readonly HashSet<int> UnlockedRecipeIds = new();
        public readonly Dictionary<int, bool> TriedItems = new();
    }

    public interface IKnowledgeQueries : IStoreQueries
    {
        bool IsKnowledgeUnlocked(int knowledgeId);
        bool IsRecipeUnlocked(int recipeId);
        bool HasTriedItem(int itemId);
    }

    public sealed class KnowledgeStore : StoreBase<KnowledgeData>, IKnowledgeQueries
    {
        public bool IsKnowledgeUnlocked(int knowledgeId) => Data.UnlockedKnowledgeIds.Contains(knowledgeId);

        public bool IsRecipeUnlocked(int recipeId) => Data.UnlockedRecipeIds.Contains(recipeId);

        public bool HasTriedItem(int itemId) => Data.TriedItems.ContainsKey(itemId);

        public void UnlockKnowledge(int knowledgeId) => Data.UnlockedKnowledgeIds.Add(knowledgeId);

        public void UnlockRecipe(int recipeId) => Data.UnlockedRecipeIds.Add(recipeId);

        public void RecordTriedItem(int itemId, bool isSafe) => Data.TriedItems[itemId] = isSafe;
    }
}
