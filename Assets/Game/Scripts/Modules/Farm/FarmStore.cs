using System.Collections.Generic;

namespace CozyYard
{
    public class FarmStore : SavableStoreBase<FarmData>, IFarmQueries
    {
        protected override string SaveKey => SaveKeys.FarmData;

        public IReadOnlyList<CropInstance> Crops => Data.Crops;

        public CropInstance GetCropAt(int x, int y)
        {
            for (int i = 0; i < Data.Crops.Count; i++)
            {
                if (Data.Crops[i].GridX == x && Data.Crops[i].GridY == y)
                    return Data.Crops[i];
            }
            return null;
        }

        public bool HasCropAt(int x, int y)
        {
            return GetCropAt(x, y) != null;
        }

        public void AddCrop(CropInstance crop)
        {
            Data.Crops.Add(crop);
            MarkDirty();
        }

        public void RemoveCrop(CropInstance crop)
        {
            Data.Crops.Remove(crop);
            MarkDirty();
        }

        public void MarkDirtyExplicit()
        {
            MarkDirty();
        }
    }
}
