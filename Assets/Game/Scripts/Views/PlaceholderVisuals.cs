using System.Collections.Generic;
using UnityEngine;

namespace IsleWorks.Views
{
    /// <summary>
    /// 运行时生成彩色方块 Sprite 的工具类，用于无美术资源时的占位视觉。
    /// </summary>
    public static class PlaceholderVisuals
    {
        private static readonly Dictionary<Color, Sprite> Cache = new Dictionary<Color, Sprite>();

        public static Sprite GetSprite(Color color)
        {
            if (Cache.TryGetValue(color, out var cached)) return cached;

            var tex = new Texture2D(4, 4);
            var pixels = new Color[16];
            for (int i = 0; i < 16; i++) pixels[i] = color;
            tex.SetPixels(pixels);
            tex.Apply();

            var sprite = Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4);
            Cache[color] = sprite;
            return sprite;
        }

        // 预定义颜色
        // 地块颜色
        public static readonly Color NormalTile = new Color(0.35f, 0.65f, 0.35f);
        public static readonly Color LockedTile = new Color(0.4f, 0.4f, 0.4f);
        public static readonly Color PortTile = new Color(0.9f, 0.8f, 0.2f);
        public static readonly Color WaterTile = new Color(0.2f, 0.4f, 0.7f);
        public static readonly Color MountainTile = new Color(0.5f, 0.45f, 0.4f);
        public static readonly Color ResourceNode = new Color(0.55f, 0.55f, 0.6f);

        // 机器颜色
        public static readonly Color MinerColor = new Color(0.6f, 0.4f, 0.2f);
        public static readonly Color SmelterColor = new Color(0.7f, 0.3f, 0.3f);
        public static readonly Color ConveyorColor = new Color(0.5f, 0.5f, 0.5f);
        public static readonly Color PortBuildingColor = new Color(0.85f, 0.75f, 0.15f);
        public static readonly Color ComboMachineColor = new Color(0.4f, 0.6f, 0.7f);
        public static readonly Color GeneratorColor = new Color(0.3f, 0.7f, 0.3f);
        public static readonly Color WireColor = new Color(0.6f, 0.6f, 0.1f);
        public static readonly Color SorterColor = new Color(0.6f, 0.3f, 0.7f);

        // 资源颜色
        public static readonly Color OreColor = new Color(0.4f, 0.4f, 0.45f);
        public static readonly Color IngotColor = new Color(0.7f, 0.6f, 0.3f);
        public static readonly Color WoodColor = new Color(0.45f, 0.3f, 0.15f);
        public static readonly Color PlankColor = new Color(0.65f, 0.5f, 0.25f);
        public static readonly Color CoalColor = new Color(0.25f, 0.25f, 0.25f);
        public static readonly Color OilColor = new Color(0.15f, 0.15f, 0.2f);
        public static readonly Color PlasticColor = new Color(0.8f, 0.8f, 0.85f);
        public static readonly Color ToolColor = new Color(0.5f, 0.55f, 0.6f);
        public static readonly Color CircuitBoardColor = new Color(0.2f, 0.6f, 0.3f);
        public static readonly Color AutomatonColor = new Color(0.7f, 0.5f, 0.9f);

        public static Color GetMachineColor(int machineTypeId)
        {
            return machineTypeId switch
            {
                1 => MinerColor,
                2 => SmelterColor,
                3 => ConveyorColor,
                4 => PortBuildingColor,
                5 => ComboMachineColor,
                6 => GeneratorColor,
                7 => WireColor,
                8 => SorterColor,
                _ => Color.white
            };
        }

        public static Color GetResourceColor(IsleWorks.Production.ResourceType type)
        {
            return type switch
            {
                IsleWorks.Production.ResourceType.Ore => OreColor,
                IsleWorks.Production.ResourceType.Ingot => IngotColor,
                IsleWorks.Production.ResourceType.Wood => WoodColor,
                IsleWorks.Production.ResourceType.Plank => PlankColor,
                IsleWorks.Production.ResourceType.Coal => CoalColor,
                IsleWorks.Production.ResourceType.Oil => OilColor,
                IsleWorks.Production.ResourceType.Plastic => PlasticColor,
                IsleWorks.Production.ResourceType.Tool => ToolColor,
                IsleWorks.Production.ResourceType.CircuitBoard => CircuitBoardColor,
                IsleWorks.Production.ResourceType.Automaton => AutomatonColor,
                _ => Color.white
            };
        }
    }
}
