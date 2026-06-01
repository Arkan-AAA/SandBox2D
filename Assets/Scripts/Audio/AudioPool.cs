using System.Collections.Generic;
using UnityEngine;

namespace Audio {
    public class AudioPool : MonoBehaviour {
        [SerializeField] private int _initialSize = 16;
        [SerializeField] private int _maxSize = 32;

        private readonly Queue<AudioSource> _available = new();
        private readonly List<AudioSource> _active = new();

        private void Awake() {
            for (int i = 0; i < _initialSize; i++)
                _available.Enqueue(CreateSource());
        }

        private void Update() {
            for (int i = _active.Count - 1; i >= 0; i--) {
                if (!_active[i].isPlaying) {
                    Return(_active[i]);
                    _active.RemoveAt(i);
                }
            }
        }

        public AudioSource Get(SoundSO sound, Vector3 position) {
            if (sound == null) return null;
            AudioClip clip = sound.GetClip();
            if (clip == null) return null;

            AudioSource source = _available.Count > 0
                ? _available.Dequeue()
                : (_active.Count < _maxSize ? CreateSource() : null);

            if (source == null) return null;

            source.transform.position = position;
            source.clip = clip;
            source.volume = sound.GetVolume();
            source.pitch = sound.GetPitch();
            source.loop = sound.loop;
            source.spatialBlend = sound.spatialBlend;
            source.minDistance = sound.minDistance;
            source.maxDistance = sound.maxDistance;
            source.gameObject.SetActive(true);
            source.Play();

            if (!sound.loop)
                _active.Add(source);

            return source;
        }

        public void Return(AudioSource source) {
            if (source == null) return;
            source.Stop();
            source.clip = null;
            source.gameObject.SetActive(false);
            _available.Enqueue(source);
        }

        public void StopAll() {
            foreach (var source in _active) {
                source.Stop();
                Return(source);
            }
            _active.Clear();
        }

        private AudioSource CreateSource() {
            var go = new GameObject("AudioSource");
            go.transform.SetParent(transform);
            go.SetActive(false);
            return go.AddComponent<AudioSource>();
        }
    }
}