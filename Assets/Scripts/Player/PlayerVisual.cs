using System.Collections;
using Other;
using Satyr.Utils;
using UnityEngine;

public class PlayerVisual : MonoBehaviour {
    private Animator _animator;
    private FlashBlink _flashBlink;

    private const string IS_RUNNING = "IsRunning";
    private const string IS_DASHING = "IsDashing";
    private const string IS_DEAD = "IsDead";
    private const string HIT = "Hit";

    private void Awake() {
        _animator = GetComponent<Animator>();
        _flashBlink = GetComponent<FlashBlink>();
    }

    private bool _isDead;

    public bool disableLook = false;

    private void Update() {
        if (_isDead) return;
        if (_animator.GetBool(HIT)) return;

        // Проверяем, активна ли игра
        if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Playing) return;
        if (Player.Instance == null) return;

        _animator.SetBool(IS_RUNNING, Player.Instance.IsRunning());

        if (!disableLook)
            AdjustPlayerFacingDirection();
    }

    public void SetDashing(bool isDashing) => _animator.SetBool(IS_DASHING, isDashing);

    public void SetDead() {
        _isDead = true;
        StopAllCoroutines();
        _animator.Play("dead", 0, 0f);
        if (_flashBlink != null) _flashBlink.StopBlinking();
    }

    public void TriggerHit() => StartCoroutine(HitAnimation());

    private IEnumerator HitAnimation() {
        if (_isDead)
            yield break;
        _animator.Play("taking damage", 0, 0f);
        yield return new WaitForSeconds(0.5f);
        _animator.SetBool(HIT, false);
    }

    private void AdjustPlayerFacingDirection() {
        // Дополнительная проверка для MainMenu
        if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Playing) return;

        float lookX = LookDirectionHelper.GetLookX();

        if (lookX < 0f)
            transform.localScale = new Vector3(-1f, 1f, 1f);
        else if (lookX > 0f)
            transform.localScale = new Vector3(1f, 1f, 1f);
    }
}