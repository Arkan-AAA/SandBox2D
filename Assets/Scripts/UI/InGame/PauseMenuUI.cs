using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PauseMenuUI : MonoBehaviour {
    [Header("Buttons")]
    public Button resumeButton;
    public Button optionsButton;
    public Button mainMenuButton;
    public Button quitButton;

    private void Start() {
        SetupNavigation();
        if (resumeButton != null) resumeButton.onClick.AddListener(Resume);
        if (optionsButton != null) optionsButton.onClick.AddListener(Options);
        if (mainMenuButton != null) mainMenuButton.onClick.AddListener(MainMenu);
        if (quitButton != null) quitButton.onClick.AddListener(Quit);
    }

    private void Resume() => GameManager.Instance?.ResumeGame();
    private void Options() => GameManager.Instance?.ToggleOptions();
    private void MainMenu() => GameManager.Instance?.LoadMainMenu();
    private void Quit() => GameManager.Instance?.QuitGame();

    /// <summary>Установить фокус на кнопку Resume (вызывается из GameManager).</summary>
    public void SetFocus() {
        if (resumeButton != null && EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(resumeButton.gameObject);
    }

    private void SetupNavigation() {
        if (resumeButton == null || optionsButton == null) return;

        var resumeNav = resumeButton.navigation;
        resumeNav.mode = Navigation.Mode.Explicit;
        resumeNav.selectOnDown = optionsButton;
        resumeButton.navigation = resumeNav;

        var optionsNav = optionsButton.navigation;
        optionsNav.mode = Navigation.Mode.Explicit;
        optionsNav.selectOnUp = resumeButton;
        optionsNav.selectOnDown = mainMenuButton;
        optionsButton.navigation = optionsNav;

        if (mainMenuButton != null && quitButton != null) {
            var mainNav = mainMenuButton.navigation;
            mainNav.mode = Navigation.Mode.Explicit;
            mainNav.selectOnUp = optionsButton;
            mainNav.selectOnDown = quitButton;
            mainMenuButton.navigation = mainNav;

            var quitNav = quitButton.navigation;
            quitNav.mode = Navigation.Mode.Explicit;
            quitNav.selectOnUp = mainMenuButton;
            quitButton.navigation = quitNav;
        }
    }
}