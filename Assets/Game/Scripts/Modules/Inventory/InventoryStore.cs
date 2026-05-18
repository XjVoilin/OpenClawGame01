using System.Collections.Generic;
using JulyArch;

namespace SpiritHealer
{
    public class HerbItem
    {
        public int ConfigId;
        public int Quality;
        public int Count;
    }

    public class SeedItem
    {
        public int ConfigId;
        public int Count;
    }

    public class InventoryData
    {
        public List<HerbItem> Herbs = new();
        public List<SeedItem> Seeds = new();
    }

    public class InventoryStore : StoreBase<InventoryData>
    {
        public IReadOnlyList<HerbItem> Herbs => Data.Herbs;
        public IReadOnlyList<SeedItem> Seeds => Data.Seeds;

        public int GetHerbCount(int configId, int quality)
        {
            var item = Data.Herbs.Find(h => h.ConfigId == configId && h.Quality == quality);
            return item?.Count ?? 0;
        }

        public int GetSeedCount(int configId)
        {
            var item = Data.Seeds.Find(s => s.ConfigId == configId);
            return item?.Count ?? 0;
        }

        public void AddHerb(int configId, int quality, int count)
        {
            var item = Data.Herbs.Find(h => h.ConfigId == configId && h.Quality == quality);
            if (item != null)
                item.Count += count;
            else
                Data.Herbs.Add(new HerbItem { ConfigId = configId, Quality = quality, Count = count });
        }

        public void RemoveHerb(int configId, int quality, int count)
        {
            var item = Data.Herbs.Find(h => h.ConfigId == configId && h.Quality == quality);
            if (item != null) item.Count = System.Math.Max(0, item.Count - count);
        }

        public void AddSeed(int configId, int count)
        {
            var item = Data.Seeds.Find(s => s.ConfigId == configId);
            if (item != null)
                item.Count += count;
            else
                Data.Seeds.Add(new SeedItem { ConfigId = configId, Count = count });
        }

        public void RemoveSeed(int configId, int count)
        {
            var item = Data.Seeds.Find(s => s.ConfigId == configId);
            if (item != null) item.Count = System.Math.Max(0, item.Count - count);
        }
    }
}
