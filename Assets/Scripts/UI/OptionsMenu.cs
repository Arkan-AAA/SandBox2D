using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using TMPro;

public class OptionsMenu : MonoBehaviour {
    [Header("Audio")]
    public AudioMixer audioMixer;
    public Slider masterVolumeSlider;
    public Slider musicVolumeSlider;
    public Slider sfxVolumeSlider;
    public Toggle muteToggle;

    [Header("Graphics")]
    public Toggle fullscreenToggle;

    [Header("Navigation")]
    public string mainMenuScene = "MainMenu";

    // Ключи для PlayerPrefs
    private const string MASTER_VOL_KEY = "MasterVolume";
    private const string MUSIC_VOL_KEY = "MusicVolume";
    private const string SFX_VOL_KEY = "SFXVolume";
    private const string MUTE_KEY = "Mute";
    private const string FULLSCREEN_KEY = "Fullscreen";

    void Start() {
        LoadSettings();
    }

    // ─── Audio ───────────────────────────────────────────

    public void SetMasterVolume(float value) {
        // Slider: 0.0001 → 1, конвертируем в dB
        audioMixer.SetFloat("MasterVolume", Mathf.Log10(value) * 20);
        PlayerPrefs.SetFloat(MASTER_VOL_KEY, value);
    }

    public void SetMusicVolume(float value) {
        audioMixer.SetFloat("MusicVolume", Mathf.Log10(value) * 20);
        PlayerPrefs.SetFloat(MUSIC_VOL_KEY, value);
    }

    public void SetSFXVolume(float value) {
        audioMixer.SetFloat("SFXVolume", Mathf.Log10(value) * 20);
        PlayerPrefs.SetFloat(SFX_VOL_KEY, value);
    }

    public void SetMute(bool isMuted) {
        audioMixer.SetFloat("MasterVolume", isMuted ? -80f : Mathf.Log10(masterVolumeSlider.value) * 20);
        PlayerPrefs.SetInt(MUTE_KEY, isMuted ? 1 : 0);
    }

    // ─── Graphics ────────────────────────────────────────

    public void SetFullscreen(bool isFullscreen) {
        Screen.fullScreen = isFullscreen;
        PlayerPrefs.SetInt(FULLSCREEN_KEY, isFullscreen ? 1 : 0);
    }

    // ─── Save / Load ─────────────────────────────────────

    void LoadSettings() {
        float master = PlayerPrefs.GetFloat(MASTER_VOL_KEY, 1f);
        float music = PlayerPrefs.GetFloat(MUSIC_VOL_KEY, 1f);
        float sfx = PlayerPrefs.GetFloat(SFX_VOL_KEY, 1f);
        bool muted = PlayerPrefs.GetInt(MUTE_KEY, 0) == 1;
        bool fs = PlayerPrefs.GetInt(FULLSCREEN_KEY, Screen.fullScreen ? 1 : 0) == 1;

        masterVolumeSlider.value = master;
        musicVolumeSlider.value = music;
        sfxVolumeSlider.value = sfx;
        muteToggle.isOn = muted;
        fullscreenToggle.isOn = fs;

        audioMixer.SetFloat("MasterVolume", Mathf.Log10(master) * 20);
        audioMixer.SetFloat("MusicVolume", Mathf.Log10(music) * 20);
        audioMixer.SetFloat("SFXVolume", Mathf.Log10(sfx) * 20);
        Screen.fullScreen = fs;
    }

    public void SaveSettings() {
        PlayerPrefs.Save(); // flush на диск
    }

    // ─── Navigation ─────────────────────────────────────

    public void Back() {
        SaveSettings();
        SceneManager.LoadScene(mainMenuScene);
    }
}