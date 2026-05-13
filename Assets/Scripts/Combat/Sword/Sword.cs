using System;
using Combat;
using Enemies;
using UnityEngine;

public class Sword : Weapon {
    [SerializeField]
    private int _damageAmount = 10;

    public event EventHandler OnSwordSwing;

    private PolygonCollider2D _polygonCollider2D;

    private void Awake() {
        _polygonCollider2D = GetComponent<PolygonCollider2D>();
        _polygonCollider2D.enabled = false;
    }

    public override void Attack() {
        _polygonCollider2D.enabled = true;
        OnSwordSwing?.Invoke(this, EventArgs.Empty);
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.transform.TryGetComponent(out EnemyEntity enemyEntity)) {
            enemyEntity.TakeDamage(transform, _damageAmount);
        }
    }

    public void AttackColiderTurnOff() {
        if (_polygonCollider2D != null)
            _polygonCollider2D.enabled = false;
    }
}
