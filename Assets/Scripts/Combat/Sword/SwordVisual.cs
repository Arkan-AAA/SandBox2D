using System;
using UnityEngine;

public class SwordVisual : MonoBehaviour {
    [SerializeField] private Sword sword;
    [SerializeField] private float attackCooldown = 0.3f;

    private Animator animator;
    private float lastAttackTime;
    private bool isAttackHeld;

    private static readonly int AttackTrigger = Animator.StringToHash("Attack");

    private void Awake() {
        animator = GetComponent<Animator>();
    }

    private void Start() {
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
            sword.Attack();
        }
    }

    private void Sword_OnSwordSwing(object sender, EventArgs e) {
        TriggerAttack();
    }

    private void TriggerAttack() {
        if (Time.time - lastAttackTime < attackCooldown) return;
        lastAttackTime = Time.time;
        animator.ResetTrigger(AttackTrigger);
        animator.SetTrigger(AttackTrigger);
    }

    private void TriggerEndAttackAnimation() {
        sword.AttackColiderTurnOff();
    }
}