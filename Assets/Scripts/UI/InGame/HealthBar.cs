using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour {
    [Header("Heart Images")]
    [SerializeField] private Image[] _hearts; // массив из 3 сердец

    [Header("Heart Sprites")]
    [SerializeField] private Sprite _fullHeart;
    [SerializeField] private Sprite _halfHeart;
    [SerializeField] private Sprite _emptyHeart;

    [Header("Text (опционально)")]
    [SerializeField] private TMP_Text _healthText;

    private int _maxHealth = 60;
    private int _healthPerHeart = 20;

    private void Start() {
        _maxHealth = Player.Instance.MaxHealth;
        _healthPerHeart = Player.Instance.HealthPerHeart;
        UpdateHealthBar(Player.Instance.CurrentHealth, Player.Instance.MaxHealth);
        Player.Instance.OnHealthChanged += UpdateHealthBar;
    }

    private void OnDestroy() {
        if (Player.Instance != null)
            Player.Instance.OnHealthChanged -= UpdateHealthBar;
    }

    private void UpdateHealthBar(int currentHealth, int maxHealth) {
        for (int i = 0; i < _hearts.Length; i++) {
            int heartStartHP = i * _healthPerHeart + 1;
            int heartEndHP = (i + 1) * _healthPerHeart;
            int healthInHeart = currentHealth - (heartStartHP - 1);

            if (healthInHeart >= _healthPerHeart) {
                _hearts[i].sprite = _fullHeart;
            }
            else if (healthInHeart > 0) {
                // Показываем половинку или четвертинку
                float percent = (float)healthInHeart / _healthPerHeart;
                if (percent >= 0.5f)
                    _hearts[i].sprite = _halfHeart;
                else
                    _hearts[i].sprite = _emptyHeart;
            }
            else {
                _hearts[i].sprite = _emptyHeart;
            }
        }

        if (_healthText != null)
            _healthText.text = $"{currentHealth} / {maxHealth}";
    }
}