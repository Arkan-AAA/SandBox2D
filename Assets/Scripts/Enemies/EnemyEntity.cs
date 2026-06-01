using System;
using System.Collections;
using Other;
using ScriptableObjects;
using UnityEngine;

namespace Enemies {
    [RequireComponent(typeof(BoxCollider2D))]
    [RequireComponent(typeof(EnemyAI))]
    public class EnemyEntity : MonoBehaviour {
        [SerializeField]
        private EnemySO _enemySO;

        public int CurrentHealth { get; private set; }
        public int MaxHealth => _enemySO?.enemyHealth ?? 0;

        public event Action OnHit;
        public event Action OnDeath;
        public event Action<int, int> OnHealthChanged;

        private KnockBack _knockBack;

        [Header("Invincibility")]
        [SerializeField] private float _invincibilityDuration = 0.3f;
        private bool _isInvincible = false;

        public bool IsInvincible => _isInvincible;

        public int GetMaxHealth() => MaxHealth;

        public void SetHealth(int health) {
            CurrentHealth = health;
        }

        private void Start() {
            CurrentHealth = _enemySO.enemyHealth;
            _knockBack = GetComponent<KnockBack>();
        }

        public void TakeDamage(Transform damageSource, int damage) {
            if (CurrentHealth <= 0) return;
            if (_isInvincible) return;

            CurrentHealth -= damage;
            OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);
            _knockBack?.GetKnockedBack(damageSource);

            StartCoroutine(InvincibilityCoroutine());

            OnHit?.Invoke();
            if (CurrentHealth <= 0) {
                OnDeath?.Invoke();
                // Добавляем очки и убийства
                if (GameManager.Instance != null) {
                    GameManager.Instance.AddKill();
                    int score = _enemySO != null ? _enemySO.scoreValue : 10;
                    GameManager.Instance.AddScore(score);
                }
            }
        }

        private IEnumerator InvincibilityCoroutine() {
            _isInvincible = true;
            yield return new WaitForSeconds(_invincibilityDuration);
            _isInvincible = false;
        }
    }
}