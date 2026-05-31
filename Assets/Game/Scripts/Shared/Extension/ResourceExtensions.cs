using Cysharp.Threading.Tasks;
using JulyCore;
using UnityEngine;
using UnityEngine.UI;

namespace CozyYard
{
    public static class ResourceExtensions
    {
        public static void LoadSprite(this Image img, string resName)
        {
            if (img == null) return;
            GF.Resource.LoadAsync<Sprite>(resName, img.gameObject)
                .ContinueWith(s => { if (img != null) img.sprite = s; })
                .Forget();
        }

        public static void LoadSprite(this SpriteRenderer renderer, string resName)
        {
            if (renderer == null) return;
            GF.Resource.LoadAsync<Sprite>(resName, renderer.gameObject)
                .ContinueWith(s => { if (renderer != null) renderer.sprite = s; })
                .Forget();
        }

        public static void LoadTexture(this RawImage img, string resName)
        {
            if (img == null) return;
            GF.Resource.LoadAsync<Texture>(resName, img.gameObject)
                .ContinueWith(s => { if (img != null) img.texture = s; })
                .Forget();
        }
    }
}
