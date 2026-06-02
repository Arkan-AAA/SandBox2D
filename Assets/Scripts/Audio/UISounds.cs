using Audio;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UISounds : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler {

    [Header("UI SFX")]
    [SerializeField] private SoundSO _hoverSFX;
    [SerializeField] private SoundSO _clickSFX;
    [SerializeField] private SoundSO _openMenuSFX;
    [SerializeField] private SoundSO _closeMenuSFX;
    [SerializeField] private SoundSO _equipWeaponSFX;
    [SerializeField] private SoundSO _levelUpSFX;
    [SerializeField] private SoundSO _gameOverSFX;

    // ── Мышь ────────────────────────────────────────────────────
    public void OnPointerEnter(PointerEventData _) =>
        AudioManager.Instance?.PlayUI(_hoverSFX);

    public void OnPointerClick(PointerEventData _) =>
        AudioManager.Instance?.PlayUI(_clickSFX);

    // ── Геймпад / клавиатура ─────────────────────────────────────
    // Срабатывает когда навигация переходит на этот объект
    public void OnSelect(BaseEventData _) =>
        AudioManager.Instance?.PlayUI(_hoverSFX);

    // Срабатывает когда нажимают Submit (A на геймпаде, Enter на клавиатуре)
    public void OnSubmit(BaseEventData _) =>
        AudioManager.Instance?.PlayUI(_clickSFX);

    // ── Публичные методы ─────────────────────────────────────────
    public void PlayOpenMenu() => AudioManager.Instance?.PlayUI(_openMenuSFX);
    public void PlayCloseMenu() => AudioManager.Instance?.PlayUI(_closeMenuSFX);
    public void PlayEquipWeapon() => AudioManager.Instance?.PlayUI(_equipWeaponSFX);
    public void PlayLevelUp() => AudioManager.Instance?.PlayUI(_levelUpSFX);
    public void PlayGameOver() => AudioManager.Instance?.PlayUI(_gameOverSFX);
}