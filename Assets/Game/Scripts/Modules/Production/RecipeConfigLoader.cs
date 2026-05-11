using cfg;
using JulyCore;
using JulyCore.Provider.Config;

namespace IsleWorks.Production
{
    /// <summary>
    /// 配方配置加载器，从 Luban 配表读取配方数据。
    /// </summary>
    public static class RecipeConfigLoader
    {
        private static TbRecipe _table;

        public static void LoadConfigs()
        {
            if (GF.TryResolve<IConfigProvider>(out var provider) && provider.TryGetTable(out TbRecipe table))
            {
                _table = table;
                GF.Log($"Recipe configs loaded: {table.DataList.Count} recipes");
            }
            else
            {
                GF.LogError("Failed to load recipe config table");
            }
        }

        public static RecipeConfig GetRecipe(int recipeId)
        {
            var row = _table?.GetOrDefault(recipeId);
            if (row == null) return null;

            var inputs = new ResourceType[row.Inputs.Length];
            for (int i = 0; i < row.Inputs.Length; i++)
                inputs[i] = (ResourceType)row.Inputs[i];

            return new RecipeConfig(row.Id, inputs, row.InputQuantities, (ResourceType)row.Output, row.ProcessTime);
        }
    }

    /// <summary>
    /// 单个配方配置。
    /// </summary>
    public class RecipeConfig
    {
        public int Id { get; }
        public ResourceType[] Inputs { get; }
        public int[] InputQuantities { get; }
        public ResourceType Output { get; }
        public float ProcessTime { get; }

        public RecipeConfig(int id, ResourceType[] inputs, int[] inputQuantities, ResourceType output, float processTime)
        {
            Id = id;
            Inputs = inputs;
            InputQuantities = inputQuantities;
            Output = output;
            ProcessTime = processTime;
        }
    }
}
