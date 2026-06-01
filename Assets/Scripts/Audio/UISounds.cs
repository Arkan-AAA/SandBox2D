using Audio;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Вешается на кнопки UI (или на общий UI GameObject).
/// Вариант A — на отдельную кнопку (автоподписка).
/// Вариант B — вызывай Play* методы вручную из других UI скриптов.
/// </summary>
public class UISounds : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    [Header("UI SFX")]
    [SerializeField] private SoundSO _hoverSFX;
    [SerializeField] private SoundSO _clickSFX;
    [SerializeField] private SoundSO _openMenuSFX;
    [SerializeField] private SoundSO _closeMenuSFX;
    [SerializeField] private SoundSO _equipWeaponSFX;
    [SerializeField] private SoundSO _levelUpSFX;
    [SerializeField] private SoundSO _gameOverSFX;

    // ── IPointerHandler — автоматически для кнопок ──────────────

    public void OnPointerEnter(PointerEventData _)
    {
        AudioManager.Instance?.PlayUI(_hoverSFX);
    }

    public void OnPointerClick(PointerEventData _)
    {
        AudioManager.Instance?.PlayUI(_clickSFX);
    }

    // ── Публичные методы для вызова из других скриптов ──────────

    public void PlayOpenMenu()    => AudioManager.Instance?.PlayUI(_openMenuSFX);
    public void PlayCloseMenu()   => AudioManager.Instance?.PlayUI(_closeMenuSFX);
    public void PlayEquipWeapon() => AudioManager.Instance?.PlayUI(_equipWeaponSFX);
    public void PlayLevelUp()     => AudioManager.Instance?.PlayUI(_levelUpSFX);
    public void PlayGameOver()    => AudioManager.Instance?.PlayUI(_gameOverSFX);
}
