using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Audio;

public class MainMenu : MonoBehaviour {
    [Header("UI Group (общая панель: тень, текст, кнопки)")]
    public CanvasGroup mainMenuGroup;
    public float uiFadeInDuration = 1f;

    [Header("Transition (ходьба к двери)")]
    public Transform player;
    public Transform doorPoint;
    public float walkSpeed = 3f;
    public CanvasGroup fadePanel;
    public float fadeDuration = 1f;

    [Header("Player Animation")]
    public Animator playerAnimator;

    [Header("Music")]
    public PlaylistSO menuPlaylist;   // ← используем готовый PlaylistSO (AudioClip[])

    [Header("Footsteps Sound")]
    public AudioClip footstepsClip;
    [Range(0f, 1f)] public float footstepsVolume = 0.5f;

    private bool _isTransitioning = false;
    private bool _fadeComplete = false;
    private Image _fadeImage;
    private AudioSource _footstepsSource;

    private void Start() {
        DisableCombatComponents();
        SetupFadePanel();
        if (player == null) FindPlayer();
        if (GameInput.Instance != null) GameInput.Instance.DisableInput();
        if (GameManager.Instance != null) GameManager.Instance.SetState(GameState.MainMenu);

        if (mainMenuGroup != null) {
            mainMenuGroup.alpha = 0f;
            mainMenuGroup.interactable = true;
            mainMenuGroup.blocksRaycasts = true;
        }

        StartCoroutine(FadeInMainMenu());
        StartCoroutine(PlayMenuMusic());
    }

    private IEnumerator FadeInMainMenu() {
        if (mainMenuGroup == null) yield break;
        float t = 0f;
        while (t < uiFadeInDuration) {
            t += Time.deltaTime;
            mainMenuGroup.alpha = Mathf.Clamp01(t / uiFadeInDuration);
            yield return null;
        }
        mainMenuGroup.alpha = 1f;
    }

    private IEnumerator PlayMenuMusic() {
        yield return null; // ждём инициализации AudioManager
        if (AudioManager.Instance != null && menuPlaylist != null)
            AudioManager.Instance.StartPlaylist(menuPlaylist);
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

    private void FindPlayer() {
        var playerObject = FindAnyObjectByType<Player>();
        if (playerObject != null) {
            player = playerObject.transform;
            player.gameObject.SetActive(true);
            player.position = new Vector3(-5f, player.position.y, 0);

            if (playerAnimator == null)
                playerAnimator = player.GetComponent<Animator>();

            if (playerAnimator != null) {
                playerAnimator.SetBool("IsRunning", false);
                playerAnimator.Play("idle", 0, 0f);
            }
        }
    }

    private void DisableCombatComponents() {
        var activeWeapon = FindAnyObjectByType<ActiveWeapon>();
        if (activeWeapon != null) activeWeapon.enabled = false;

        var weaponInput = FindAnyObjectByType<WeaponInput>();
        if (weaponInput != null) weaponInput.enabled = false;

        var weaponInventory = FindAnyObjectByType<WeaponInventory>();
        if (weaponInventory != null) weaponInventory.enabled = false;

        var swords = FindObjectsByType<Sword>(FindObjectsInactive.Include);
        foreach (var sword in swords) sword.enabled = false;
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

        // Плавно скрываем UI главного меню
        if (mainMenuGroup != null) {
            float t = 0f;
            float startAlpha = mainMenuGroup.alpha;
            while (t < 0.2f) {
                t += Time.deltaTime;
                mainMenuGroup.alpha = Mathf.Lerp(startAlpha, 0f, t / 0.2f);
                yield return null;
            }
            mainMenuGroup.alpha = 0f;
            mainMenuGroup.interactable = false;
        }

        if (player != null) {
            player.gameObject.SetActive(true);
            player.position = new Vector3(-5f, player.position.y, 0);

            if (playerAnimator != null)
                playerAnimator.SetBool("IsRunning", true);

            // Воспроизводим звук шагов через временный AudioSource
            if (footstepsClip != null) {
                _footstepsSource = gameObject.AddComponent<AudioSource>();
                _footstepsSource.clip = footstepsClip;
                _footstepsSource.volume = footstepsVolume;
                _footstepsSource.loop = true;
                _footstepsSource.Play();
            }

            float doorX = doorPoint != null ? doorPoint.position.x : 5f;
            float startX = player.position.x;
            float duration = Mathf.Abs(doorX - startX) / walkSpeed;

            StartCoroutine(FadeToBlack());

            float elapsed = 0f;
            while (elapsed < duration) {
                elapsed += Time.deltaTime;
                float progress = elapsed / duration;
                float x = Mathf.Lerp(startX, doorX, progress);
                player.position = new Vector3(x, player.position.y, 0);
                yield return null;
            }

            // Останавливаем и удаляем временный AudioSource
            if (_footstepsSource != null) {
                _footstepsSource.Stop();
                Destroy(_footstepsSource);
            }

            if (playerAnimator != null)
                playerAnimator.SetBool("IsRunning", false);
        }
        else {
            StartCoroutine(FadeToBlack());
        }

        while (!_fadeComplete) yield return null;

        // Загружаем игровую сцену
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
            t += Time.deltaTime;
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