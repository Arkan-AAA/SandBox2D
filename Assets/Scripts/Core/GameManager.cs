using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public enum GameState { None, MainMenu, Playing, Paused, GameOver }

public class GameManager : MonoBehaviour {
    public static GameManager Instance { get; private set; }

    [Header("UI Panels")]
    public CanvasGroup hudCanvas;
    public CanvasGroup pausePanel;
    public CanvasGroup gameOverPanel;
    public GameObject inventoryPanel;
    public GameObject optionsPanel;
    public CanvasGroup darkOverlay;
    public CanvasGroup mainMenuPanel;
    public float fadeDuration = 0.2f;

    [Header("Scene Names")]
    public string mainMenuSceneName = "MainMenu";
    public string gameSceneName = "Level1";

    [Header("Progress")]
    public int floorIndex = 0;
    public int totalScore = 0;
    public int totalKills = 0;

    public event Action<GameState> OnStateChanged;
    public event Action<int> OnScoreChanged;
    public event Action<int> OnKillsChanged;

    public GameState CurrentState { get; private set; } = GameState.None;
    public bool IsPaused => CurrentState == GameState.Paused;
    public bool IsGameOver => CurrentState == GameState.GameOver;

    private bool _isGameOverTriggered = false;
    private Coroutine _activeOverlayFade;
    private Dictionary<CanvasGroup, Coroutine> _activePanelFades = new Dictionary<CanvasGroup, Coroutine>();

    private void Awake() {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy() => SceneManager.sceneLoaded -= OnSceneLoaded;

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
        FindUIElements();
        if (gameOverPanel != null) SetPanelImmediate(gameOverPanel, false);
        if (pausePanel != null) SetPanelImmediate(pausePanel, false);
        if (hudCanvas != null) SetPanelImmediate(hudCanvas, scene.name != mainMenuSceneName);
        if (darkOverlay != null) {
            darkOverlay.alpha = 0f;
            darkOverlay.gameObject.SetActive(false);
        }

        if (scene.name == mainMenuSceneName) {
            SetState(GameState.MainMenu);
            if (mainMenuPanel != null) SetPanelImmediate(mainMenuPanel, true);
        }
        else if (scene.name == gameSceneName) {
            SetState(GameState.Playing);
            _isGameOverTriggered = false;
            if (mainMenuPanel != null) SetPanelImmediate(mainMenuPanel, false);
        }
    }

    private void FindUIElements() {
        var canvases = FindObjectsByType<CanvasGroup>(FindObjectsInactive.Include);
        foreach (var c in canvases) {
            if (c.name == "HUDCanvas") hudCanvas = c;
            if (c.name == "PausePanel") pausePanel = c;
            if (c.name == "GameOverPanel") gameOverPanel = c;
            if (c.name == "MainMenuPanel") mainMenuPanel = c;
        }
        inventoryPanel = FindInactiveObjectByName("InventoryPanel");
        optionsPanel = FindInactiveObjectByName("OptionsPanel");
        if (darkOverlay == null)
            darkOverlay = GameObject.Find("DarkOverlay")?.GetComponent<CanvasGroup>();
        if (mainMenuPanel == null)
            mainMenuPanel = GameObject.Find("MainMenuPanel")?.GetComponent<CanvasGroup>();
    }

    private GameObject FindInactiveObjectByName(string name) {
        var allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
        foreach (var go in allObjects)
            if (go.name == name && go.scene.IsValid())
                return go;
        return null;
    }

    // ─── Управление оверлеем (затемнение) ───────────────────────────────
    private void SetDarkOverlayVisible(bool visible, float duration = -1) {
        if (darkOverlay == null) return;
        if (duration < 0) duration = fadeDuration;

        if (_activeOverlayFade != null)
            StopCoroutine(_activeOverlayFade);

        darkOverlay.gameObject.SetActive(true);
        _activeOverlayFade = StartCoroutine(FadeOverlay(visible, duration));
    }

    private IEnumerator FadeOverlay(bool visible, float duration) {
        float startAlpha = darkOverlay.alpha;
        float targetAlpha = visible ? 0.7f : 0f;
        float elapsed = 0f;

        while (elapsed < duration) {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;
            darkOverlay.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
            yield return null;
        }

        darkOverlay.alpha = targetAlpha;
        if (!visible) darkOverlay.gameObject.SetActive(false);
        _activeOverlayFade = null;
    }

