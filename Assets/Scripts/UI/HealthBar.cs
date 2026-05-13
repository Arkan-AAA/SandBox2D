using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField]
    private Image _healthImage;

    [SerializeField]
    private Sprite[] _healthSprites;

    [SerializeField]
    private TMP_Text _healthText;

    private void Start()
    {
        UpdateHealthBar(Player.Instance.MaxHealth, Player.Instance.MaxHealth);
        Player.Instance.OnHealthChanged += UpdateHealthBar;
    }

    private void OnDestroy()
    {
        if (Player.Instance != null)
            Player.Instance.OnHealthChanged -= UpdateHealthBar;
    }

    private void UpdateHealthBar(int current, int max)
    {
        if (current <= 0)
        {
            _healthImage.sprite = _healthSprites[_healthSprites.Length - 1];
            _healthText.text = "0 / " + max;
            return;
        }

        float percent = (float)current / max;
        int index = Mathf.Clamp(
            Mathf.FloorToInt((1f - percent) * _healthSprites.Length),
            0,
            _healthSprites.Length - 1
        );
        _healthImage.sprite = _healthSprites[index];

        _healthText.text = current + " / " + max;
    }
}
