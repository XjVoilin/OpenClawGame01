#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace CozyYard.Editor
{
    /// <summary>
    /// 生成等轴测瓦片 Sprite 资源（菱形 png），保存到 Assets/Game/Arts/Sprites/Tiles/
    /// 后续美术替换时直接覆盖这些 png 即可。
    /// </summary>
    public static class TileSpriteGenerator
    {
        private const string OutputDir = "Assets/Game/Res/Sprites/Tiles";
        private const int TexWidth = 64;
        private const int TexHeight = 32;
        private const int PPU = 64;

        private static readonly TileDef[] Tiles =
        {
            new("Tile_Empty", new Color(0.6f, 0.85f, 0.45f)),
            new("Tile_Obstacle", new Color(0.4f, 0.3f, 0.25f)),
            new("Tile_Soil", new Color(0.55f, 0.35f, 0.2f)),
            new("Tile_Building", new Color(0.85f, 0.65f, 0.3f)),
            new("Tile_Highlight", Color.white),
            new("Tile_Unexplored", new Color(0.25f, 0.25f, 0.28f)),
        };

        [MenuItem("CozyYard/生成瓦片 Sprite 资源", false, 210)]
        public static void Generate()
        {
            if (!Directory.Exists(OutputDir))
            {
                Directory.CreateDirectory(OutputDir);
                AssetDatabase.Refresh();
            }

            foreach (var tile in Tiles)
            {
                var tex = CreateDiamondTexture(tile.Color);
                var pngBytes = tex.EncodeToPNG();
                Object.DestroyImmediate(tex);

                var path = $"{OutputDir}/{tile.Name}.png";
                File.WriteAllBytes(path, pngBytes);
            }

            AssetDatabase.Refresh();

            foreach (var tile in Tiles)
            {
                var path = $"{OutputDir}/{tile.Name}.png";
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer != null)
                {
                    importer.textureType = TextureImporterType.Sprite;
                    importer.spritePixelsPerUnit = PPU;
                    importer.filterMode = FilterMode.Point;
                    importer.textureCompression = TextureImporterCompression.Uncompressed;
                    importer.mipmapEnabled = false;
                    importer.SaveAndReimport();
                }
            }

            Debug.Log($"[TileSpriteGenerator] 已生成 {Tiles.Length} 张瓦片 Sprite → {OutputDir}/");
        }

        [MenuItem("CozyYard/生成瓦片 Sprite 资源", true)]
        private static bool GenerateValidate() => !Application.isPlaying;

        private static Texture2D CreateDiamondTexture(Color color)
        {
            var tex = new Texture2D(TexWidth, TexHeight, TextureFormat.RGBA32, false);
            var pixels = new Color[TexWidth * TexHeight];

            float halfW = TexWidth * 0.5f;
            float halfH = TexHeight * 0.5f;

            for (int py = 0; py < TexHeight; py++)
            {
                for (int px = 0; px < TexWidth; px++)
                {
                    float nx = Mathf.Abs(px - halfW + 0.5f) / halfW;
                    float ny = Mathf.Abs(py - halfH + 0.5f) / halfH;
                    pixels[py * TexWidth + px] = (nx + ny <= 1f) ? color : Color.clear;
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            tex.filterMode = FilterMode.Point;
            return tex;
        }

        private readonly struct TileDef
        {
            public readonly string Name;
            public readonly Color Color;

            public TileDef(string name, Color color)
            {
                Name = name;
                Color = color;
            }
        }
    }
}
#endif
