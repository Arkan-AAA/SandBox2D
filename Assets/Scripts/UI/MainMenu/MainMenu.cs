using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using Audio;

public class MainMenu : MonoBehaviour {
    [Header("UI Group")]
    public CanvasGroup mainMenuGroup;
    public float uiFadeInDuration = 1f;

    [Header("Transition")]
    public Transform player;
    public Transform doorPoint;
    public float walkSpeed = 3f;
    public CanvasGroup fadePanel;
    public float fadeDuration = 1f;

    [Header("Player Animation")]
    public Animator playerAnimator;

    [Header("Music")]
    public PlaylistSO menuPlaylist;

    [Header("Footsteps")]
    public AudioClip footstepsClip;
    [Range(0f, 1f)] public float footstepsVolume = 0.5f;

    private bool _isTransitioning = false;
    private bool _fadeComplete = false;
    private Image _fadeImage;
    private AudioSource _footstepsSource;
    private Button _firstButton;

    private void Start() {
        DisableCombatComponents();
        SetupFadePanel();
        // player назначается только вручную в инспекторе — не ищем через FindAnyObjectByType
        if (GameInput.Instance != null) GameInput.Instance.DisableInput();

        if (mainMenuGroup != null) {
            mainMenuGroup.alpha = 0f;
            mainMenuGroup.interactable = true;
            mainMenuGroup.blocksRaycasts = true;
        }

        // Найти первую кнопку в главном меню (например, "PlayButton")
        _firstButton = GetComponentInChildren<Button>();
        if (_firstButton == null) {
            Button[] btns = GetComponentsInChildren<Button>();
            if (btns.Length > 0) _firstButton = btns[0];
        }

        StartCoroutine(FadeInMainMenu());
        StartCoroutine(PlayMenuMusic());
    }

    private IEnumerator FadeInMainMenu() {
        if (mainMenuGroup == null) yield break;
        float t = 0f;
        while (t < uiFadeInDuration) {
            t += Time.unscaledDeltaTime;
            mainMenuGroup.alpha = Mathf.Clamp01(t / uiFadeInDuration);
            yield return null;
        }
        mainMenuGroup.alpha = 1f;
        SetFocusToDefaultButton();
    }

    private IEnumerator PlayMenuMusic() {
        yield return null;
        if (AudioManager.Instance != null && menuPlaylist != null)
            AudioManager.Instance.StartPlaylist(menuPlaylist);
    }

