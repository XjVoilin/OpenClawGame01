using UnityEngine;

namespace CozyYard
{
    public static class IsometricUtils
    {
        public const float TileWidth = 1f;
        public const float TileHeight = 0.5f;

        public static Vector2 GridToWorld(int gridX, int gridY)
        {
            float worldX = (gridX - gridY) * TileWidth * 0.5f;
            float worldY = (gridX + gridY) * TileHeight * 0.5f;
            return new Vector2(worldX, -worldY);
        }

        public static Vector2Int WorldToGrid(Vector2 worldPos)
        {
            float invX = worldPos.x / (TileWidth * 0.5f);
            float invY = -worldPos.y / (TileHeight * 0.5f);

            float gridX = (invX + invY) * 0.5f;
            float gridY = (invY - invX) * 0.5f;

            return new Vector2Int(Mathf.RoundToInt(gridX), Mathf.RoundToInt(gridY));
        }

        public static int GetSortingOrder(int gridX, int gridY, int heightOffset = 0)
        {
            return -(gridX + gridY) * 100 - heightOffset;
        }
    }
}
