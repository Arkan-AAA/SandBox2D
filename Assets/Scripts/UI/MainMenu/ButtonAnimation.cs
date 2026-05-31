using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class ButtonAnimation : MonoBehaviour {
    [Header("Hover Effect")]
    public float hoverScale = 1.1f;
    public float hoverDuration = 0.2f;

    [Header("Click Effect")]
    public float clickScale = 0.95f;
    public float clickDuration = 0.1f;

    [Header("Pulse Effect")]
    public bool enablePulse = true;
    public float pulseScale = 1.05f;
    public float pulseDuration = 1f;

    private Vector3 originalScale;
    private Button button;
    private Coroutine currentAnimation;

    private void Start() {
        originalScale = transform.localScale;
        button = GetComponent<Button>();

        if (enablePulse) {
            StartCoroutine(PulseLoop());
        }
    }

    public void OnHoverEnter() {
        StopCurrentAnimation();
        currentAnimation = StartCoroutine(ScaleTo(hoverScale, hoverDuration));
    }

    public void OnHoverExit() {
        StopCurrentAnimation();
        currentAnimation = StartCoroutine(ScaleTo(originalScale, hoverDuration));
    }

    public void OnClick() {
        StopCurrentAnimation();
        StartCoroutine(ClickAnimation());
    }

    private IEnumerator ScaleTo(Vector3 targetScale, float duration) {
        Vector3 startScale = transform.localScale;
        float elapsed = 0f;

        while (elapsed < duration) {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            transform.localScale = Vector3.Lerp(startScale, targetScale, t);
            yield return null;
        }

        transform.localScale = targetScale;
    }

    private IEnumerator ScaleTo(float targetScale, float duration) {
        yield return ScaleTo(new Vector3(targetScale, targetScale, targetScale), duration);
    }

    private IEnumerator ClickAnimation() {
        yield return ScaleTo(clickScale, clickDuration);
        yield return ScaleTo(originalScale, clickDuration);
    }

    private void StopCurrentAnimation() {
        if (currentAnimation != null) {
            StopCoroutine(currentAnimation);
        }
    }

    private IEnumerator PulseLoop() {
        while (true) {
            yield return ScaleTo(pulseScale, pulseDuration / 2f);
            yield return ScaleTo(originalScale, pulseDuration / 2f);
            yield return new WaitForSeconds(0.5f);
        }
    }
}