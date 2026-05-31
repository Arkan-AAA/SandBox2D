using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameOverUI : MonoBehaviour {
    [Header("Buttons")]
    public Button restartButton;
    public Button mainMenuButton;
    public Button quitButton;

    [Header("Text")]
    public TMP_Text scoreText;
    public TMP_Text enemiesKilledText;

    private void Start() {
        if (restartButton != null) restartButton.onClick.AddListener(RestartGame);
        if (mainMenuButton != null) mainMenuButton.onClick.AddListener(GoToMainMenu);
        if (quitButton != null) quitButton.onClick.AddListener(QuitGame);
    }

    private void RestartGame() {
        GameManager.Instance?.RestartLevel();
    }

    private void GoToMainMenu() {
        GameManager.Instance?.LoadMainMenu();
    }

    private void QuitGame() {
        GameManager.Instance?.QuitGame();
    }
}