using UnityEngine;
using JulyCore;

namespace IsleWorks.Tech
{
    /// <summary>
    /// 粒子反馈管理器，用于在时代切换时播放粒子特效。
    /// </summary>
    public static class ParticleFeedbackManager
    {
        public static void PlayEraTransitionEffect(Transform effectRoot)
        {
            var particleSystem = effectRoot.Find("EraTransitionParticles")?.GetComponent<ParticleSystem>();
            if (particleSystem == null)
            {
                GF.LogError("Era transition particle system not found.");
                return;
            }

            particleSystem.Play();
            GF.Log("Era transition particles played.");
        }
    }
}