using JulyArch;

namespace CozyYard
{
    public interface IGridQueries : IStoreQueries
    {
        int Width { get; }
        int Height { get; }
        GridCellData GetCell(int x, int y);
        bool IsInBounds(int x, int y);
        bool IsCellEmpty(int x, int y);
        bool IsCellBuildable(int x, int y);
        bool CanPlaceAt(int x, int y, int sizeX, int sizeY);
    }
}
