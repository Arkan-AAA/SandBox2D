using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour {
    public static UIManager Instance { get; private set; }

    [Header("References")]
    public GameManager gameManager;

    private void Awake() {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else {
            Destroy(gameObject);
        }
    }

    private void Start() {
        if (gameManager == null) {
            gameManager = GameManager.Instance;
        }

        // Подписываемся на изменение состояния
        if (gameManager != null) {
            gameManager.OnStateChanged += OnGameStateChanged;
        }
    }

    private void OnDestroy() {
        if (gameManager != null) {
            gameManager.OnStateChanged -= OnGameStateChanged;
        }
    }

    private void OnGameStateChanged(GameState newState) {
        switch (newState) {
            case GameState.Playing:
                Time.timeScale = 1f;
                break;
            case GameState.Paused:
                Time.timeScale = 0f;
                break;
            case GameState.GameOver:
                Time.timeScale = 0f;
                break;
            case GameState.MainMenu:
                Time.timeScale = 0f;
                break;
        }
    }

    // ========== MAIN MENU ==========
    public void StartGame() {
        if (gameManager != null) {
            gameManager.SetState(GameState.Playing);
        }
        SceneManager.LoadScene("Level1");
    }

    public void QuitToMainMenu() {
        if (gameManager != null) {
            gameManager.LoadMainMenu();
        }
        else {
            SceneManager.LoadScene("MainMenu");
        }
    }

    // ========== PAUSE ==========
    public void PauseGame() {
        gameManager?.PauseGame();
    }

    public void ResumeGame() {
        gameManager?.ResumeGame();
    }

    // ========== GAME OVER ==========
    public void ShowGameOver() {
        gameManager?.GameOver();
    }

    public void RestartGame() {
        gameManager?.RestartLevel();
    }

    // ========== OPTIONS ==========
    public void ShowOptions() {
        // Открыть меню настроек
    }

    public void HideOptions() {
        // Закрыть меню настроек
    }

    public void QuitGame() {
        gameManager?.QuitGame();
    }
}