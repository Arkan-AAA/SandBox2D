using UnityEngine;
using Combat; // если ваш RangedWeapon в этом namespace

public class Staff : RangedWeapon {
    [Header("Staff Specific")]
    [SerializeField] private int _damageAmount = 15;
    [SerializeField] private GameObject _muzzleFlash;
    [SerializeField] private StaffVisual _staffVisual;

    private void Start() {
        damageAmount = _damageAmount;

        if (_staffVisual == null)
            _staffVisual = GetComponentInChildren<StaffVisual>();
    }

    protected override void Shoot() {
        if (projectilePrefab == null) return;
        if (_staffVisual == null) return;

        Vector2 direction = _staffVisual.GetShootDirection();
        Vector2 spawnPoint = _staffVisual.GetSpawnPoint();

        GameObject projectile = Instantiate(projectilePrefab, spawnPoint, Quaternion.identity);
        Projectile proj = projectile.GetComponent<Projectile>();
        if (proj != null) {
            proj.Initialize(direction, _damageAmount);
        }
        else {
            Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
            if (rb != null) rb.linearVelocity = direction * projectileSpeed;
        }

        // Эффект выстрела
        if (_muzzleFlash != null) {
            GameObject flash = Instantiate(_muzzleFlash, spawnPoint, Quaternion.identity);
            Destroy(flash, 0.1f);
        }

        // Можно добавить звук
        // AudioSource.PlayClipAtPoint(shootSound, spawnPoint);
    }

    // Переопределяем направление, чтобы использовать StaffVisual
    protected override Vector2 GetDirection() => _staffVisual?.GetShootDirection() ?? Vector2.right;
}