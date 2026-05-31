using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
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

    private const string MASTER_VOL_KEY = "MasterVolume";
    private const string MUSIC_VOL_KEY = "MusicVolume";
    private const string SFX_VOL_KEY = "SFXVolume";
    private const string MUTE_KEY = "Mute";
    private const string FULLSCREEN_KEY = "Fullscreen";

    void Start() {
        LoadSettings();
    }

    public void SetMasterVolume(float value) {
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
        if (isMuted) {
            audioMixer.SetFloat("MasterVolume", -80f);
        }
        else {
            audioMixer.SetFloat("MasterVolume", Mathf.Log10(masterVolumeSlider.value) * 20);
        }
        PlayerPrefs.SetInt(MUTE_KEY, isMuted ? 1 : 0);
    }

    public void SetFullscreen(bool isFullscreen) {
        Screen.fullScreen = isFullscreen;
        PlayerPrefs.SetInt(FULLSCREEN_KEY, isFullscreen ? 1 : 0);
    }

    void LoadSettings() {
        float master = PlayerPrefs.GetFloat(MASTER_VOL_KEY, 1f);
        float music = PlayerPrefs.GetFloat(MUSIC_VOL_KEY, 1f);
        float sfx = PlayerPrefs.GetFloat(SFX_VOL_KEY, 1f);
        bool muted = PlayerPrefs.GetInt(MUTE_KEY, 0) == 1;
        bool fs = PlayerPrefs.GetInt(FULLSCREEN_KEY, Screen.fullScreen ? 1 : 0) == 1;

        if (masterVolumeSlider != null) masterVolumeSlider.value = master;
        if (musicVolumeSlider != null) musicVolumeSlider.value = music;
        if (sfxVolumeSlider != null) sfxVolumeSlider.value = sfx;
        if (muteToggle != null) muteToggle.isOn = muted;
        if (fullscreenToggle != null) fullscreenToggle.isOn = fs;

        if (!muted) {
            audioMixer.SetFloat("MasterVolume", Mathf.Log10(master) * 20);
        }
        audioMixer.SetFloat("MusicVolume", Mathf.Log10(music) * 20);
        audioMixer.SetFloat("SFXVolume", Mathf.Log10(sfx) * 20);
        Screen.fullScreen = fs;
    }

    public void SaveSettings() {
        PlayerPrefs.Save();
    }

    public void Back() {
        SaveSettings();
        GameManager.Instance?.LoadMainMenu();
    }
}