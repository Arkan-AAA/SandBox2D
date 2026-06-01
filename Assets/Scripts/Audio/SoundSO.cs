using UnityEngine;

namespace Audio {
    [CreateAssetMenu(fileName = "SoundSO", menuName = "Audio/Sound")]
    public class SoundSO : ScriptableObject {
        [Header("Clips")]
        public AudioClip[] clips;               // Несколько клипов — будет выбран случайный

        [Header("Volume")]
        [Range(0f, 1f)] public float volume = 1f;
        [Range(0f, 0.3f)] public float volumeVariance = 0.05f;

        [Header("Pitch")]
        [Range(0.1f, 3f)] public float pitch = 1f;
        [Range(0f, 0.3f)] public float pitchVariance = 0.05f;

        [Header("Settings")]
        public bool loop = false;
        [Range(0f, 1f)] public float spatialBlend = 0f;  // 0 = 2D, 1 = 3D
        public float minDistance = 1f;
        public float maxDistance = 20f;

        /// <summary>Случайный клип из списка.</summary>
        public AudioClip GetClip() {
            if (clips == null || clips.Length == 0) return null;
            return clips[Random.Range(0, clips.Length)];
        }

        public float GetVolume() => volume + Random.Range(-volumeVariance, volumeVariance);
        public float GetPitch() => pitch + Random.Range(-pitchVariance, pitchVariance);
    }
}
