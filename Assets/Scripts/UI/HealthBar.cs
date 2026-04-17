using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private Slider slider;
    [SerializeField] private Health playerHealth;

    private void Start()
    {
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = 1f;

        playerHealth.OnHealthChanged.AddListener(UpdateBar);
    }

    private void UpdateBar(int current, int max)
    {
        slider.value = (float)current / max;
    }

    private void OnDestroy()
    {
        playerHealth.OnHealthChanged.RemoveListener(UpdateBar);
    }
}
