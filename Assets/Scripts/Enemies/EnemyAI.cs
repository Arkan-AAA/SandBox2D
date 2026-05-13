using UnityEngine;
using UnityEngine.AI;
using Satyr.Utils;
using System;
using Enemies;
using Other;
using Random = UnityEngine.Random;

public class EnemyAI : MonoBehaviour
{
    [SerializeField] private State _startingState;
    [SerializeField] private float _roamingDistanceMax = 7f;
    [SerializeField] private float _roamingDistanceMin = 3f;
    [SerializeField] private float _roamingTimerMax = 2f;
    [SerializeField] private float _chaseRange = 8f;
    [SerializeField] private float _attackRange = 2f;
    [SerializeField] private float _attackCooldown = 1.2f;
    [SerializeField] private int _attackDamage = 10;
    [SerializeField] private float _roamAnimSpeed = 1f;
    [SerializeField] private float _chaseAnimSpeed = 1.5f;

#if UNITY_EDITOR
    [Header("Debug")]
    [SerializeField] private bool _isChasingEnemy;
    [SerializeField] private bool _isAttackingEnemy;
#endif

    public event EventHandler OnFlashBlink;
    
    private NavMeshAgent _navMeshAgent;
    private State state;
    private float roamingTime;
    private Vector3 roamPosition;
    private Vector3 startingPosition;
    private Animator animator;

    private const string IS_MOVING = "IsMoving";
    private const string IS_CHASING = "IsChasing";
    private const string ATTACK = "Attack";
    private const string HIT = "Hit";
    private const string DEATH = "Death";

    private float _attackTimer;
    private EnemyEntity _enemyEntity;
    private KnockBack _knockBack;

    private enum State
    {
        Idle,
        Roaming,
        Chasing,
        Attacking,
        Death
    }

    private void Awake()
    {
        _navMeshAgent = GetComponent<NavMeshAgent>();
        _navMeshAgent.updateUpAxis = false;
        _navMeshAgent.updateRotation = false;

        var rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.freezeRotation = true;

        state = _startingState;
        roamingTime = Random.Range(0f, _roamingTimerMax);

        animator = GetComponentInChildren<Animator>();
    }

    private void Start()
    {
        startingPosition = transform.position;

        _enemyEntity = GetComponent<EnemyEntity>();
        _knockBack = GetComponent<KnockBack>();
        _enemyEntity.OnHit += OnHit;
        _enemyEntity.OnDeath += OnDeath;
    }

    private void Update()
    {
        if (Player.Instance == null) return;
        if (_knockBack != null && _knockBack.IsGettingKnockedBack)
        {
            _navMeshAgent.ResetPath();
            return;
        }

#if UNITY_EDITOR
        if (_isAttackingEnemy) { state = State.Attacking; }
        else if (_isChasingEnemy) { state = State.Chasing; }
#endif

        bool isMoving = _navMeshAgent.velocity.magnitude > 0.1f;
        animator.SetBool(IS_MOVING, isMoving);

        float distToPlayer = Vector3.Distance(transform.position, Player.Instance.transform.position);

        switch (state)
        {
            case State.Idle:
                if (distToPlayer < _chaseRange)
                {
                    state = State.Chasing;
                    break;
                }
                roamingTime -= Time.deltaTime;
                if (roamingTime < 0) Roaming();
                break;

            case State.Roaming:
                if (distToPlayer < _chaseRange)
                {
                    state = State.Chasing;
                    break;
                }
                if (!_navMeshAgent.pathPending && _navMeshAgent.remainingDistance < 0.2f)
                {
                    state = State.Idle;
                    roamingTime = Random.Range(1f, _roamingTimerMax);
                }
                break;

            case State.Chasing:
                if (distToPlayer > _chaseRange)
                {
                    state = State.Roaming;
                    Roaming();
                    break;
                }
                if (distToPlayer < _attackRange)
                {
                    state = State.Attacking;
                    _navMeshAgent.ResetPath();
                    _attackTimer = 0f;
                    break;
                }
                _navMeshAgent.SetDestination(Player.Instance.transform.position);
                ChangeFacingDirection(transform.position, Player.Instance.transform.position);
                break;

            case State.Attacking:
                _attackTimer -= Time.deltaTime;
                if (_attackTimer <= 0f)
                {
                    animator.SetTrigger(ATTACK);
                    _attackTimer = _attackCooldown;
                }
                if (distToPlayer > _attackRange)
                {
                    state = State.Chasing;
                }
                break;

            case State.Death:
                break;
        }

        bool isChasing = state == State.Chasing;
        animator.SetBool(IS_CHASING, isChasing);
        animator.speed = isChasing ? _chaseAnimSpeed : _roamAnimSpeed;
    }

    private void OnHit() => animator.SetTrigger(HIT);

    private void OnDestroy()
    {
        if (_enemyEntity != null)
        {
            _enemyEntity.OnHit -= OnHit;
            _enemyEntity.OnDeath -= OnDeath;
        }
    }

    private void OnDeath()
    {
        state = State.Death;
        _navMeshAgent.ResetPath();
        animator.SetTrigger(DEATH);
        foreach (var col in GetComponents<Collider2D>())
            col.enabled = false;
    }

    public void DealDamage()
    {
        if (Player.Instance == null) return;
        float dist = Vector3.Distance(transform.position, Player.Instance.transform.position);
        if (dist < _attackRange)
        {
            Player.Instance.TakeDamage(transform, _attackDamage);
        }
    }

    public void DestroyEnemy()
    {
        Destroy(gameObject);
    }

    private void Roaming()
    {
        roamPosition = GetRoamingPosition();
        _navMeshAgent.SetDestination(roamPosition);
        ChangeFacingDirection(startingPosition, roamPosition);
        roamingTime = Random.Range(1f, 3f);
    }

    private Vector3 GetRoamingPosition()
    {
        Vector3 randomDirection = Utils.GetRandomDir() * Random.Range(_roamingDistanceMin, _roamingDistanceMax);
        Vector3 targetPosition = startingPosition + randomDirection;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(targetPosition, out hit, 2f, NavMesh.AllAreas))
        {
            return hit.position;
        }

        return transform.position;
    }

    private void ChangeFacingDirection(Vector3 sourcePosition, Vector3 targetPosition)
    {
        transform.rotation = targetPosition.x < sourcePosition.x
            ? Quaternion.Euler(0f, 180f, 0f)
            : Quaternion.Euler(0f, 0f, 0f);
    }
}
