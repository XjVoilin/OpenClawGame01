#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace CozyYard.Editor
{
    public static class SpriteImportTool
    {
        [MenuItem("CozyYard/配置 SproutLands 导入设置", false, 220)]
        public static void ConfigureSproutLandsImport()
        {
            ConfigureFolder("Assets/Game/Res/Sprites/World", 16);
            ConfigureFolder("Assets/Game/Res/Sprites/UI", 16);
            ConfigureFolder("Assets/Game/Arts/SproutLands", 16);
            ConfigureUISliceBorders();
        }

        private static void ConfigureFolder(string folder, int ppu)
        {
            var guids = AssetDatabase.FindAssets("t:Texture2D", new[] { folder });
            int count = 0;
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (AssetImporter.GetAtPath(path) is not TextureImporter importer) continue;

                bool changed = false;

                if (importer.textureType != TextureImporterType.Sprite)
                {
                    importer.textureType = TextureImporterType.Sprite;
                    changed = true;
                }
                if (importer.spritePixelsPerUnit != ppu)
                {
                    importer.spritePixelsPerUnit = ppu;
                    changed = true;
                }
                if (importer.filterMode != FilterMode.Point)
                {
                    importer.filterMode = FilterMode.Point;
                    changed = true;
                }
                if (importer.textureCompression != TextureImporterCompression.Uncompressed)
                {
                    importer.textureCompression = TextureImporterCompression.Uncompressed;
                    changed = true;
                }
                if (importer.mipmapEnabled)
                {
                    importer.mipmapEnabled = false;
                    changed = true;
                }

                if (changed)
                {
                    importer.SaveAndReimport();
                    count++;
                }
            }
            Debug.Log($"[SproutLands] 已配置 {count}/{guids.Length} 个纹理 ({folder})");
        }

        private static void ConfigureUISliceBorders()
        {
            int count = 0;

            count += SetBorder("Assets/Game/Res/Sprites/UI/SL_UI_Panel.png",
                new Vector4(16, 16, 16, 16));

            var btnNames = new[]
            {
                "SL_UI_Btn_Lightest", "SL_UI_Btn_Lightest_Dark",
                "SL_UI_Btn_Light", "SL_UI_Btn_Light_Dark",
                "SL_UI_Btn_Medium", "SL_UI_Btn_Medium_Dark",
                "SL_UI_Btn_Dark", "SL_UI_Btn_Dark_Dark",
            };
            foreach (var n in btnNames)
                count += SetBorder($"Assets/Game/Res/Sprites/UI/{n}.png",
                    new Vector4(6, 6, 6, 6));

            Debug.Log($"[SproutLands] 已配置 {count} 个 9-slice 边框");
        }

        private static int SetBorder(string path, Vector4 border)
        {
            if (AssetImporter.GetAtPath(path) is not TextureImporter importer) return 0;
            if (importer.spriteBorder == border) return 0;

            importer.spriteBorder = border;
            importer.SaveAndReimport();
            return 1;
        }
    }
}
#endif
