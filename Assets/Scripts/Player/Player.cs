using System;
using System.Collections;
using Other;
using UnityEngine;

public class Player : MonoBehaviour
{
    public static Player Instance { get; private set; }

    [SerializeField]
    private float movingSpeed = 5f;

    [SerializeField]
    private int _maxHealth = 40;

    [SerializeField]
    private float _damageCooldown = 1f;

    [SerializeField]
    private float _dashSpeed = 15f;

    [SerializeField]
    private float _dashDuration = 0.2f;

    [SerializeField]
    private float _dashCooldown = 1f;

    private float minMovingSpeed = 0.1f;
    private bool isRunning = false;
    private Rigidbody2D rb;
    private PlayerVisual playerVisual;
    private KnockBack _knockBack;

    private int _currentHealth;
    private bool _canTakeDamage;
    private bool _isDashing;
    private bool _canDash = true;
    private bool _isDead;
    private Vector2 _dashDirection;

    private EventHandler _onAttack;
    public event EventHandler OnFlashBlink;
    public int MaxHealth => _maxHealth;
    public event Action<int, int> OnHealthChanged;

    private Action _onAttackHeld;
    private Action _onAttackReleased;
    private Action _onDash;

    private void Awake()
    {
        Instance = this;
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;
        playerVisual = GetComponentInChildren<PlayerVisual>();
        _knockBack = GetComponent<KnockBack>();
        _dashDirection = Vector2.right;
    }

    private void Start()
    {
        _canTakeDamage = true;
        _currentHealth = _maxHealth;
        _onAttack = (s, e) => ActiveWeapon.Instance.Attack();
        _onAttackHeld = () => ActiveWeapon.Instance.AttackHeld();
        _onAttackReleased = () => ActiveWeapon.Instance.AttackReleased();
        _onDash = () => StartCoroutine(Dash());

        GameInput.Instance.OnPlayerAttack += _onAttack;
        GameInput.Instance.OnPlayerAttackHeld += _onAttackHeld;
        GameInput.Instance.OnPlayerAttackReleased += _onAttackReleased;
        GameInput.Instance.OnPlayerDash += _onDash;
    }

    private void FixedUpdate()
    {
        HandleMovement();
    }

    public void TakeDamage(Transform damageSource, int damage)
    {
        if (_isDead || !_canTakeDamage)
            return;
        _canTakeDamage = false;
        _currentHealth = Mathf.Max(0, _currentHealth -= damage);
        OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
        playerVisual.TriggerHit();
        _knockBack.GetKnockedBack(damageSource);
        OnFlashBlink?.Invoke(this, EventArgs.Empty);
        StartCoroutine(DamageCooldown());
        DetectDeath();
    }

    private void DetectDeath()
    {
        if (_currentHealth == 0)
        {
            _isDead = true;
            _canTakeDamage = false;
            ActiveWeapon.Instance.gameObject.SetActive(false);
            _knockBack.StopKnockBackMovement();
            playerVisual.SetDead();
            StartCoroutine(DestroyAfterDeath());
        }
    }

    private IEnumerator DestroyAfterDeath()
    {
        yield return new WaitForSeconds(2f);
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        GameInput.Instance.OnPlayerAttack -= _onAttack;
        GameInput.Instance.OnPlayerAttackHeld -= _onAttackHeld;
        GameInput.Instance.OnPlayerAttackReleased -= _onAttackReleased;
        GameInput.Instance.OnPlayerDash -= _onDash;
    }

    private IEnumerator DamageCooldown()
    {
        yield return new WaitForSeconds(_damageCooldown);
        _canTakeDamage = true;
    }

    private void HandleMovement()
    {
        if (_isDead || _knockBack.IsGettingKnockedBack || _isDashing)
            return;
        Vector2 inputVector = GameInput.Instance.GetMovementVector();
        rb.MovePosition(rb.position + inputVector * (movingSpeed * Time.fixedDeltaTime));

        if (Mathf.Abs(inputVector.x) > minMovingSpeed || Mathf.Abs(inputVector.y) > minMovingSpeed)
        {
            isRunning = true;
            _dashDirection = inputVector;
        }
        else
        {
            isRunning = false;
        }
    }

    private IEnumerator Dash()
    {
        if (!_canDash || _dashDirection == Vector2.zero || _isDead)
            yield break;

        _canDash = false;
        _isDashing = true;
        playerVisual.SetDashing(true);

        rb.linearVelocity = _dashDirection.normalized * _dashSpeed;
        yield return new WaitForSeconds(_dashDuration);
        rb.linearVelocity = Vector2.zero;

        _isDashing = false;
        playerVisual.SetDashing(false);

        yield return new WaitForSeconds(_dashCooldown);
        _canDash = true;
    }

    public bool IsRunning()
    {
        return isRunning;
    }

    public Vector3 GetPlayerScreenPosition()
    {
        Vector3 playerScreenPosition = Camera.main.WorldToScreenPoint(transform.position);
        return playerScreenPosition;
    }
}
