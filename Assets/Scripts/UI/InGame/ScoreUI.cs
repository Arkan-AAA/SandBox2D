using TMPro;
using UnityEngine;

public class ScoreUI : MonoBehaviour {
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text enemiesKilledText;

    private void Start() {
        UpdateScore(0);
        UpdateEnemiesKilled(0);

        if (GameManager.Instance != null) {
            // Подписка на изменение счёта
        }
    }

    public void UpdateScore(int score) {
        if (scoreText != null)
            scoreText.text = $"Score: {score}";
    }

    public void UpdateEnemiesKilled(int count) {
        if (enemiesKilledText != null)
            enemiesKilledText.text = $"Kills: {count}";
    }
}