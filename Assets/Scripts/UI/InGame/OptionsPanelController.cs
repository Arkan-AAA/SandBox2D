using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using UnityEngine.EventSystems;

public class OptionsPanelController : MonoBehaviour {
    [Header("Navigation")]
    public Selectable firstSelected;

    [Header("Audio")]
    public AudioMixer audioMixer;
    public Slider masterSlider;
    public Slider musicSlider;
    public Slider sfxSlider;
    public Slider uiSlider;
    public Toggle muteToggle;

    [Header("Graphics")]
    public Toggle fullscreenToggle;

    private const string MASTER_VOL_KEY = "MasterVolume";
    private const string MUSIC_VOL_KEY = "MusicVolume";
    private const string SFX_VOL_KEY = "SFXVolume";
    private const string UI_VOL_KEY = "UIVolume";
    private const string MUTE_KEY = "Mute";
    private const string FULLSCREEN_KEY = "Fullscreen";

    void Start() {
        LoadSettings();

        masterSlider.onValueChanged.AddListener(SetMasterVolume);
        musicSlider.onValueChanged.AddListener(SetMusicVolume);
        sfxSlider.onValueChanged.AddListener(SetSFXVolume);
        uiSlider.onValueChanged.AddListener(SetUIVolume);
        muteToggle.onValueChanged.AddListener(SetMute);
        fullscreenToggle.onValueChanged.AddListener(SetFullscreen);
    }

    private static float ToDb(float value) => Mathf.Log10(Mathf.Max(value, 0.0001f)) * 20f;

    public void SetMasterVolume(float value) {
        audioMixer.SetFloat("MasterVolume", ToDb(value));
        PlayerPrefs.SetFloat(MASTER_VOL_KEY, value);
    }

    public void SetMusicVolume(float value) {
        audioMixer.SetFloat("MusicVolume", ToDb(value));
        PlayerPrefs.SetFloat(MUSIC_VOL_KEY, value);
    }

    public void SetSFXVolume(float value) {
        audioMixer.SetFloat("SFXVolume", ToDb(value));
        PlayerPrefs.SetFloat(SFX_VOL_KEY, value);
    }

    public void SetUIVolume(float value) {
        audioMixer.SetFloat("UIVolume", ToDb(value));
        PlayerPrefs.SetFloat(UI_VOL_KEY, value);
    }

    public void SetMute(bool isMuted) {
        audioMixer.SetFloat("MasterVolume", isMuted ? -80f : ToDb(masterSlider != null ? masterSlider.value : 1f));
        PlayerPrefs.SetInt(MUTE_KEY, isMuted ? 1 : 0);
    }

    public void SetFullscreen(bool isFullscreen) {
        Screen.fullScreen = isFullscreen;
        PlayerPrefs.SetInt(FULLSCREEN_KEY, isFullscreen ? 1 : 0);
    }

    private void LoadSettings() {
        float master = PlayerPrefs.GetFloat(MASTER_VOL_KEY, 1f);
        float music = PlayerPrefs.GetFloat(MUSIC_VOL_KEY, 1f);
        float sfx = PlayerPrefs.GetFloat(SFX_VOL_KEY, 1f);
        float ui = PlayerPrefs.GetFloat(UI_VOL_KEY, 1f);
        bool muted = PlayerPrefs.GetInt(MUTE_KEY, 0) == 1;
        bool fs = PlayerPrefs.GetInt(FULLSCREEN_KEY, Screen.fullScreen ? 1 : 0) == 1;

        if (masterSlider != null) masterSlider.value = master;
        if (musicSlider != null) musicSlider.value = music;
        if (sfxSlider != null) sfxSlider.value = sfx;
        if (uiSlider != null) uiSlider.value = ui;
        if (muteToggle != null) muteToggle.isOn = muted;
        if (fullscreenToggle != null) fullscreenToggle.isOn = fs;

        if (!muted) {
            audioMixer.SetFloat("MasterVolume", ToDb(master));
        }
        else {
            audioMixer.SetFloat("MasterVolume", -80f);
        }
        audioMixer.SetFloat("MusicVolume", ToDb(music));
        audioMixer.SetFloat("SFXVolume", ToDb(sfx));
        audioMixer.SetFloat("UIVolume", ToDb(ui));
        Screen.fullScreen = fs;
    }

    public void SaveSettings() {
        PlayerPrefs.Save();
    }

    public void Close() {
        SaveSettings();
        if (GameManager.Instance != null)
            GameManager.Instance.ToggleOptions();
        else
            gameObject.SetActive(false);
    }

    private void OnEnable() {
        if (firstSelected != null && EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(firstSelected.gameObject);
    }
}