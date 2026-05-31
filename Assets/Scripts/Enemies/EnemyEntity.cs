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

        // Публичное свойство для проверки неуязвимости
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
            if (_isInvincible) return; // Неуязвимость - урон не проходит

            CurrentHealth -= damage;
            OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);
            _knockBack?.GetKnockedBack(damageSource);

            // Включаем неуязвимость
            StartCoroutine(InvincibilityCoroutine());

            if (CurrentHealth <= 0) {
                OnDeath?.Invoke();
            }
            else {
                OnHit?.Invoke();
            }
        }

        private IEnumerator InvincibilityCoroutine() {
            _isInvincible = true;
            yield return new WaitForSeconds(_invincibilityDuration);
            _isInvincible = false;
        }
    }
}