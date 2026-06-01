using TMPro;
using UnityEngine;

public class ScoreUI : MonoBehaviour {
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text killsText;

    private void Start() {
        if (GameManager.Instance == null) {
            Debug.LogError("GameManager.Instance is null!");
            return;
        }

        // Первоначальное обновление
        UpdateScore(GameManager.Instance.totalScore);
        UpdateKills(GameManager.Instance.totalKills);

        // Подписка на события
        GameManager.Instance.OnScoreChanged += UpdateScore;
        GameManager.Instance.OnKillsChanged += UpdateKills;
    }

    private void OnDestroy() {
        if (GameManager.Instance != null) {
            GameManager.Instance.OnScoreChanged -= UpdateScore;
            GameManager.Instance.OnKillsChanged -= UpdateKills;
        }
    }

    private void UpdateScore(int score) {
        if (scoreText != null) scoreText.text = $"Score: {score}";
    }

    private void UpdateKills(int kills) {
        if (killsText != null) killsText.text = $"Kills: {kills}";
    }
}