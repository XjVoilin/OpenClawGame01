using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using JulyCore;
using UnityEngine;

namespace CozyYard
{
    public static class SpriteLoader
    {
        private static readonly Dictionary<string, Sprite> _cache = new();

        public static async UniTask<Sprite> LoadAsync(string spriteName)
        {
            if (string.IsNullOrEmpty(spriteName)) return null;
            if (_cache.TryGetValue(spriteName, out var cached)) return cached;

            try
            {
                var sprite = await GF.Resource.LoadAsync<Sprite>(spriteName);
                if (sprite != null)
                {
                    _cache[spriteName] = sprite;
                }
                else
                {
                    Debug.LogWarning($"[SpriteLoader] 加载失败(null): {spriteName}");
                }
                return sprite;
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[SpriteLoader] 加载异常: {spriteName} -> {ex.Message}");
                return null;
            }
        }

        public static void ClearCache() => _cache.Clear();
    }
}
