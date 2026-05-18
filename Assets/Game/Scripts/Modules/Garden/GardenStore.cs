using System.Collections.Generic;
using JulyArch;

namespace SpiritHealer
{
    public enum SoilType { Normal, Spirit, FertileSpirit, Immortal }
    public enum FiveElement { None, Metal, Wood, Water, Fire, Earth }
    public enum Moisture { Dry, Normal, Wet }
    public enum PlantStage { Empty, Seed, Sprout, Growing, Mature, Withered }

    public class GardenPlot
    {
        public int X;
        public int Y;
        public SoilType Soil;
        public float SpiritLevel;
        public FiveElement Element;
        public Moisture Moisture;
        public int HerbConfigId;
        public PlantStage Stage;
        public float GrowthProgress;
        public int Quality;
    }

    public class GardenData
    {
        public int GridSize = 3;
        public List<GardenPlot> Plots = new();
        public bool HasSpiritArray;
        public bool HasSpiritSpring;
        public bool HasGreenhouse;
    }
    
    public class GardenStore : StoreBase<GardenData>
    {
        public int GridSize => Data.GridSize;
        public IReadOnlyList<GardenPlot> Plots => Data.Plots;
        public bool HasSpiritArray => Data.HasSpiritArray;
        public bool HasSpiritSpring => Data.HasSpiritSpring;
        public bool HasGreenhouse => Data.HasGreenhouse;

        public GardenPlot GetPlot(int x, int y) =>
            Data.Plots.Find(p => p.X == x && p.Y == y);

        public void SetPlotStage(int x, int y, PlantStage stage)
        {
            var plot = GetPlot(x, y);
            if (plot != null) plot.Stage = stage;
        }

        public void ExpandGrid(int newSize) => Data.GridSize = newSize;

        public void AddPlot(GardenPlot plot) => Data.Plots.Add(plot);
    }
}