    // ─── Плавное управление панелями ─────────────────────────────────────
    public void ShowPanel(CanvasGroup panel, float duration = -1) {
        if (panel == null) return;
        if (duration < 0) duration = fadeDuration;

        if (_activePanelFades.ContainsKey(panel) && _activePanelFades[panel] != null)
            StopCoroutine(_activePanelFades[panel]);

        panel.gameObject.SetActive(true);
        panel.alpha = 0f;
        panel.interactable = false;
        panel.blocksRaycasts = false;

        _activePanelFades[panel] = StartCoroutine(FadePanel(panel, 0f, 1f, duration, true));
    }

    public void HidePanel(CanvasGroup panel, float duration = -1) {
        if (panel == null) return;
        if (duration < 0) duration = fadeDuration;

        if (_activePanelFades.ContainsKey(panel) && _activePanelFades[panel] != null)
            StopCoroutine(_activePanelFades[panel]);

        _activePanelFades[panel] = StartCoroutine(FadePanel(panel, panel.alpha, 0f, duration, false));
    }

    private IEnumerator FadePanel(CanvasGroup panel, float startAlpha, float targetAlpha, float duration, bool enableOnComplete) {
        float elapsed = 0f;
        while (elapsed < duration) {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;
            panel.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
            yield return null;
        }

        panel.alpha = targetAlpha;
        if (enableOnComplete) {
            panel.interactable = true;
            panel.blocksRaycasts = true;
        }
        else {
            panel.interactable = false;
            panel.blocksRaycasts = false;
            panel.gameObject.SetActive(false);
        }
        _activePanelFades.Remove(panel);
    }

    private void SetPanelImmediate(CanvasGroup panel, bool visible) {
        if (panel == null) return;
        panel.alpha = visible ? 1f : 0f;
        panel.interactable = visible;
        panel.blocksRaycasts = visible;
        panel.gameObject.SetActive(visible);
    }

    // ─── Состояния игры ─────────────────────────────────────────────────
    private void Start() {
        if (CurrentState == GameState.None) SetState(GameState.Playing);
    }

