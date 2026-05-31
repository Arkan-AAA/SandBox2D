using UnityEngine;
using Enemies;

public class Projectile : MonoBehaviour {
    public int damage = 10;
    public float speed = 10f;
    public float lifetime = 3f;
    public GameObject hitEffect;

    [Header("Layers")]
    public LayerMask wallLayer;
    public LayerMask enemyLayer;

    [Header("Visual")]
    public bool rotateToDirection = true;
    public bool rotateContinuously = false;
    public float rotationSpeed = 360f;

    private Rigidbody2D _rb;
    private Vector2 _direction;
    private bool _isInitialized = false;

    public void Initialize(Vector2 direction, int damageAmount) {
        damage = damageAmount;
        _direction = direction.normalized;
        _rb = GetComponent<Rigidbody2D>();

        if (rotateToDirection) {
            RotateToDirection(_direction);
        }

        if (_rb != null) {
            _rb.linearVelocity = _direction * speed;
        }

        _isInitialized = true;
        Destroy(gameObject, lifetime);
    }

    private void RotateToDirection(Vector2 direction) {
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    private void Update() {
        if (!_isInitialized) return;

        if (rotateContinuously) {
            transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
        }
        else if (rotateToDirection && _rb != null && _rb.linearVelocity.sqrMagnitude > 0.01f) {
            RotateToDirection(_rb.linearVelocity.normalized);
        }
    }

    private void OnTriggerEnter2D(Collider2D other) {
        // Игнорируем триггер до инициализации
        if (!_isInitialized) return;

        // Попадание во врага
        if (other.TryGetComponent<EnemyEntity>(out var enemy)) {
            enemy.TakeDamage(transform, damage);
            if (hitEffect != null) {
                Instantiate(hitEffect, transform.position, Quaternion.identity);
            }
            Destroy(gameObject);
        }
        // Попадание в стену
        else if (wallLayer != 0 && ((1 << other.gameObject.layer) & wallLayer) != 0) {
            Destroy(gameObject);
        }
        // Попадание в другого врага (опционально)
        else if (enemyLayer != 0 && ((1 << other.gameObject.layer) & enemyLayer) != 0) {
            Destroy(gameObject);
        }
    }
}