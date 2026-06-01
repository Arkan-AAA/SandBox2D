using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

namespace Audio {
    public class AudioManager : MonoBehaviour {
        public static AudioManager Instance { get; private set; }

        [Header("Mixer")]
        [SerializeField] private AudioMixer _mixer;
        [SerializeField] private string _sfxGroupName = "SFX";
        [SerializeField] private string _musicGroupName = "Music";
        [SerializeField] private string _uiGroupName = "UI";

        [Header("Music Sources")]
        [SerializeField] private AudioSource _musicSourceA;
        [SerializeField] private AudioSource _musicSourceB;
        [SerializeField] private float _crossfadeTime = 1f;
        private bool _useSourceA = true;

        [Header("Default Playlist (SoundSO[])")]
        [SerializeField] private SoundSO[] _defaultSoundSOPlaylist;
        [SerializeField] private bool _shuffleDefaultSoundSO = false;
        [SerializeField] private bool _loopDefaultSoundSO = true;

        private AudioPool _pool;
        private AudioMixerGroup _sfxGroup, _musicGroup, _uiGroup;

        private Coroutine _playlistCoroutine;
        // Для SoundSO[] плейлиста
        private SoundSO[] _currentSoundSOPlaylist;
        private int _currentTrackIndex;
        private bool _shuffle;
        private bool _loop;
        // Для PlaylistSO (AudioClip[])
        private PlaylistSO _currentPlaylistSO;

        private void Awake() {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            _pool = GetComponent<AudioPool>();
            if (_pool == null) _pool = gameObject.AddComponent<AudioPool>();

            if (_musicSourceA == null) _musicSourceA = CreateMusicSource("MusicSourceA");
            if (_musicSourceB == null) _musicSourceB = CreateMusicSource("MusicSourceB");
            _musicSourceA.volume = 0f;
            _musicSourceB.volume = 0f;

            if (_mixer != null) {
                _sfxGroup = GetMixerGroup(_sfxGroupName);
                _musicGroup = GetMixerGroup(_musicGroupName);
                _uiGroup = GetMixerGroup(_uiGroupName);
                if (_musicGroup != null) {
                    _musicSourceA.outputAudioMixerGroup = _musicGroup;
                    _musicSourceB.outputAudioMixerGroup = _musicGroup;
                }
            }
        }

        private AudioSource CreateMusicSource(string name) {
            var go = new GameObject(name);
            go.transform.SetParent(transform);
            var src = go.AddComponent<AudioSource>();
            src.loop = false;
            src.playOnAwake = false;
            src.spatialBlend = 0f;
            return src;
        }

        private AudioMixerGroup GetMixerGroup(string name) {
            if (_mixer == null) return null;
            var groups = _mixer.FindMatchingGroups(name);
            return groups.Length > 0 ? groups[0] : null;
        }

        // ─── SFX / UI ────────────────────────────────────────────────
        public AudioSource PlaySFX(SoundSO sound, Vector3 position = default) {
            if (sound == null) return null;
            var src = _pool.Get(sound, position);
            if (src != null && _sfxGroup != null) src.outputAudioMixerGroup = _sfxGroup;
            return src;
        }

        public AudioSource PlayUI(SoundSO sound) {
            if (sound == null) return null;
            var src = _pool.Get(sound, Vector3.zero);
            if (src != null) {
                src.spatialBlend = 0f;
                if (_uiGroup != null) src.outputAudioMixerGroup = _uiGroup;
            }
            return src;
        }

        public void StopAllSFX() => _pool.StopAll();

        // ─── Одиночный трек (SoundSO) ─────────────────────────────────
        public void PlayMusic(SoundSO track, float volume = 1f) {
            if (_playlistCoroutine != null) StopCoroutine(_playlistCoroutine);
            StartCoroutine(PlaySingleTrack(track, volume));
        }

        private IEnumerator PlaySingleTrack(SoundSO track, float targetVolume) {
            AudioSource newSource = _useSourceA ? _musicSourceB : _musicSourceA;
            AudioSource oldSource = _useSourceA ? _musicSourceA : _musicSourceB;

            newSource.clip = track.GetClip();
            newSource.volume = 0f;
            newSource.Play();

            float elapsed = 0f;
            while (elapsed < _crossfadeTime) {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / _crossfadeTime;
                newSource.volume = Mathf.Lerp(0f, targetVolume, t);
                oldSource.volume = Mathf.Lerp(targetVolume, 0f, t);
                yield return null;
            }
            newSource.volume = targetVolume;
            oldSource.volume = 0f;
            oldSource.Stop();
            _useSourceA = !_useSourceA;
        }

