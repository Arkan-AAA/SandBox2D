using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class GameOverUI : MonoBehaviour {
    public Button restartButton;
    public Button mainMenuButton;
    public Button quitButton;
    public TMP_Text scoreText;
    public TMP_Text enemiesKilledText;

    private void Start() {
        if (restartButton != null) restartButton.onClick.AddListener(RestartGame);
        if (mainMenuButton != null) mainMenuButton.onClick.AddListener(GoToMainMenu);
        if (quitButton != null) quitButton.onClick.AddListener(QuitGame);
        SetupNavigation();
    }

    private void RestartGame() => GameManager.Instance?.RestartLevel();
    private void GoToMainMenu() => GameManager.Instance?.LoadMainMenu();
    private void QuitGame() => GameManager.Instance?.QuitGame();

    public void SetFocus() {
        if (restartButton != null && EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(restartButton.gameObject);
    }

    private void SetupNavigation() {
        Button[] buttons = { restartButton, mainMenuButton, quitButton };
        for (int i = 0; i < buttons.Length; i++) {
            if (buttons[i] == null) continue;
            var nav = buttons[i].navigation;
            nav.mode = Navigation.Mode.Explicit;
            nav.selectOnUp = i > 0 ? buttons[i - 1] : null;
            nav.selectOnDown = i < buttons.Length - 1 ? buttons[i + 1] : null;
            buttons[i].navigation = nav;
        }
    }
}