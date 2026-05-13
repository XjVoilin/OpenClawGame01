using System;
using System.Collections.Generic;
using cfg;
using JulyArch;
using JulyCore;
using JulyCore.Provider.Config;
using OffTrail;
using OffTrail.Inventory;
using OffTrail.Knowledge;
using OffTrail.World;

namespace OffTrail.Crafting
{
    public sealed class CraftingSystem : GameSystemBase, IAppArch
    {
        public new IGameContext GetArchitecture() => AppArch.Context;

        public List<int> GetAvailableRecipes()
        {
            var list = new List<int>();
            var cfgProv = GF.Resolve<IConfigProvider>();
            if (!cfgProv.TryGetTable(out Tables tb))
                return list;

            foreach (var r in tb.TbRecipe.DataList)
            {
                if (CanCraft(tb, r.Id))
                    list.Add(r.Id);
            }

            return list;
        }

        public bool CanCraft(int recipeId)
        {
            var cfgProv = GF.Resolve<IConfigProvider>();
            if (!cfgProv.TryGetTable(out Tables tb))
                return false;

            return CanCraft(tb, recipeId);
        }

        public bool Craft(int recipeId)
        {
            var cfgProv = GF.Resolve<IConfigProvider>();
            if (!cfgProv.TryGetTable(out Tables tb))
                return false;

            if (!CanCraft(tb, recipeId))
                return false;

            var def = tb.TbRecipe.GetOrDefault(recipeId);
            if (def == null)
                return false;

            var materialsOk = ResolveMaterials(def.Materials, out var needs);
            if (!materialsOk)
                return false;

            this.Publish(new CraftingStarted { RecipeId = def.Id });

            foreach (var m in needs)
                this.Mutate<InventoryStore>(inv => inv.RemoveItem(m.ItemId, m.Count));

            this.Mutate<InventoryStore>(inv => inv.AddItem(def.ResultItemId, def.ResultCount));

            this.Publish(new CraftingCompleted { RecipeId = def.Id, ResultItemId = def.ResultItemId });
            return true;
        }

        private bool CanCraft(Tables tb, int recipeId)
        {
            var inv = this.Query<IInventoryQueries>();
            var know = this.Query<IKnowledgeQueries>();
            var world = this.Query<IWorldQueries>();
            var def = tb.TbRecipe.GetOrDefault(recipeId);
            if (inv == null || know == null || world == null || def == null)
                return false;

            if (!know.IsRecipeUnlocked(recipeId))
                return false;

            if (def.RequiredKnowledgeId != 0 && !know.IsKnowledgeUnlocked(def.RequiredKnowledgeId))
                return false;

            if (!RecipeStationReady(tb, world, def.StationType))
                return false;

            if (!ResolveMaterials(def.Materials, out var needs))
                return false;

            foreach (var m in needs)
            {
                if (!inv.HasItems(m.ItemId, m.Count))
                    return false;
            }

            return true;
        }

        private static bool RecipeStationReady(Tables tb, IWorldQueries world, int recipeStationType)
        {
            if (recipeStationType == 0)
                return true;

            foreach (var row in tb.TbCraftStation.DataList)
            {
                if (row.StationType != recipeStationType)
                    continue;

                if (world.IsStationBuilt(row.Id))
                    return true;
            }

            return false;
        }

        private static bool ResolveMaterials(string materialsCsv, out List<(int ItemId, int Count)> needs)
        {
            needs = new List<(int, int)>();
            if (string.IsNullOrWhiteSpace(materialsCsv))
                return true;

            var parts = materialsCsv.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var part in parts)
            {
                var seg = part.Split(':');
                if (seg.Length != 2 ||
                    !int.TryParse(seg[0].Trim(), out var itemId) ||
                    !int.TryParse(seg[1].Trim(), out var count))
                {
                    return false;
                }

                if (itemId == 0 || count <= 0)
                    return false;

                needs.Add((itemId, count));
            }

            return true;
        }
    }
}
