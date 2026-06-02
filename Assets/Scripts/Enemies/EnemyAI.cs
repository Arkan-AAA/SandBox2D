using System;
using Enemies;
using Other;
using Satyr.Utils;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemyAI : MonoBehaviour {
    [SerializeField] protected State _startingState;
    [SerializeField] protected float _roamingDistanceMax = 7f;
    [SerializeField] protected float _roamingDistanceMin = 3f;
    [SerializeField] protected float _roamingTimerMax = 2f;
    [SerializeField] protected float _chaseRange = 8f;
    [SerializeField] protected float _attackRange = 2f;
    [SerializeField] protected float _attackCooldown = 1.2f;
    [SerializeField] protected int _attackDamage = 10;
    [SerializeField] protected float _roamAnimSpeed = 1f;
    [SerializeField] protected float _chaseAnimSpeed = 1.5f;
    [SerializeField] protected float _moveSpeed = 3.5f;

    [Header("Obstacle Avoidance")]
    [SerializeField] protected float _rayDistance = 1.2f;
    [SerializeField] protected float _avoidForce = 2f;
    [SerializeField] protected LayerMask _obstacleLayer;

    [Header("Layers")]
    [SerializeField] protected LayerMask _enemyLayer;

#if UNITY_EDITOR
    [Header("Debug")]
    [SerializeField] private bool _isChasingEnemy;
    [SerializeField] private bool _isAttackingEnemy;
#endif

    public event EventHandler OnFlashBlink;
    public event Action OnAlert;
    public event Action OnAttackSound;

    protected Rigidbody2D _rb;
    protected State state;
    protected float roamingTime;
    protected Vector3 roamPosition;
    protected Vector3 startingPosition;
    protected Animator animator;
    protected bool _isDead;

    protected const string IS_MOVING = "IsMoving";
    protected const string IS_CHASING = "IsChasing";
    protected const string ATTACK = "Attack";
    protected const string HIT = "Hit";
    protected const string DEATH = "Death";

    protected float _attackTimer;
    protected EnemyEntity _enemyEntity;
    protected KnockBack _knockBack;
    protected Transform _player;

    private bool _alertFired = false;

    protected enum State { Idle, Roaming, Chasing, Attacking, Death }

    protected virtual void Awake() {
        _rb = GetComponent<Rigidbody2D>();
        if (_rb != null) _rb.freezeRotation = true;

        state = _startingState;
        roamingTime = Random.Range(0f, _roamingTimerMax);
        animator = GetComponentInChildren<Animator>();
    }

    protected virtual void Start() {
        startingPosition = transform.position;
        _enemyEntity = GetComponent<EnemyEntity>();
        _knockBack = GetComponent<KnockBack>();
        _player = Player.Instance?.transform;

        if (_enemyEntity != null) {
            _enemyEntity.OnHit += OnHit;
            _enemyEntity.OnDeath += OnDeath;
        }
    }

    protected virtual void Update() {
        if (Player.Instance == null || Player.Instance.IsDead) return;
        if (_player == null) _player = Player.Instance?.transform;
        if (_player == null) return;

        if (_knockBack != null && _knockBack.IsGettingKnockedBack) {
            _rb.linearVelocity = Vector2.zero;
            return;
        }

#if UNITY_EDITOR
        if (_isAttackingEnemy) state = State.Attacking;
        else if (_isChasingEnemy) state = State.Chasing;
#endif

        float distToPlayer = Vector3.Distance(transform.position, _player.position);
        bool isMoving = _rb.linearVelocity.magnitude > 0.1f;
        if (animator != null) animator.SetBool(IS_MOVING, isMoving);

        switch (state) {
            case State.Idle:
                if (distToPlayer < _chaseRange) {
                    state = State.Chasing;
                    FireAlert();
                    break;
                }
                roamingTime -= Time.deltaTime;
                if (roamingTime < 0) Roaming();
                break;

            case State.Roaming:
                if (distToPlayer < _chaseRange) {
                    state = State.Chasing;
                    FireAlert();
                    break;
                }
                if (Vector3.Distance(transform.position, roamPosition) < 0.3f) {
                    state = State.Idle;
                    roamingTime = Random.Range(1f, _roamingTimerMax);
                    _rb.linearVelocity = Vector2.zero;
                }
                break;

            case State.Chasing:
                HandleChasing();
                break;

            case State.Attacking:
                _attackTimer -= Time.deltaTime;
                if (_attackTimer <= 0f) {
                    if (animator != null) animator.SetTrigger(ATTACK);
                    OnAttackSound?.Invoke();  // ← звук атаки
                    _attackTimer = _attackCooldown;
                }
                if (distToPlayer > _attackRange)
                    state = State.Chasing;
                break;

            case State.Death:
                _rb.linearVelocity = Vector2.zero;
                break;
        }

        bool isChasing = state == State.Chasing;
        if (animator != null) {
            animator.SetBool(IS_CHASING, isChasing);
            animator.speed = isChasing ? _chaseAnimSpeed : _roamAnimSpeed;
        }
    }

    private void FireAlert() {
        if (_alertFired) return;
        _alertFired = true;
        OnAlert?.Invoke();
    }

    protected virtual void HandleChasing() {
        float distToPlayer = Vector3.Distance(transform.position, _player.position);

        if (distToPlayer > _chaseRange) {
            state = State.Roaming;
            Roaming();
            return;
        }

        if (distToPlayer < _attackRange) {
            state = State.Attacking;
            _rb.linearVelocity = Vector2.zero;
            _attackTimer = 0f;
            return;
        }

        Vector2 direction = (_player.position - transform.position).normalized;
        Vector2 avoidance = GetAvoidanceVector();
        _rb.linearVelocity = (direction + avoidance) * _moveSpeed;
        ChangeFacingDirection(transform.position, _player.position);
    }

    protected virtual Vector2 GetAvoidanceVector() {
        Vector2 avoidance = Vector2.zero;
        Vector2 forward = _rb.linearVelocity.normalized;
        if (forward == Vector2.zero) forward = transform.right;

        Vector2[] dirs = { forward, Quaternion.Euler(0, 0, 45) * forward, Quaternion.Euler(0, 0, -45) * forward };
        foreach (Vector2 dir in dirs) {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, _rayDistance, _obstacleLayer);
            if (hit.collider != null)
                avoidance += Vector2.Perpendicular(hit.normal).normalized * _avoidForce;
        }
        return avoidance;
    }

    protected virtual void OnHit() {
        if (animator != null) animator.SetTrigger(HIT);
        OnFlashBlink?.Invoke(this, EventArgs.Empty);
    }

    protected virtual void OnDestroy() {
        if (_enemyEntity != null) {
            _enemyEntity.OnHit -= OnHit;
            _enemyEntity.OnDeath -= OnDeath;
        }
    }

    protected virtual void OnDeath() {
        _isDead = true;
        state = State.Death;
        _rb.linearVelocity = Vector2.zero;
        if (animator != null) animator.SetTrigger(DEATH);
        foreach (var col in GetComponents<Collider2D>())
            col.enabled = false;
    }

    public virtual void DealDamage() {
        if (Player.Instance == null || Player.Instance.IsDead) return;
        float dist = Vector3.Distance(transform.position, Player.Instance.transform.position);
        if (dist < _attackRange)
            Player.Instance.TakeDamage(transform, _attackDamage);
    }

    public virtual void DestroyEnemy() => Destroy(gameObject);

    protected virtual void Roaming() {
        roamPosition = GetRoamingPosition();
        Vector2 direction = (roamPosition - transform.position).normalized;
        _rb.linearVelocity = direction * _moveSpeed;
        ChangeFacingDirection(transform.position, roamPosition);
        roamingTime = Random.Range(1f, 3f);
    }

    protected virtual Vector3 GetRoamingPosition() {
        Vector2 randomDir = Utils.GetRandomDir() * Random.Range(_roamingDistanceMin, _roamingDistanceMax);
        return startingPosition + (Vector3)randomDir;
    }

    protected virtual void ChangeFacingDirection(Vector3 sourcePosition, Vector3 targetPosition) {
        transform.rotation = targetPosition.x < sourcePosition.x
            ? Quaternion.Euler(0f, 180f, 0f)
            : Quaternion.Euler(0f, 0f, 0f);
    }

    public virtual void SetTarget(Transform target) => _player = target;

    protected virtual void OnDrawGizmosSelected() {
        Gizmos.color = Color.red;
        Vector2 forward = _rb != null ? _rb.linearVelocity.normalized : transform.right;
        if (forward == Vector2.zero) forward = transform.right;
        Gizmos.DrawRay(transform.position, forward * _rayDistance);
        Gizmos.DrawRay(transform.position, Quaternion.Euler(0, 0, 45) * forward * _rayDistance);
        Gizmos.DrawRay(transform.position, Quaternion.Euler(0, 0, -45) * forward * _rayDistance);
    }
}