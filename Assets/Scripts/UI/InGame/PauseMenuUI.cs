using UnityEngine;
using UnityEngine.UI;

public class PauseMenuUI : MonoBehaviour {
    [Header("Buttons")]
    public Button resumeButton;
    public Button optionsButton;
    public Button mainMenuButton;

    private void Start() {
        if (resumeButton != null) resumeButton.onClick.AddListener(Resume);
        if (optionsButton != null) optionsButton.onClick.AddListener(Options);
        if (mainMenuButton != null) mainMenuButton.onClick.AddListener(MainMenu);
    }

    private void Resume() {
        GameManager.Instance?.ResumeGame();
    }

    private void Options() {
        UIManager.Instance.ShowOptions();
    }

    private void MainMenu() {
        GameManager.Instance?.LoadMainMenu();
    }
}