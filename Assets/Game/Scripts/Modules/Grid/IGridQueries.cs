using JulyArch;

namespace IsleWorks.Grid
{
    public interface IGridQueries : IStoreQueries
    {
        int Width { get; }
        int Height { get; }
        TileType GetTile(int x, int y);
        int GetBuilding(int x, int y);
    }
}
