using System;
using System.Collections.Generic;
using cfg;
using JulyArch;
using JulyCore;
using JulyCore.Provider.Config;
using OffTrail;
using OffTrail.Inventory;

namespace OffTrail.Knowledge
{
    public sealed class KnowledgeSystem : GameSystemBase, IAppArch
    {
        public new IGameContext GetArchitecture() => AppArch.Context;

        protected override void OnInitialize()
        {
            this.Subscribe<ItemPickedUp>(_ => RefreshCombosFromInventory());
            this.Subscribe<ItemUsed>(_ => RefreshCombosFromInventory());
        }

        public override void OnShutdown()
        {
            this.UnsubscribeAll();
            base.OnShutdown();
        }

        private static bool TryGetTables(out Tables tables)
        {
            var cfg = GF.Resolve<IConfigProvider>();
            return cfg.TryGetTable(out tables);
        }

        private void RefreshCombosFromInventory()
        {
            var inv = this.Query<IInventoryQueries>();
            if (inv != null)
                CheckItemComboTrigger(inv.GetAllUniqueItemIds());
        }

        public void CheckItemComboTrigger(List<int> currentInventoryItemIds)
        {
            if (currentInventoryItemIds == null || currentInventoryItemIds.Count == 0 || !TryGetTables(out var tables))
                return;

            var hold = new HashSet<int>(currentInventoryItemIds);
            foreach (var row in tables.TbKnowledge.DataList)
            {
                if (row.TriggerType != 0 || string.IsNullOrEmpty(row.TriggerCondition))
                    continue;

                var required = ParseItemComboRequirements(row.TriggerCondition);
                if (required.Count == 0)
                    continue;

                var all = true;
                for (var i = 0; i < required.Count; i++)
                {
                    if (hold.Contains(required[i]))
                        continue;
                    all = false;
                    break;
                }

                if (all)
                    TryUnlockKnowledge(row.Id);
            }
        }

        private static List<int> ParseItemComboRequirements(string triggerCondition)
        {
            var list = new List<int>();
            var parts = triggerCondition.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var part in parts)
            {
                if (int.TryParse(part.Trim(), out var id))
                    list.Add(id);
            }

            return list;
        }

        public void TryUnlockKnowledge(int knowledgeId)
        {
            if (!TryGetTables(out var tables))
                return;

            var def = tables.TbKnowledge.GetOrDefault(knowledgeId);
            if (def == null)
                return;

            var kq = this.Query<IKnowledgeQueries>();
            if (kq != null && kq.IsKnowledgeUnlocked(knowledgeId))
                return;

            List<(int RecipeId, string RecipeName)> newRecipes = null;

            this.Mutate<KnowledgeStore>(store =>
            {
                if (store.IsKnowledgeUnlocked(knowledgeId))
                    return;

                store.UnlockKnowledge(knowledgeId);
                this.Publish(new KnowledgeDiscovered { KnowledgeId = def.Id, KnowledgeName = def.Name });

                if (string.IsNullOrEmpty(def.UnlockRecipeIds))
                    return;

                var spans = def.UnlockRecipeIds.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var part in spans)
                {
                    if (!int.TryParse(part.Trim(), out var recipeId))
                        continue;

                    var recipeDef = tables.TbRecipe.GetOrDefault(recipeId);
                    if (recipeDef == null || store.IsRecipeUnlocked(recipeId))
                        continue;

                    store.UnlockRecipe(recipeDef.Id);
                    newRecipes ??= new List<(int, string)>();
                    newRecipes.Add((recipeDef.Id, recipeDef.Name));
                }
            });

            if (newRecipes == null)
                return;

            foreach (var r in newRecipes)
                this.Publish(new RecipeUnlocked { RecipeId = r.RecipeId, RecipeName = r.RecipeName });
        }
    }
}
