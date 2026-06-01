using UnityEngine;
using Combat;

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

        // Направление и точка спавна — всегда от tipPoint к мыши
        Vector2 spawnPoint = _staffVisual.GetSpawnPoint();
        Vector2 direction = _staffVisual.GetShootDirection();

        GameObject projectile = Instantiate(projectilePrefab, spawnPoint, Quaternion.identity);
        Projectile proj = projectile.GetComponent<Projectile>();

        if (proj != null) {
            proj.Initialize(direction, _damageAmount);
        }
        else {
            Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
            if (rb != null) rb.linearVelocity = direction * projectileSpeed;
        }

        if (_muzzleFlash != null) {
            GameObject flash = Instantiate(_muzzleFlash, spawnPoint, Quaternion.identity);
            Destroy(flash, 0.1f);
        }
    }

    protected override Vector2 GetDirection() => _staffVisual?.GetShootDirection() ?? Vector2.right;
    protected override Vector2 GetSpawnPoint() => _staffVisual != null ? _staffVisual.GetSpawnPoint() : base.GetSpawnPoint();
}