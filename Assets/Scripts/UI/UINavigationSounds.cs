using Audio;
using UnityEngine;

public class UINavigationSounds : MonoBehaviour {
    public static UINavigationSounds Instance { get; private set; }

    [SerializeField] private SoundSO _hoverSFX;
    [SerializeField] private SoundSO _clickSFX;

    private void Awake() {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void PlayHover() => AudioManager.Instance?.PlayUI(_hoverSFX);
    public void PlayClick() => AudioManager.Instance?.PlayUI(_clickSFX);
}