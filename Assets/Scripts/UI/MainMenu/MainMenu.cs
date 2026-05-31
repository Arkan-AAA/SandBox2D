using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MainMenu : MonoBehaviour {
    [Header("Transition")]
    public CanvasGroup fadePanel;
    public Transform player;
    public Transform doorPoint;
    public float walkSpeed = 3f;
    public float fadeDuration = 1f;

    [Header("UI Animation")]
    public CanvasGroup titleGroup;
    public CanvasGroup buttonsGroup;
    public float uiFadeInDuration = 1f;

    [Header("Player Animation")]
    public SpriteRenderer playerSprite;
    public Animator playerAnimator;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip buttonHoverSound;
    public AudioClip buttonClickSound;
    public AudioClip footstepsSound;

    private bool _isTransitioning = false;
    private bool _fadeComplete = false;
    private Image _fadeImage;

    private void Start() {
        // Находим компоненты
        DisableCombatComponents();
        SetupFadePanel();
        FindPlayer();
        SetupInput();
        SetupUI();
        StartCoroutine(FadeInUI());

        if (GameManager.Instance != null) {
            GameManager.Instance.SetState(GameState.MainMenu);
        }
    }

    private void SetupFadePanel() {
        if (fadePanel == null) {
            // Создаём FadePanel если его нет
            GameObject fadeObj = new GameObject("FadePanel");
            fadeObj.transform.SetParent(GameObject.Find("MainMenuCanvas")?.transform ?? transform);

            // Добавляем RectTransform
            var rect = fadeObj.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            // Добавляем Image
            _fadeImage = fadeObj.AddComponent<Image>();
            _fadeImage.color = new Color(0, 0, 0, 0);
            _fadeImage.raycastTarget = false;

            // Добавляем CanvasGroup
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

        // Убеждаемся, что FadePanel последний в иерархии
        fadePanel.transform.SetAsLastSibling();
    }

    private void FindPlayer() {
        var playerObject = FindAnyObjectByType<Player>();
        if (playerObject != null) {
            player = playerObject.transform;
            player.gameObject.SetActive(true);
            player.position = new Vector3(-5f, player.position.y, 0);

            if (playerAnimator == null) {
                playerAnimator = player.GetComponent<Animator>();
            }

            if (playerAnimator != null) {
                playerAnimator.SetBool("IsRunning", false);
                playerAnimator.Play("idle", 0, 0f);
            }
        }
    }

    private void SetupInput() {
        if (GameInput.Instance != null) {
            GameInput.Instance.DisableInput();
        }
    }

    private void SetupUI() {
        if (titleGroup != null) titleGroup.alpha = 0f;
        if (buttonsGroup != null) buttonsGroup.alpha = 0f;
    }

    private void DisableCombatComponents() {
        var activeWeapon = FindAnyObjectByType<ActiveWeapon>();
        if (activeWeapon != null) activeWeapon.enabled = false;

        var weaponInput = FindAnyObjectByType<WeaponInput>();
        if (weaponInput != null) weaponInput.enabled = false;

        var weaponInventory = FindAnyObjectByType<WeaponInventory>();
        if (weaponInventory != null) weaponInventory.enabled = false;

        var swords = FindObjectsByType<Sword>(FindObjectsInactive.Include);
        foreach (var sword in swords) {
            sword.enabled = false;
        }
    }

    private IEnumerator FadeInUI() {
        yield return new WaitForSeconds(0.3f);

        float t = 0f;
        while (t < uiFadeInDuration) {
            t += Time.deltaTime;
            float alpha = Mathf.Clamp01(t / uiFadeInDuration);
            if (titleGroup != null) titleGroup.alpha = alpha;
            if (buttonsGroup != null) buttonsGroup.alpha = alpha;
            yield return null;
        }
    }

    public void PlayGame() {
        if (_isTransitioning) return;
        if (audioSource != null && buttonClickSound != null) {
            audioSource.PlayOneShot(buttonClickSound);
        }
        StartCoroutine(PlayTransition());
    }

    public void OnButtonHover() {
        if (audioSource != null && buttonHoverSound != null) {
            audioSource.PlayOneShot(buttonHoverSound);
        }
    }

    public void OnButtonClick() {
        if (audioSource != null && buttonClickSound != null) {
            audioSource.PlayOneShot(buttonClickSound);
        }
    }

    IEnumerator PlayTransition() {
        _isTransitioning = true;
        _fadeComplete = false;

        // 1. Мгновенно скрываем UI
        if (titleGroup != null) titleGroup.alpha = 0f;
        if (buttonsGroup != null) buttonsGroup.alpha = 0f;

        // 2. Активируем игрока и запускаем анимацию ходьбы
        if (player != null) {
            player.gameObject.SetActive(true);
            player.position = new Vector3(-5f, player.position.y, 0);

            if (playerAnimator != null) {
                playerAnimator.SetBool("IsRunning", true);
            }

            if (audioSource != null && footstepsSound != null) {
                audioSource.loop = true;
                audioSource.clip = footstepsSound;
                audioSource.Play();
            }

            // Идём к двери
            float doorX = doorPoint != null ? doorPoint.position.x : 5f;
            Vector3 doorPos = new Vector3(doorX, player.position.y, 0);
            float startX = player.position.x;
            float distance = Mathf.Abs(doorX - startX);
            float duration = distance / walkSpeed;

            // Запускаем затемнение параллельно с ходьбой
            StartCoroutine(FadeToBlack());

            // Параллельная ходьба
            float elapsed = 0f;
            while (elapsed < duration) {
                elapsed += Time.deltaTime;
                float progress = elapsed / duration;
                float x = Mathf.Lerp(startX, doorX, progress);
                player.position = new Vector3(x, player.position.y, 0);
                yield return null;
            }

            // Останавливаем звук шагов
            if (audioSource != null && footstepsSound != null) {
                audioSource.loop = false;
                audioSource.Stop();
            }

            if (playerAnimator != null) {
                playerAnimator.SetBool("IsRunning", false);
            }
        }
        else {
            // Если нет игрока, просто запускаем затемнение
            StartCoroutine(FadeToBlack());
        }

        // 3. Ждём завершения затемнения
        while (!_fadeComplete) {
            yield return null;
        }

        // 4. Загружаем уровень
        GameManager.Instance?.RestartLevel();
    }

    private IEnumerator FadeToBlack() {
        // Включаем блокировку
        if (fadePanel != null) {
            fadePanel.interactable = true;
            fadePanel.blocksRaycasts = true;
        }

        // Убеждаемся что Image чёрный
        if (_fadeImage != null) {
            _fadeImage.color = new Color(0, 0, 0, 0);
        }

        float fadeT = 0f;
        while (fadeT < fadeDuration) {
            fadeT += Time.deltaTime;
            float alpha = Mathf.Clamp01(fadeT / fadeDuration);

            if (fadePanel != null) {
                fadePanel.alpha = alpha;
            }

            if (_fadeImage != null) {
                _fadeImage.color = new Color(0, 0, 0, alpha);
            }

            yield return null;
        }

        // Гарантируем полностью чёрный экран
        if (fadePanel != null) {
            fadePanel.alpha = 1f;
        }
        if (_fadeImage != null) {
            _fadeImage.color = new Color(0, 0, 0, 1);
        }

        _fadeComplete = true;
    }

    public void OptionsMenu() {
        GameManager.Instance?.LoadOptionsMenu();
    }

    public void QuitGame() {
        GameManager.Instance?.QuitGame();
    }
}