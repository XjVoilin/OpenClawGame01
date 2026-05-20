using cfg;

namespace CozyYard
{
    /// <summary>
    /// UI层配置查询工具，封装常用的配置表名称解析。
    /// </summary>
    public static class CfgHelper
    {
        public static string GetItemName(int itemId)
        {
            var item = CfgTable.Tables?.TbItem.GetOrDefault(itemId);
            return item?.Name ?? $"#{itemId}";
        }

        public static string GetBuildingName(int buildingId)
        {
            var building = CfgTable.Tables?.TbBuilding.GetOrDefault(buildingId);
            return building?.Name ?? $"#{buildingId}";
        }

        public static string GetRecipeName(int recipeId)
        {
            var recipe = CfgTable.Tables?.TbRecipe.GetOrDefault(recipeId);
            return recipe?.Name ?? $"#{recipeId}";
        }

        public static string GetVisitorName(int visitorId)
        {
            var visitor = CfgTable.Tables?.TbVisitor.GetOrDefault(visitorId);
            return visitor?.Name ?? $"#{visitorId}";
        }

        public static string GetMilestoneName(int milestoneId)
        {
            var m = CfgTable.Tables?.TbMilestone.GetOrDefault(milestoneId);
            return m?.Name ?? $"#{milestoneId}";
        }

        public static string GetMilestoneDesc(int milestoneId)
        {
            var m = CfgTable.Tables?.TbMilestone.GetOrDefault(milestoneId);
            return m?.Description ?? "";
        }

        /// <summary>
        /// Format material cost string from comma-separated IDs and quantities.
        /// Example: "木材×20, 石头×10"
        /// </summary>
        public static string FormatMaterials(string materialIds, string materialQtys)
        {
            if (string.IsNullOrEmpty(materialIds)) return "";

            var ids = materialIds.Split(',');
            var qtys = materialQtys.Split(',');
            var parts = new string[ids.Length];

            for (int i = 0; i < ids.Length; i++)
            {
                if (int.TryParse(ids[i].Trim(), out int id))
                {
                    string name = GetItemName(id);
                    string qty = i < qtys.Length ? qtys[i].Trim() : "?";
                    parts[i] = $"{name}×{qty}";
                }
                else
                {
                    parts[i] = "?";
                }
            }

            return string.Join(", ", parts);
        }

        /// <summary>
        /// Format recipe inputs as readable string.
        /// </summary>
        public static string FormatRecipeInputs(int recipeId)
        {
            var recipe = CfgTable.Tables?.TbRecipe.GetOrDefault(recipeId);
            if (recipe == null) return "?";
            return FormatMaterials(recipe.InputItemIds, recipe.InputQuantities);
        }

        /// <summary>
        /// Format recipe output as readable string.
        /// </summary>
        public static string FormatRecipeOutput(int recipeId)
        {
            var recipe = CfgTable.Tables?.TbRecipe.GetOrDefault(recipeId);
            if (recipe == null) return "?";
            return $"{GetItemName(recipe.OutputItemId)}×{recipe.OutputQuantity}";
        }
    }
}
