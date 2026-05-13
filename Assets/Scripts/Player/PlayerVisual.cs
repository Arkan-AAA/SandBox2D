using System.Collections;
using Misc;
using UnityEngine;
using Satyr.Utils;

public class PlayerVisual : MonoBehaviour
{
    private Animator _animator;
    private FlashBlink _flashBlink;

    private const string IS_RUNNING = "IsRunning";
    private const string IS_DASHING = "IsDashing";
    private const string IS_DEAD = "IsDead";
    private const string HIT = "Hit";

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _flashBlink = GetComponent<FlashBlink>();
    }

    private bool _isDead;

    private void Update()
    {
        if (_isDead) return;
        if (_animator.GetBool(HIT)) return;
        _animator.SetBool(IS_RUNNING, Player.Instance.IsRunning());
        AdjustPlayerFacingDirection();
    }

    public void SetDashing(bool isDashing) => _animator.SetBool(IS_DASHING, isDashing);

    public void SetDead()
    {
        _isDead = true;
        StopAllCoroutines();
        _animator.Play("dead", 0, 0f);
        _flashBlink.StopBlinking();
    }

    public void TriggerHit() => StartCoroutine(HitAnimation());

    private IEnumerator HitAnimation()
    {
        if (_isDead) yield break;
        _animator.Play("taking damage", 0, 0f);
        yield return new WaitForSeconds(0.5f);
        _animator.SetBool(HIT, false);
    }

    private void AdjustPlayerFacingDirection()
    {
        float lookX = LookDirectionHelper.GetLookX();

        if (lookX < 0f)
            transform.localScale = new Vector3(-1f, 1f, 1f);
        else if (lookX > 0f)
            transform.localScale = new Vector3(1f, 1f, 1f);
    }
}
