using System;
using UnityEngine;

public class SwordSlashVisual : MonoBehaviour {
    [SerializeField]
    private Sword sword;

    [SerializeField]
    private float attackCooldown = 0.3f;

    private static readonly int AttackHash = Animator.StringToHash("Attack");
    private Animator animator;
    private float lastAttackTime;
    private bool isAttackHeld;

    private void Awake() {
        animator = GetComponent<Animator>();
    }

    private void Start() {
        animator.ResetTrigger(AttackHash);
        sword.OnSwordSwing += Sword_OnSwordSwing;
        GameInput.Instance.OnPlayerAttackHeld += OnAttackHeld;
        GameInput.Instance.OnPlayerAttackReleased += OnAttackReleased;
    }

    private void OnAttackHeld() => isAttackHeld = true;

    private void OnAttackReleased() => isAttackHeld = false;

    private void OnDestroy() {
        sword.OnSwordSwing -= Sword_OnSwordSwing;
        if (GameInput.Instance != null) {
            GameInput.Instance.OnPlayerAttackHeld -= OnAttackHeld;
            GameInput.Instance.OnPlayerAttackReleased -= OnAttackReleased;
        }
    }

    private void Update() {
        if (isAttackHeld && Time.time - lastAttackTime >= attackCooldown) {
            TriggerAttack();
        }
    }

    private void Sword_OnSwordSwing(object sender, EventArgs e) {
        TriggerAttack();
    }

    private void TriggerAttack() {
        if (Time.time - lastAttackTime < attackCooldown)
            return;
        lastAttackTime = Time.time;
        animator.ResetTrigger(AttackHash);
        animator.SetTrigger(AttackHash);
    }
}
