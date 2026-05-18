using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour {
    [Header("Transition")]
    public CanvasGroup fadePanel;
    public Transform player;
    public float doorX = 3f;
    public float walkSpeed = 2f;
    public float fadeDuration = 1f;

    private void Start() {
        // Блокируем Input на главном меню
        GameInput.Instance.DisableInput();
    }

    public void PlayGame() {
        StartCoroutine(PlayTransition());
    }

    IEnumerator PlayTransition() {
        // 1. Герой идёт к двери
        Vector3 doorPos = new Vector3(doorX, player.position.y, 0);
        while (Vector3.Distance(player.position, doorPos) > 0.1f) {
            player.position = Vector3.MoveTowards(
                player.position,
                doorPos,
                walkSpeed * Time.deltaTime
            );
            yield return null;
        }

        // 2. Скрываем героя
        player.gameObject.SetActive(false);

        // 3. Fade to black
        float t = 0f;
        while (t < fadeDuration) {
            t += Time.deltaTime;
            fadePanel.alpha = Mathf.Clamp01(t / fadeDuration);
            yield return null;
        }

        // 4. Загружаем сцену (Input включится сам т.к. GameInput новый)
        SceneManager.LoadScene("Level1");
    }

    public void OptionsMenu() {
        SceneManager.LoadScene("OptionsMenu");
    }

    public void QuitGame() {
        Application.Quit();
    }
}