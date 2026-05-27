using UnityEngine;

namespace CozyYard
{
    public static class GridUtils
    {
        public const float TileSize = 1f;

        public static Vector2 GridToWorld(int gridX, int gridY)
        {
            return new Vector2(gridX * TileSize, -gridY * TileSize);
        }

        public static Vector2Int WorldToGrid(Vector2 worldPos)
        {
            return new Vector2Int(
                Mathf.RoundToInt(worldPos.x / TileSize),
                Mathf.RoundToInt(-worldPos.y / TileSize)
            );
        }

        public static int GetSortingOrder(int gridX, int gridY, int heightOffset = 0)
        {
            return -gridY * 100 - heightOffset;
        }
    }
}
