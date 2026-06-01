using Audio;
using UnityEngine;

public class PlayerSounds : MonoBehaviour {
    [Header("Combat")]
    [SerializeField] private SoundSO _attackSwordSFX;
    [SerializeField] private SoundSO _attackStaffSFX;
    [SerializeField] private SoundSO _hitSFX;
    [SerializeField] private SoundSO _deathSFX;

    [Header("Movement")]
    [SerializeField] private SoundSO _dashSFX;
    [SerializeField] private SoundSO _footstepSFX;

    [Header("Footstep Settings")]
    [SerializeField] private float _footstepInterval = 0.35f;

    private float _footstepTimer;
    private Player _player;
    private Rigidbody2D _rb;

    private void Awake() {
        _player = GetComponent<Player>();
        _rb = GetComponent<Rigidbody2D>();
    }

    private void Start() {
        if (_player != null) {
            _player.OnFlashBlink += OnHit;
            _player.OnPlayerDeath += OnDeath; // ← ДОБАВЬТЕ ЭТУ СТРОКУ
        }

        if (GameInput.Instance != null) {
            GameInput.Instance.OnPlayerAttack += OnAttack;
            GameInput.Instance.OnPlayerDash += OnDash;
        }
    }

    private void Update() {
        HandleFootsteps();
    }

    private void HandleFootsteps() {
        if (_rb == null || _footstepSFX == null) return;
        if (_rb.linearVelocity.sqrMagnitude < 0.5f) return;

        _footstepTimer -= Time.deltaTime;
        if (_footstepTimer <= 0f) {
            AudioManager.Instance?.PlaySFX(_footstepSFX, transform.position);
            _footstepTimer = _footstepInterval;
        }
    }

    private void OnHit(object sender, System.EventArgs e) {
        AudioManager.Instance?.PlaySFX(_hitSFX, transform.position);
    }

    private void OnDeath() {
        AudioManager.Instance?.PlaySFX(_deathSFX, transform.position);
    }

    private void OnDash() {
        AudioManager.Instance?.PlaySFX(_dashSFX, transform.position);
    }

    private void OnAttack(object sender, System.EventArgs e) {
        var weapon = ActiveWeapon.Instance?.GetActiveWeapon();
        if (weapon is Staff)
            AudioManager.Instance?.PlaySFX(_attackStaffSFX, transform.position);
        else
            AudioManager.Instance?.PlaySFX(_attackSwordSFX, transform.position);
    }

    private void OnDestroy() {
        if (_player != null) {
            _player.OnFlashBlink -= OnHit;
            _player.OnPlayerDeath -= OnDeath;
        }

        if (GameInput.Instance != null) {
            GameInput.Instance.OnPlayerAttack -= OnAttack;
            GameInput.Instance.OnPlayerDash -= OnDash;
        }
    }
}