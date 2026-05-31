using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossAI : EnemyAI {
    [Header("Boss Phase Settings")]
    [SerializeField] private int _phase2HealthThreshold = 100;
    [SerializeField] private int _phase3HealthThreshold = 50;

    [Header("Phase 2 - Ranged Attack")]
    [SerializeField] private GameObject _rangedProjectile;
    [SerializeField] private float _rangedAttackRange = 8f;
    [SerializeField] private float _rangedAttackCooldown = 3f;
    private float _lastRangedAttackTime;

    [Header("Phase 3 - Summon Minions")]
    [SerializeField] private GameObject[] _minionPrefabs;
    [SerializeField] private int _minionsToSummon = 3;
    [SerializeField] private float _summonCooldown = 8f;
    private float _lastSummonTime;

    [Header("Phase 3 - Ground Slam (Attack1)")]
    [SerializeField] private float _slamRadius = 3f;
    [SerializeField] private int _slamDamage = 20;
    [SerializeField] private float _slamCooldown = 5f;
    private float _lastSlamTime;

    [Header("Phase 2 - Full Combo")]
    [SerializeField] private float _comboCooldown = 6f;
    [SerializeField] private int _comboDamage = 15;
    private float _lastComboTime;

    [Header("Visual Effects")]
    [SerializeField] private ParticleSystem _phaseTransitionEffect;
    [SerializeField] private ParticleSystem _slamEffect;

    private int _currentPhase = 1;
    private bool _isInSpecialAttack = false;
    private int _currentHealth;
    private int _maxHealth;

    // Анимационные триггеры (под вашу структуру)
    private static readonly int SLAM = Animator.StringToHash("Slam");           // Attack1
    private static readonly int RANGED_ATTACK = Animator.StringToHash("RangedAttack");
    private static readonly int SUMMON = Animator.StringToHash("Summon");
    private static readonly int FULL_COMBO = Animator.StringToHash("FullCombo");

    protected override void Start() {
        base.Start();

        if (_enemyEntity != null) {
            _maxHealth = _enemyEntity.GetMaxHealth();
            _currentHealth = _maxHealth;
            _enemyEntity.OnHealthChanged += OnHealthChanged;
        }
    }

    private void OnHealthChanged(int currentHealth, int maxHealth) {
        _currentHealth = currentHealth;
        int healthPercent = (currentHealth * 100) / maxHealth;

        if (healthPercent <= _phase3HealthThreshold && _currentPhase < 3) {
            _currentPhase = 3;
            TriggerPhaseTransition(3);
        }
        else if (healthPercent <= _phase2HealthThreshold && _currentPhase < 2) {
            _currentPhase = 2;
            TriggerPhaseTransition(2);
        }
    }

    protected override void HandleChasing() {
        if (_isInSpecialAttack) return;

        float distToPlayer = Vector3.Distance(transform.position, _player.position);

        // Phase 3: Ground Slam (Attack1) - когда близко
        if (_currentPhase >= 3 && distToPlayer < 2.5f && Time.time >= _lastSlamTime + _slamCooldown) {
            StartCoroutine(GroundSlam());
            return;
        }

        // Phase 2: Full Combo - редкая мощная атака
        if (_currentPhase >= 2 && distToPlayer < _attackRange && Time.time >= _lastComboTime + _comboCooldown) {
            if (Random.Range(0f, 1f) < 0.3f) // 30% шанс на комбо
            {
                StartCoroutine(FullComboAttack());
                return;
            }
        }

        // Phase 2: Ranged Attack - дальняя атака
        if (_currentPhase >= 2 && distToPlayer > _attackRange && distToPlayer <= _rangedAttackRange) {
            if (Time.time >= _lastRangedAttackTime + _rangedAttackCooldown) {
                StartCoroutine(RangedAttack());
                return;
            }
        }

        // Phase 3: Summon Minions - призыв миньонов
        if (_currentPhase >= 3 && Time.time >= _lastSummonTime + _summonCooldown) {
            StartCoroutine(SummonMinions());
            return;
        }

        // ОБЫЧНЫЕ АТАКИ (Attack2 или Attack3)
        if (distToPlayer < _attackRange) {
            PerformMeleeAttack();
            return;
        }

        // Стандартное преследование
        base.HandleChasing();
    }

    private void PerformMeleeAttack() {
        // Случайный выбор атаки (Attack2 или Attack3)
        // Оба используют триггер "Attack", но анимация выбирается случайно через Animator
        if (animator != null) {
            animator.SetTrigger(ATTACK);
        }

        // Задержка перед нанесением урона
        CancelInvoke(nameof(DelayedDamage));
        Invoke(nameof(DelayedDamage), 0.3f);
    }

    private void DelayedDamage() {
        float distToPlayer = Vector3.Distance(transform.position, _player.position);
        if (distToPlayer < _attackRange) {
            Player.Instance?.TakeDamage(transform, _attackDamage);
        }
    }

    protected override void OnHit() {
        base.OnHit();
        if (animator != null) {
            animator.SetTrigger(HIT);
        }
    }

    protected override void OnDeath() {
        base.OnDeath();
        if (animator != null) {
            animator.SetTrigger(DEATH);
        }
    }

    private IEnumerator RangedAttack() {
        _isInSpecialAttack = true;
        _lastRangedAttackTime = Time.time;

        _rb.linearVelocity = Vector2.zero;

        if (animator != null) {
            animator.SetTrigger(RANGED_ATTACK);
        }

        yield return new WaitForSeconds(0.5f);

        if (_rangedProjectile != null && _player != null) {
            Vector2 direction = (_player.position - transform.position).normalized;
            GameObject projectile = Instantiate(_rangedProjectile, transform.position, Quaternion.identity);

            var proj = projectile.GetComponent<Projectile>();
            if (proj != null) {
                proj.Initialize(direction, _attackDamage / 2);  // Initialize сам ставит velocity + rotation
            }
        }

        yield return new WaitForSeconds(0.5f);
        _isInSpecialAttack = false;
    }

    private IEnumerator GroundSlam() {
        _isInSpecialAttack = true;
        _lastSlamTime = Time.time;

        _rb.linearVelocity = Vector2.zero;

        if (animator != null) {
            animator.SetTrigger(SLAM);  // Attack1
        }

        if (_slamEffect != null) {
            _slamEffect.transform.position = transform.position;
            _slamEffect.Play();
        }

        yield return new WaitForSeconds(0.3f);

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, _slamRadius);
        foreach (var hit in hits) {
            if (hit.TryGetComponent<Player>(out var player)) {
                player.TakeDamage(transform, _slamDamage);
            }
        }

        yield return new WaitForSeconds(0.5f);
        _isInSpecialAttack = false;
    }

    private IEnumerator SummonMinions() {
        _isInSpecialAttack = true;
        _lastSummonTime = Time.time;

        _rb.linearVelocity = Vector2.zero;

        if (animator != null) {
            animator.SetTrigger(SUMMON);
        }

        yield return new WaitForSeconds(0.8f);

        for (int i = 0; i < _minionsToSummon; i++) {
            if (_minionPrefabs.Length > 0) {
                GameObject minionPrefab = _minionPrefabs[Random.Range(0, _minionPrefabs.Length)];
                Vector3 spawnOffset = Random.insideUnitCircle.normalized * 2f;
                Vector3 spawnPos = transform.position + spawnOffset;
                Instantiate(minionPrefab, spawnPos, Quaternion.identity);
            }
        }

        yield return new WaitForSeconds(0.8f);
        _isInSpecialAttack = false;
    }

    private IEnumerator FullComboAttack() {
        _isInSpecialAttack = true;
        _lastComboTime = Time.time;

        _rb.linearVelocity = Vector2.zero;

        if (animator != null) {
            animator.SetTrigger(FULL_COMBO);
        }

        // 3 удара с таймингами под анимацию
        yield return new WaitForSeconds(0.4f);
        DealDamageCombo();
        yield return new WaitForSeconds(0.3f);
        DealDamageCombo();
        yield return new WaitForSeconds(0.3f);
        DealDamageCombo();

        yield return new WaitForSeconds(0.5f);
        _isInSpecialAttack = false;
    }

    private void DealDamageCombo() {
        float dist = Vector3.Distance(transform.position, _player.position);
        if (dist < _attackRange) {
            Player.Instance?.TakeDamage(transform, _comboDamage);
        }
    }

    private void TriggerPhaseTransition(int phase) {
        Debug.Log($"Boss entered Phase {phase}!");
        if (_phaseTransitionEffect != null) {
            _phaseTransitionEffect.Play();
        }

        // Сброс кулдаунов
        _lastRangedAttackTime = -_rangedAttackCooldown;
        _lastSummonTime = -_summonCooldown;
        _lastSlamTime = -_slamCooldown;
        _lastComboTime = -_comboCooldown;
    }

    public override void DealDamage() {
        if (Player.Instance == null) return;
        if (Player.Instance.IsDead) return;

        float dist = Vector3.Distance(transform.position, _player.position);
        if (dist < _attackRange) {
            Player.Instance?.TakeDamage(transform, _attackDamage);
        }
    }

    protected override void OnDestroy() {
        if (_enemyEntity != null) {
            _enemyEntity.OnHealthChanged -= OnHealthChanged;
        }
        base.OnDestroy();
    }

    protected new void OnDrawGizmosSelected() {
        base.OnDrawGizmosSelected();
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _slamRadius);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _rangedAttackRange);
    }
}