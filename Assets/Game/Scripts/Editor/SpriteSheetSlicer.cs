#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace CozyYard.Editor
{
    public static class SpriteSheetSlicer
    {
        private const int MinSpriteSize = 4;

        [MenuItem("Assets/CozyYard/切割 Spritesheet 为独立 PNG", false, 100)]
        public static void SliceSelected()
        {
            var tex = Selection.activeObject as Texture2D;
            if (tex == null)
            {
                EditorUtility.DisplayDialog("错误", "请先在 Project 窗口中选中一张 Texture2D", "确定");
                return;
            }

            var srcPath = AssetDatabase.GetAssetPath(tex);
            var srcDir = Path.GetDirectoryName(srcPath);
            var srcName = Path.GetFileNameWithoutExtension(srcPath);
            var outputDir = $"{srcDir}/{srcName}_sliced";

            if (!Directory.Exists(outputDir))
                Directory.CreateDirectory(outputDir);

            var importer = AssetImporter.GetAtPath(srcPath) as TextureImporter;
            if (importer == null) return;

            bool wasReadable = importer.isReadable;
            if (!wasReadable)
            {
                importer.isReadable = true;
                importer.SaveAndReimport();
            }

            tex = AssetDatabase.LoadAssetAtPath<Texture2D>(srcPath);
            var rects = DetectSpriteRects(tex);

            rects.Sort((a, b) =>
            {
                int rowA = a.y / 32;
                int rowB = b.y / 32;
                if (rowA != rowB) return rowB.CompareTo(rowA);
                return a.x.CompareTo(b.x);
            });

            int exported = 0;
            for (int i = 0; i < rects.Count; i++)
            {
                var rect = rects[i];
                var cropped = new Texture2D(rect.width, rect.height, TextureFormat.RGBA32, false);
                var pixels = tex.GetPixels(rect.x, rect.y, rect.width, rect.height);
                cropped.SetPixels(pixels);
                cropped.Apply();

                var pngBytes = cropped.EncodeToPNG();
                Object.DestroyImmediate(cropped);

                var fileName = $"{srcName}_{i:D3}.png";
                File.WriteAllBytes($"{outputDir}/{fileName}", pngBytes);
                exported++;
            }

            if (!wasReadable)
            {
                importer.isReadable = false;
                importer.SaveAndReimport();
            }

            AssetDatabase.Refresh();

            ConfigureExportedSprites(outputDir, exported);

            Debug.Log($"[SpriteSheetSlicer] 从 {srcName} 中切出 {exported} 个 sprite → {outputDir}/");
            EditorUtility.DisplayDialog("完成", $"已导出 {exported} 个 sprite 到:\n{outputDir}/", "确定");
        }

        [MenuItem("Assets/CozyYard/切割 Spritesheet 为独立 PNG", true)]
        private static bool SliceSelectedValidate()
        {
            return Selection.activeObject is Texture2D;
        }

        private static List<RectInt> DetectSpriteRects(Texture2D tex)
        {
            int w = tex.width;
            int h = tex.height;
            var pixels = tex.GetPixels32();
            var visited = new bool[w * h];
            var rects = new List<RectInt>();

            for (int y = h - 1; y >= 0; y--)
            {
                for (int x = 0; x < w; x++)
                {
                    int idx = y * w + x;
                    if (visited[idx] || pixels[idx].a == 0) continue;

                    var bounds = FloodFillBounds(pixels, visited, w, h, x, y);

                    if (bounds.width >= MinSpriteSize && bounds.height >= MinSpriteSize)
                        rects.Add(bounds);
                }
            }

            return MergeOverlappingRects(rects);
        }

        private static RectInt FloodFillBounds(Color32[] pixels, bool[] visited, int w, int h, int startX, int startY)
        {
            int minX = startX, maxX = startX, minY = startY, maxY = startY;
            var stack = new Stack<(int x, int y)>();
            stack.Push((startX, startY));
            visited[startY * w + startX] = true;

            while (stack.Count > 0)
            {
                var (cx, cy) = stack.Pop();

                if (cx < minX) minX = cx;
                if (cx > maxX) maxX = cx;
                if (cy < minY) minY = cy;
                if (cy > maxY) maxY = cy;

                for (int dy = -1; dy <= 1; dy++)
                {
                    for (int dx = -1; dx <= 1; dx++)
                    {
                        if (dx == 0 && dy == 0) continue;
                        int nx = cx + dx, ny = cy + dy;
                        if (nx < 0 || nx >= w || ny < 0 || ny >= h) continue;
                        int ni = ny * w + nx;
                        if (visited[ni] || pixels[ni].a == 0) continue;
                        visited[ni] = true;
                        stack.Push((nx, ny));
                    }
                }
            }

            return new RectInt(minX, minY, maxX - minX + 1, maxY - minY + 1);
        }

        /// <summary>
        /// 合并间距小于 gap 的相邻矩形（处理同一 sprite 内因透明像素断裂产生的多个区域）
        /// </summary>
        private static List<RectInt> MergeOverlappingRects(List<RectInt> rects, int gap = 2)
        {
            bool merged = true;
            while (merged)
            {
                merged = false;
                for (int i = 0; i < rects.Count; i++)
                {
                    for (int j = i + 1; j < rects.Count; j++)
                    {
                        if (RectsClose(rects[i], rects[j], gap))
                        {
                            rects[i] = Union(rects[i], rects[j]);
                            rects.RemoveAt(j);
                            merged = true;
                            j--;
                        }
                    }
                }
            }
            return rects;
        }

        private static bool RectsClose(RectInt a, RectInt b, int gap)
        {
            return a.xMin - gap <= b.xMax && b.xMin - gap <= a.xMax &&
                   a.yMin - gap <= b.yMax && b.yMin - gap <= a.yMax;
        }

        private static RectInt Union(RectInt a, RectInt b)
        {
            int xMin = Mathf.Min(a.xMin, b.xMin);
            int yMin = Mathf.Min(a.yMin, b.yMin);
            int xMax = Mathf.Max(a.xMax, b.xMax);
            int yMax = Mathf.Max(a.yMax, b.yMax);
            return new RectInt(xMin, yMin, xMax - xMin, yMax - yMin);
        }

        private static void ConfigureExportedSprites(string dir, int count)
        {
            var guids = AssetDatabase.FindAssets("t:Texture2D", new[] { dir });
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (AssetImporter.GetAtPath(path) is not TextureImporter importer) continue;

                bool changed = false;
                if (importer.textureType != TextureImporterType.Sprite)
                { importer.textureType = TextureImporterType.Sprite; changed = true; }
                if (importer.spritePixelsPerUnit != 16)
                { importer.spritePixelsPerUnit = 16; changed = true; }
                if (importer.filterMode != FilterMode.Point)
                { importer.filterMode = FilterMode.Point; changed = true; }
                if (importer.textureCompression != TextureImporterCompression.Uncompressed)
                { importer.textureCompression = TextureImporterCompression.Uncompressed; changed = true; }
                if (importer.mipmapEnabled)
                { importer.mipmapEnabled = false; changed = true; }

                if (changed)
                    importer.SaveAndReimport();
            }
        }
    }
}
#endif