    /// <summary>Устанавливает фокус на кнопку по умолчанию (первую в меню).</summary>
    public void SetFocusToDefaultButton() {
        if (_firstButton != null && EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(_firstButton.gameObject);
        else {
            // fallback: найти любую кнопку
            Button anyButton = GetComponentInChildren<Button>();
            if (anyButton != null && EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(anyButton.gameObject);
        }
    }

    private void SetupFadePanel() {
        if (fadePanel == null) {
            GameObject fadeObj = new GameObject("FadePanel");
            fadeObj.transform.SetParent(GameObject.Find("MainMenuCanvas")?.transform ?? transform);

            var rect = fadeObj.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            _fadeImage = fadeObj.AddComponent<Image>();
            _fadeImage.color = new Color(0, 0, 0, 0);
            _fadeImage.raycastTarget = false;

            fadePanel = fadeObj.AddComponent<CanvasGroup>();
            fadePanel.alpha = 0f;
            fadePanel.interactable = false;
            fadePanel.blocksRaycasts = false;
        }
        else {
            _fadeImage = fadePanel.GetComponent<Image>();
            if (_fadeImage == null) {
                _fadeImage = fadePanel.gameObject.AddComponent<Image>();
                _fadeImage.color = new Color(0, 0, 0, 0);
                _fadeImage.raycastTarget = false;
            }
            fadePanel.alpha = 0f;
            fadePanel.interactable = false;
            fadePanel.blocksRaycasts = false;
        }
        fadePanel.transform.SetAsLastSibling();
    }

    private void DisableCombatComponents() {
        // Отключаем компоненты только на декоративном персонаже в MainMenu (назначен через player)
        if (player == null) return;
        if (playerAnimator == null)
            playerAnimator = player.GetComponent<Animator>();
        if (playerAnimator != null) {
            playerAnimator.SetBool("IsRunning", false);
            playerAnimator.Play("idle", 0, 0f);
        }
    }

    // ─── Кнопки главного меню ─────────────────────────────────────────
    public void PlayGame() {
        if (_isTransitioning) return;
        StartCoroutine(PlayTransition());
    }

    public void OptionsMenu() {
        Debug.Log("OptionsMenu button clicked");
        if (GameManager.Instance != null) {
            Debug.Log("GameManager found, calling ToggleOptions");
            GameManager.Instance.ToggleOptions();
        }
        else {
            Debug.LogError("GameManager instance is null!");
        }
    }

    public void QuitGame() {
        GameManager.Instance?.QuitGame();
    }

    // ─── Анимация перехода к игре (ходьба + затемнение) ───────────────
    private IEnumerator PlayTransition() {
        _isTransitioning = true;
        _fadeComplete = false;
        Debug.Log("[Transition] START");

        if (mainMenuGroup != null) {
            float t = 0f;
            float startAlpha = mainMenuGroup.alpha;
            while (t < 0.2f) {
                t += Time.unscaledDeltaTime;
                mainMenuGroup.alpha = Mathf.Lerp(startAlpha, 0f, t / 0.2f);
                yield return null;
            }
            mainMenuGroup.alpha = 0f;
            mainMenuGroup.interactable = false;
        }
        Debug.Log("[Transition] UI hidden");

        if (player != null) {
            Debug.Log($"[Transition] Player found, doorPoint={doorPoint}, walkSpeed={walkSpeed}");
            float doorX = doorPoint != null ? doorPoint.position.x : player.position.x + 10f;
            float startX = player.position.x;
            float duration = Mathf.Abs(doorX - startX) / walkSpeed;
            Debug.Log($"[Transition] doorX={doorX}, startX={startX}, duration={duration}");

            StartCoroutine(FadeToBlack());
            Debug.Log("[Transition] FadeToBlack started");

            float elapsed = 0f;
            while (elapsed < duration) {
                elapsed += Time.unscaledDeltaTime;
                player.position = new Vector3(Mathf.Lerp(startX, doorX, elapsed / duration), player.position.y, 0);
                yield return null;
            }
            Debug.Log("[Transition] Walk complete");
        }
        else {
            Debug.Log("[Transition] No player, starting FadeToBlack directly");
            StartCoroutine(FadeToBlack());
        }

        Debug.Log($"[Transition] Waiting for fade, _fadeComplete={_fadeComplete}");
        while (!_fadeComplete) yield return null;
        Debug.Log("[Transition] Fade complete, loading scene");

        if (player != null) Destroy(player.gameObject);

        if (GameManager.Instance != null)
            GameManager.Instance.LoadGameScene();
        else
            SceneManager.LoadScene("Level1");
    }

    private IEnumerator FadeToBlack() {
        if (fadePanel != null) {
            fadePanel.interactable = true;
            fadePanel.blocksRaycasts = true;
        }

        if (_fadeImage != null)
            _fadeImage.color = new Color(0, 0, 0, 0);

        float t = 0f;
        while (t < fadeDuration) {
            t += Time.unscaledDeltaTime;
            float alpha = Mathf.Clamp01(t / fadeDuration);
            if (fadePanel != null) fadePanel.alpha = alpha;
            if (_fadeImage != null) _fadeImage.color = new Color(0, 0, 0, alpha);
            yield return null;
        }

        if (fadePanel != null) fadePanel.alpha = 1f;
        if (_fadeImage != null) _fadeImage.color = new Color(0, 0, 0, 1);
        _fadeComplete = true;
    }
}