    private void Update() {
        if (CurrentState != GameState.Playing && CurrentState != GameState.Paused) return;
        if (Keyboard.current?.escapeKey.wasPressedThisFrame == true || Gamepad.current?.startButton.wasPressedThisFrame == true)
            TogglePause();
        if (CurrentState == GameState.Playing && Keyboard.current?.tabKey.wasPressedThisFrame == true)
            ToggleInventory();
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
                SetPanelImmediate(hudCanvas, false);
                HidePanel(pausePanel, 0.15f);
                HideOptionsPanelIfVisible();
                SetPanelImmediate(gameOverPanel, false);
                if (inventoryPanel) inventoryPanel.SetActive(false);
                SetDarkOverlayVisible(false, 0.15f);
                if (mainMenuPanel != null && !mainMenuPanel.gameObject.activeSelf)
                    SetPanelImmediate(mainMenuPanel, true);
                GameInput.Instance?.DisableInput();
                break;

            case GameState.Playing:
                Time.timeScale = 1f;
                SetPanelImmediate(hudCanvas, true);
                HidePanel(pausePanel, 0.15f);
                HideOptionsPanelIfVisible();
                SetPanelImmediate(gameOverPanel, false);
                if (inventoryPanel) inventoryPanel.SetActive(false);
                SetDarkOverlayVisible(false, 0.15f);
                if (mainMenuPanel != null && mainMenuPanel.gameObject.activeSelf)
                    SetPanelImmediate(mainMenuPanel, false);
                GameInput.Instance?.EnableInput();
                break;

            case GameState.Paused:
                Time.timeScale = 0f;
                SetPanelImmediate(hudCanvas, true);
                SetDarkOverlayVisible(true, fadeDuration);
                if (pausePanel != null && pausePanel.alpha < 0.5f)
                    ShowPanel(pausePanel, fadeDuration);
                SetPanelImmediate(gameOverPanel, false);
                if (inventoryPanel) inventoryPanel.SetActive(false);
                GameInput.Instance?.DisableInput();
                break;

            case GameState.GameOver:
                Time.timeScale = 0f;
                SetPanelImmediate(hudCanvas, false);
                HidePanel(pausePanel, 0.15f);
                HideOptionsPanelIfVisible();
                SetPanelImmediate(gameOverPanel, true);
                if (inventoryPanel) inventoryPanel.SetActive(false);
                SetDarkOverlayVisible(false, 0.15f);
                GameInput.Instance?.DisableInput();
                break;
        }
    }

    private void HideOptionsPanelIfVisible() {
        if (optionsPanel == null) return;
        CanvasGroup optCg = optionsPanel.GetComponent<CanvasGroup>();
        if (optCg != null && optCg.gameObject.activeSelf)
            HidePanel(optCg, fadeDuration);
    }

    // ─── Инвентарь ───────────────────────────────────────────────────────
    public void ToggleInventory() {
        if (CurrentState != GameState.Playing || inventoryPanel == null) return;
        bool active = !inventoryPanel.activeSelf;
        inventoryPanel.SetActive(active);
        Time.timeScale = active ? 0f : 1f;
        if (active) GameInput.Instance?.DisableInput();
        else GameInput.Instance?.EnableInput();
    }

    // ─── Панель настроек (универсально) ───────────────────────────────────
    public void ToggleOptions() {
        Debug.Log("ToggleOptions called");
        if (optionsPanel == null) {
            Debug.LogError("optionsPanel is NULL! Check assignment.");
            return;
        }
        Debug.Log("optionsPanel found: " + optionsPanel.name);
        CanvasGroup optCg = optionsPanel.GetComponent<CanvasGroup>();
        if (optCg == null) {
            Debug.LogError("CanvasGroup missing on OptionsPanel");
            return;
        }
        Debug.Log("CanvasGroup OK, alpha=" + optCg.alpha + ", activeSelf=" + optCg.gameObject.activeSelf);

        bool isOptionsVisible = optCg.alpha > 0.5f && optCg.gameObject.activeSelf;

        if (isOptionsVisible)
            StartCoroutine(CloseOptionsAndRestore(optCg));
        else
            StartCoroutine(OpenOptionsAndHidePrevious(optCg));
    }

    private IEnumerator OpenOptionsAndHidePrevious(CanvasGroup optCg) {
        if (CurrentState == GameState.Paused) {
            if (pausePanel != null && pausePanel.alpha > 0.5f)
                HidePanel(pausePanel, fadeDuration);
        }
        else if (CurrentState == GameState.MainMenu) {
            if (mainMenuPanel != null && mainMenuPanel.alpha > 0.5f) {
                SetDarkOverlayVisible(true, fadeDuration);
                HidePanel(mainMenuPanel, fadeDuration);
            }
        }
        else if (CurrentState == GameState.Playing) {
            SetState(GameState.Paused);
            yield return new WaitForSecondsRealtime(0.05f);
            if (pausePanel != null && pausePanel.alpha > 0.5f)
                HidePanel(pausePanel, fadeDuration);
        }
        ShowPanel(optCg, fadeDuration);
    }

    private IEnumerator CloseOptionsAndRestore(CanvasGroup optCg) {
        HidePanel(optCg, fadeDuration);
        yield return new WaitForSecondsRealtime(0.1f);

        if (CurrentState == GameState.Paused) {
            if (pausePanel != null && pausePanel.alpha < 0.5f)
                ShowPanel(pausePanel, fadeDuration);
        }
        else if (CurrentState == GameState.MainMenu) {
            if (mainMenuPanel != null && mainMenuPanel.alpha < 0.5f) {
                ShowPanel(mainMenuPanel, fadeDuration);
                SetDarkOverlayVisible(false, fadeDuration);
            }
        }
    }

    // ─── Пауза ────────────────────────────────────────────────────────────
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
        SetPanelImmediate(gameOverPanel, false);
        SetPanelImmediate(pausePanel, false);
        SceneManager.LoadScene(gameSceneName);
    }

    public void LoadMainMenu() {
        Time.timeScale = 1f;
        _isGameOverTriggered = false;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void LoadGameScene() {
        Time.timeScale = 1f;
        _isGameOverTriggered = false;
        SceneManager.LoadScene(gameSceneName);
    }

    public void QuitGame() => Application.Quit();

    public void AddScore(int amount) {
        totalScore += amount;
        OnScoreChanged?.Invoke(totalScore);
    }

    public void AddKill() {
        totalKills++;
        OnKillsChanged?.Invoke(totalKills);
    }

    private void OnApplicationQuit() {
        var progress = new PlayerProgress {
            maxFloor = floorIndex,
            totalScore = totalScore,
            totalKills = totalKills
        };
        SaveSystem.SaveGame(progress);
    }
}