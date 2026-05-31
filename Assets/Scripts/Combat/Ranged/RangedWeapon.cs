using System;
using UnityEngine;

namespace Combat {
    public abstract class RangedWeapon : Weapon {
        [Header("Ranged Weapon Settings")]
        [SerializeField] protected GameObject projectilePrefab;
        [SerializeField] protected float projectileSpeed = 10f;
        [SerializeField] protected float shootDelay = 0.5f;
        [SerializeField] protected int damageAmount = 10; // Добавлено

        protected float lastShootTime;
        public event EventHandler OnShoot;

        public override void Attack() {
            if (Time.time < lastShootTime + shootDelay) return;
            lastShootTime = Time.time;
            Shoot();
        }

        protected virtual void Shoot() {
            if (projectilePrefab == null) return;

            GameObject projectile = Instantiate(projectilePrefab, GetSpawnPoint(), Quaternion.identity);
            Vector2 direction = GetDirection();

            var proj = projectile.GetComponent<Projectile>();
            if (proj != null) {
                proj.Initialize(direction, damageAmount);
            }
            else if (projectile.TryGetComponent<Rigidbody2D>(out var rb)) {
                rb.linearVelocity = direction * projectileSpeed;
            }

            OnShoot?.Invoke(this, EventArgs.Empty);
        }

        protected virtual Vector2 GetSpawnPoint() {
            return transform.position;
        }

        protected virtual Vector2 GetDirection() {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            return (mousePos - transform.position).normalized;
        }
    }
}