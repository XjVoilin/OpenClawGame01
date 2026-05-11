using UnityEngine;

namespace IsleWorks.Grid
{
    public enum Direction : byte
    {
        Up = 0,
        Right = 1,
        Down = 2,
        Left = 3
    }

    public static class DirectionExtensions
    {
        private static readonly Vector2Int[] Offsets =
        {
            Vector2Int.up,      // Up
            Vector2Int.right,   // Right
            Vector2Int.down,    // Down
            Vector2Int.left     // Left
        };

        public static Vector2Int ToVector2Int(this Direction dir) => Offsets[(int)dir];

        public static Direction Opposite(this Direction dir) => (Direction)(((int)dir + 2) % 4);
    }
}