        // ─── Плейлист из SoundSO[] ─────────────────────────────────────
        public void StartPlaylist(SoundSO[] playlist, bool shuffle = false, bool loop = true) {
            if (playlist == null || playlist.Length == 0) {
                Debug.LogWarning("SoundSO playlist is empty!");
                return;
            }
            if (_playlistCoroutine != null) StopCoroutine(_playlistCoroutine);
            _currentSoundSOPlaylist = playlist;
            _shuffle = shuffle;
            _loop = loop;
            _currentTrackIndex = _shuffle ? Random.Range(0, _currentSoundSOPlaylist.Length) : 0;
            _playlistCoroutine = StartCoroutine(PlaySoundSOPlaylistCoroutine());
        }

        private IEnumerator PlaySoundSOPlaylistCoroutine() {
            while (_loop || _currentTrackIndex < _currentSoundSOPlaylist.Length) {
                if (_currentTrackIndex >= _currentSoundSOPlaylist.Length) {
                    if (_loop) _currentTrackIndex = 0;
                    else break;
                }

                SoundSO currentTrack = _currentSoundSOPlaylist[_currentTrackIndex];
                if (currentTrack == null) {
                    _currentTrackIndex++;
                    continue;
                }

                AudioSource newSource = _useSourceA ? _musicSourceB : _musicSourceA;
                AudioSource oldSource = _useSourceA ? _musicSourceA : _musicSourceB;
                newSource.clip = currentTrack.GetClip();
                newSource.volume = 0f;
                newSource.Play();

                float elapsed = 0f;
                while (elapsed < _crossfadeTime) {
                    elapsed += Time.unscaledDeltaTime;
                    float t = elapsed / _crossfadeTime;
                    newSource.volume = Mathf.Lerp(0f, currentTrack.GetVolume(), t);
                    oldSource.volume = Mathf.Lerp(oldSource.volume, 0f, t);
                    yield return null;
                }
                newSource.volume = currentTrack.GetVolume();
                oldSource.volume = 0f;
                oldSource.Stop();
                _useSourceA = !_useSourceA;

                float clipLength = newSource.clip.length;
                yield return new WaitForSecondsRealtime(clipLength);

                if (_shuffle)
                    _currentTrackIndex = Random.Range(0, _currentSoundSOPlaylist.Length);
                else
                    _currentTrackIndex++;
            }
            _playlistCoroutine = null;
        }

        // ─── Плейлист из PlaylistSO (AudioClip[]) ──────────────────────
        public void StartPlaylist(PlaylistSO playlist) {
            if (playlist == null || playlist.tracks == null || playlist.tracks.Length == 0) {
                Debug.LogWarning("PlaylistSO is empty!");
                return;
            }
            if (_playlistCoroutine != null) StopCoroutine(_playlistCoroutine);
            _currentPlaylistSO = playlist;
            _currentPlaylistSO.ResetPlaylist(); // сбросить индекс и перемешать, если нужно
            _playlistCoroutine = StartCoroutine(PlayAudioClipPlaylistCoroutine());
        }

        private IEnumerator PlayAudioClipPlaylistCoroutine() {
            while (true) {
                AudioClip nextClip = _currentPlaylistSO.GetNextClip();
                if (nextClip == null) break; // плейлист закончился

                AudioSource newSource = _useSourceA ? _musicSourceB : _musicSourceA;
                AudioSource oldSource = _useSourceA ? _musicSourceA : _musicSourceB;

                newSource.clip = nextClip;
                newSource.volume = 0f;
                newSource.pitch = _currentPlaylistSO.pitch;
                newSource.Play();

                float elapsed = 0f;
                float crossfade = _currentPlaylistSO.crossfadeTime;
                while (elapsed < crossfade) {
                    elapsed += Time.unscaledDeltaTime;
                    float t = elapsed / crossfade;
                    newSource.volume = Mathf.Lerp(0f, _currentPlaylistSO.volume, t);
                    oldSource.volume = Mathf.Lerp(oldSource.volume, 0f, t);
                    yield return null;
                }
                newSource.volume = _currentPlaylistSO.volume;
                oldSource.volume = 0f;
                oldSource.Stop();
                _useSourceA = !_useSourceA;

                yield return new WaitForSecondsRealtime(nextClip.length);
            }
            _playlistCoroutine = null;
        }

        public void StartDefaultPlaylist() {
            if (_defaultSoundSOPlaylist != null && _defaultSoundSOPlaylist.Length > 0)
                StartPlaylist(_defaultSoundSOPlaylist, _shuffleDefaultSoundSO, _loopDefaultSoundSO);
            else
                Debug.LogWarning("Default SoundSO playlist is empty!");
        }

        public void StopMusic() {
            if (_playlistCoroutine != null) StopCoroutine(_playlistCoroutine);
            _musicSourceA.Stop();
            _musicSourceB.Stop();
            _musicSourceA.volume = 0f;
            _musicSourceB.volume = 0f;
        }

        public void SetVolume(string paramName, float value) {
            _mixer?.SetFloat(paramName, Mathf.Log10(Mathf.Clamp(value, 0.0001f, 1f)) * 20f);
        }
    }
}