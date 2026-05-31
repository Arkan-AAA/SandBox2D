using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public enum GameState {
    None,
    MainMenu,
    Playing,
    Paused,
    GameOver
}

public class GameManager : MonoBehaviour {
    public static GameManager Instance { get; private set; }

    [Header("UI Panels (in-game)")]
    public CanvasGroup hudCanvas;
    public CanvasGroup pausePanel;
    public CanvasGroup gameOverPanel;
    public GameObject inventoryPanel;

    [Header("Scene Names")]
    public string mainMenuSceneName = "MainMenu";
    public string optionsMenuSceneName = "OptionsMenu";
    public string gameSceneName = "Level1";

    public event Action<GameState> OnStateChanged;

    public GameState CurrentState { get; private set; } = GameState.None;
    public bool IsPaused => CurrentState == GameState.Paused;
    public bool IsGameOver => CurrentState == GameState.GameOver;

    private bool _isGameOverTriggered = false;

    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy() {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
        FindUIElements();

        // Сбрасываем состояние панелей
        if (gameOverPanel != null) {
            SetPanel(gameOverPanel, false);
            gameOverPanel.alpha = 0f;
        }
        if (pausePanel != null) {
            SetPanel(pausePanel, false);
        }
        if (hudCanvas != null) {
            SetPanel(hudCanvas, true);
        }

        if (scene.name == mainMenuSceneName) {
            SetState(GameState.MainMenu);
        }
        else if (scene.name == gameSceneName) {
            SetState(GameState.Playing);
            _isGameOverTriggered = false; // сброс флага
        }
    }

    private void FindUIElements() {
        var canvases = FindObjectsByType<CanvasGroup>(FindObjectsInactive.Include);
        foreach (var canvas in canvases) {
            if (canvas.name == "HUDCanvas") hudCanvas = canvas;
            if (canvas.name == "PausePanel") pausePanel = canvas;
            if (canvas.name == "GameOverPanel") gameOverPanel = canvas;
        }

        inventoryPanel = FindAnyObjectByType<GameObject>();
        var inventoryPanelObj = GameObject.Find("InventoryPanel");
        if (inventoryPanelObj != null) inventoryPanel = inventoryPanelObj;
    }

    private void Start() {
        if (CurrentState == GameState.None) {
            SetState(GameState.Playing);
        }
    }

    private void Update() {
        if (CurrentState != GameState.Playing && CurrentState != GameState.Paused) return;

        if (Keyboard.current?.escapeKey.wasPressedThisFrame == true ||
            Gamepad.current?.startButton.wasPressedThisFrame == true) {
            TogglePause();
        }

        if (CurrentState == GameState.Playing && Keyboard.current?.tabKey.wasPressedThisFrame == true) {
            ToggleInventory();
        }
    }

    public void SetState(GameState newState) {
        if (CurrentState == newState) return;

        CurrentState = newState;
        ApplyState();
        OnStateChanged?.Invoke(CurrentState);
    }

    private void ApplyState() {
        switch (CurrentState) {
            case GameState.MainMenu:
                Time.timeScale = 1f;
                SetPanel(hudCanvas, false);
                SetPanel(pausePanel, false);
                SetPanel(gameOverPanel, false);
                if (inventoryPanel) inventoryPanel.SetActive(false);
                GameInput.Instance?.DisableInput();
                break;
            case GameState.Playing:
                Time.timeScale = 1f;
                SetPanel(hudCanvas, true);
                SetPanel(pausePanel, false);
                SetPanel(gameOverPanel, false);
                GameInput.Instance?.EnableInput();
                break;
            case GameState.Paused:
                Time.timeScale = 0f;
                SetPanel(hudCanvas, true);
                SetPanel(pausePanel, true);
                SetPanel(gameOverPanel, false);
                if (inventoryPanel) inventoryPanel.SetActive(false);
                GameInput.Instance?.DisableInput();
                break;
            case GameState.GameOver:
                Time.timeScale = 0f;
                SetPanel(hudCanvas, false);
                SetPanel(pausePanel, false);
                SetPanel(gameOverPanel, true);
                if (inventoryPanel) inventoryPanel.SetActive(false);
                GameInput.Instance?.DisableInput();
                break;
        }
    }

    public void ToggleInventory() {
        if (CurrentState != GameState.Playing) return;
        if (inventoryPanel == null) return;

        bool isActive = !inventoryPanel.activeSelf;
        inventoryPanel.SetActive(isActive);
        Time.timeScale = isActive ? 0f : 1f;

        if (isActive) {
            GameInput.Instance?.DisableInput();
        }
        else {
            GameInput.Instance?.EnableInput();
        }
    }

    public void TogglePause() {
        if (IsGameOver) return;
        SetState(IsPaused ? GameState.Playing : GameState.Paused);
    }

    public void PauseGame() => SetState(GameState.Paused);
    public void ResumeGame() => SetState(GameState.Playing);

    public void GameOver() {
        if (_isGameOverTriggered) return;
        _isGameOverTriggered = true;
        Debug.Log("GameOver called!");
        SetState(GameState.GameOver);
    }

    public void RestartLevel() {
        Time.timeScale = 1f;
        _isGameOverTriggered = false;
        _isGameOverTriggered = false;

        // Сбрасываем панели перед загрузкой
        if (gameOverPanel != null) {
            SetPanel(gameOverPanel, false);
            gameOverPanel.alpha = 0f;
        }
        if (pausePanel != null) {
            SetPanel(pausePanel, false);
        }

        SceneManager.LoadScene(gameSceneName);
        SetState(GameState.Playing);
    }

    public void LoadMainMenu() {
        Time.timeScale = 1f;
        _isGameOverTriggered = false;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void LoadOptionsMenu() {
        SceneManager.LoadScene(optionsMenuSceneName);
    }

    public void QuitGame() => Application.Quit();

    private static void SetPanel(CanvasGroup panel, bool visible) {
        if (panel == null) return;
        panel.alpha = visible ? 1f : 0f;
        panel.interactable = visible;
        panel.blocksRaycasts = visible;
    }
}