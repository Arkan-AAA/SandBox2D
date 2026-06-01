using UnityEngine;
using UnityEngine.UI;

public class PauseMenuUI : MonoBehaviour {
    [Header("Buttons")]
    public Button resumeButton;
    public Button optionsButton;
    public Button mainMenuButton;
    public Button quitButton;

    private void Start() {
        if (resumeButton != null) resumeButton.onClick.AddListener(Resume);
        if (optionsButton != null) optionsButton.onClick.AddListener(Options);
        if (mainMenuButton != null) mainMenuButton.onClick.AddListener(MainMenu);
        if (quitButton != null) quitButton.onClick.AddListener(Quit);

    }

    private void Resume() {
        GameManager.Instance?.ResumeGame();
    }

    private void Options() {
        // Вызываем открытие панели настроек через GameManager
        GameManager.Instance?.ToggleOptions();
    }

    private void MainMenu() {
        GameManager.Instance?.LoadMainMenu();
    }

    private void Quit() {
        GameManager.Instance?.QuitGame();
    }
}