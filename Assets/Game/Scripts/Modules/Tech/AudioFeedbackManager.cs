using UnityEngine;

namespace IsleWorks.Tech
{
    /// <summary>
    /// 音效反馈管理器，用于在时代切换时提供音效支持。
    /// </summary>
    public static class AudioFeedbackManager
    {
        private static AudioSource _audioSource;

        public static void Initialize(AudioSource source)
        {
            _audioSource = source;
        }

        public static void PlayEraTransitionSound()
        {
            if (_audioSource == null)
            {
                Debug.LogError("AudioSource not initialized for AudioFeedbackManager");
                return;
            }

            // 假设音效名为 EraTransition
            var clip = Resources.Load<AudioClip>("Sounds/EraTransition");
            if (clip != null)
            {
                _audioSource.PlayOneShot(clip);
                Debug.Log("Era transition sound played.");
            }
            else
            {
                Debug.LogError("Era transition sound not found.");
            }
        }
    }
}