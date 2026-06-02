using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

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

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
        if (scene.name == gameSceneName) {
            Player player = FindAnyObjectByType<Player>();
            Debug.Log($"Player found on scene {scene.name}: {player != null}, isDecoration={player?.isDecoration}");
        }

        FindUIElements();
        SubscribeToGameInput();
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
        // Ищем CanvasGroup по имени — быстрее чем FindObjectsByType
        hudCanvas = FindCanvasGroup("HUDCanvas");
        pausePanel = FindCanvasGroup("PausePanel");
        gameOverPanel = FindCanvasGroup("GameOverPanel");
        mainMenuPanel = FindCanvasGroup("MainMenuPanel");

        if (darkOverlay == null)
            darkOverlay = FindCanvasGroup("DarkOverlay");

        inventoryPanel = FindInactiveObjectByName("InventoryPanel");
        optionsPanel = FindInactiveObjectByName("OptionsPanel");
    }

    private CanvasGroup FindCanvasGroup(string name) {
        var go = FindInactiveObjectByName(name);
        return go != null ? go.GetComponent<CanvasGroup>() : null;
    }

    private GameObject FindInactiveObjectByName(string name) {
        var roots = SceneManager.GetActiveScene().GetRootGameObjects();
        foreach (var root in roots) {
            var found = root.transform.Find(name);
            if (found != null) return found.gameObject;
            var child = FindInChildren(root.transform, name);
            if (child != null) return child;
        }
        return null;
    }

    private GameObject FindInChildren(Transform parent, string name) {
        foreach (Transform child in parent) {
            if (child.name == name) return child.gameObject;
            var found = FindInChildren(child, name);
            if (found != null) return found;
        }
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

    private void SubscribeToGameInput() {
        if (GameInput.Instance == null) return;
        GameInput.Instance.OnPause -= TogglePause;
        GameInput.Instance.OnInventory -= ToggleInventory;
        GameInput.Instance.OnPause += TogglePause;
        GameInput.Instance.OnInventory += ToggleInventory;
    }

    private void OnDestroy() {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        if (GameInput.Instance != null) {
            GameInput.Instance.OnPause -= TogglePause;
            GameInput.Instance.OnInventory -= ToggleInventory;
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
                SetPanelImmediate(hudCanvas, false);
                HidePanel(pausePanel, 0.15f);
                HideOptionsPanelIfVisible();
                SetPanelImmediate(gameOverPanel, false);
                if (inventoryPanel) inventoryPanel.SetActive(false);
                SetDarkOverlayVisible(false, 0.15f);
                if (mainMenuPanel != null && !mainMenuPanel.gameObject.activeSelf)
                    SetPanelImmediate(mainMenuPanel, true);
                GameInput.Instance?.DisableInput();
                GameInput.Instance?.EnableUIInput();
                // Установить фокус на первую кнопку главного меню
                StartCoroutine(SetFocusAfterDelay(mainMenuPanel, () => {
                    var mainMenu = FindAnyObjectByType<MainMenu>();
                    if (mainMenu != null) mainMenu.SetFocusToDefaultButton();
                }));
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
                GameInput.Instance?.DisableUIInput();
                // Сбросить выбранный объект в EventSystem
                if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
                break;

            case GameState.Paused:
                Time.timeScale = 0f;
                SetPanelImmediate(hudCanvas, true);
                SetDarkOverlayVisible(true, fadeDuration);
                if (pausePanel != null && pausePanel.alpha < 0.5f) {
                    ShowPanel(pausePanel, fadeDuration);
                    StartCoroutine(SetFocusAfterDelay(pausePanel, () => {
                        var pauseUI = FindAnyObjectByType<PauseMenuUI>();
                        if (pauseUI != null) pauseUI.SetFocus();
                    }));
                }
                SetPanelImmediate(gameOverPanel, false);
                if (inventoryPanel) inventoryPanel.SetActive(false);
                GameInput.Instance?.DisableInput();
                GameInput.Instance?.EnableUIInput();
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
                GameInput.Instance?.EnableUIInput();
                // Установить фокус на кнопку Restart
                var gameOverUI = FindAnyObjectByType<GameOverUI>();
                if (gameOverUI != null) gameOverUI.SetFocus();
                break;
        }
    }

    private IEnumerator SetFocusAfterDelay(CanvasGroup panel, Action onFocus) {
        float timeout = 1f;
        float elapsed = 0f;
        while (panel != null && panel.alpha < 0.99f && elapsed < timeout) {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        yield return null; // один кадр после готовности
        onFocus?.Invoke();
    }

    private void HideOptionsPanelIfVisible() {
        if (optionsPanel == null) return;
        var optCg = optionsPanel.GetComponent<CanvasGroup>();
        if (optCg != null && optCg.gameObject.activeSelf)
            SetPanelImmediate(optCg, false);
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
        Debug.Log($"[Options] optionsPanel={optionsPanel}, state={CurrentState}");
        if (optionsPanel == null) return;
        var optCg = optionsPanel.GetComponent<CanvasGroup>();
        if (optCg == null) return;

        if (optCg.gameObject.activeSelf && optCg.alpha > 0.5f)
            StartCoroutine(CloseOptionsAndRestore(optCg));
        else
            StartCoroutine(OpenOptions(optCg));
    }

    private IEnumerator OpenOptions(CanvasGroup optCg) {
        Debug.Log($"[Options] Opening, optCg={optCg.name}, alpha={optCg.alpha}, active={optCg.gameObject.activeSelf}");
        SetPanelImmediate(optCg, true);
        Debug.Log($"[Options] After SetPanelImmediate, alpha={optCg.alpha}, active={optCg.gameObject.activeSelf}");
        // Скрываем текущую панель
        if (CurrentState == GameState.Paused && pausePanel != null)
            HidePanel(pausePanel, fadeDuration);
        else if (CurrentState == GameState.MainMenu && mainMenuPanel != null)
            HidePanel(mainMenuPanel, fadeDuration);
        else if (CurrentState == GameState.Playing) {
            SetState(GameState.Paused);
            if (pausePanel != null) HidePanel(pausePanel, fadeDuration);
        }

        // Показываем options немедленно (без fade-зависимости)
        SetPanelImmediate(optCg, true);

        // Фокус на первый элемент
        var optCtrl = optCg.GetComponent<OptionsPanelController>();
        if (optCtrl != null && optCtrl.firstSelected != null && EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(optCtrl.firstSelected.gameObject);

        yield break;
    }

    private IEnumerator CloseOptionsAndRestore(CanvasGroup optCg) {
        SetPanelImmediate(optCg, false);

        if (CurrentState == GameState.Paused && pausePanel != null) {
            SetPanelImmediate(pausePanel, true);
            yield return null;
            yield return null; // два кадра для надёжности
            var pauseUI = FindAnyObjectByType<PauseMenuUI>();
            if (pauseUI != null) pauseUI.SetFocus();
        }
        else if (CurrentState == GameState.MainMenu && mainMenuPanel != null) {
            SetPanelImmediate(mainMenuPanel, true);
            SetDarkOverlayVisible(false, fadeDuration);
            yield return null;
            yield return null;
            var mainMenu = FindAnyObjectByType<MainMenu>();
            if (mainMenu != null) mainMenu.SetFocusToDefaultButton();
        }
        else {
            yield return null; // гарантируем что это корутина
        }
    }

    // ─── Пауза ────────────────────────────────────────────────────────────
    public void TogglePause() {
        if (IsGameOver) return;
        // Если options открыт — закрыть его и вернуться в паузу
        if (optionsPanel != null && optionsPanel.activeSelf) {
            var optCg = optionsPanel.GetComponent<CanvasGroup>();
            if (optCg != null) StartCoroutine(CloseOptionsAndRestore(optCg));
            return;
        }
        SetState(IsPaused ? GameState.Playing : GameState.Paused);
    }

    public void PauseGame() => SetState(GameState.Paused);
    public void ResumeGame() => SetState(GameState.Playing);

    public void GameOver() {
        if (_isGameOverTriggered) return;
        _isGameOverTriggered = true;
        SetState(GameState.GameOver);
    }

    public void RestartLevel() {
        Time.timeScale = 1f;
        _isGameOverTriggered = false;
        floorIndex = 0;
        totalScore = 0;
        totalKills = 0;
        OnScoreChanged?.Invoke(totalScore);
        OnKillsChanged?.Invoke(totalKills);
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