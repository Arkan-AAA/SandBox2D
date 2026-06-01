using UnityEngine;

namespace Audio {
    [CreateAssetMenu(fileName = "PlaylistSO", menuName = "Audio/Playlist (AudioClip)")]
    public class PlaylistSO : ScriptableObject {
        [Header("Tracks (AudioClips)")]
        public AudioClip[] tracks;

        [Header("Playback Settings")]
        public bool loopPlaylist = true;
        public bool shuffle = false;
        public float crossfadeTime = 1f;

        [Header("Global Volume & Pitch")]
        [Range(0f, 1f)] public float volume = 1f;
        [Range(0.1f, 3f)] public float pitch = 1f;

        // Внутреннее состояние (не сохраняется)
        private int _currentIndex = -1;
        private int[] _shuffledIndices;

        /// <summary>Получить следующий клип в плейлисте.</summary>
        public AudioClip GetNextClip() {
            if (tracks == null || tracks.Length == 0)
                return null;

            // Инициализация при первом вызове или после изменений
            if (shuffle) {
                if (_shuffledIndices == null || _shuffledIndices.Length != tracks.Length)
                    ResetPlaylist();

                _currentIndex++;
                if (_currentIndex >= _shuffledIndices.Length) {
                    if (!loopPlaylist) return null;
                    _currentIndex = 0;
                    ShuffleIndices(); // перетасовать заново при зацикливании
                }

                int trackIdx = _shuffledIndices[_currentIndex];
                return tracks[trackIdx];
            }
            else {
                // Без перемешивания — прямой порядок
                _currentIndex++;
                if (_currentIndex >= tracks.Length) {
                    if (!loopPlaylist) return null;
                    _currentIndex = 0;
                }
                return tracks[_currentIndex];
            }
        }

        public void ResetPlaylist() {
            _currentIndex = -1;
            if (shuffle && tracks != null && tracks.Length > 0) {
                _shuffledIndices = new int[tracks.Length];
                for (int i = 0; i < tracks.Length; i++) _shuffledIndices[i] = i;
                ShuffleIndices();
            }
            else {
                _shuffledIndices = null;
            }
        }

        private void ShuffleIndices() {
            if (_shuffledIndices == null) return;
            for (int i = 0; i < _shuffledIndices.Length; i++) {
                int r = Random.Range(i, _shuffledIndices.Length);
                int temp = _shuffledIndices[i];
                _shuffledIndices[i] = _shuffledIndices[r];
                _shuffledIndices[r] = temp;
            }
        }

        private void OnEnable() {
            // Автоматический сброс при загрузке ассета (в редакторе и в игре)
            ResetPlaylist();
        }
    }
}