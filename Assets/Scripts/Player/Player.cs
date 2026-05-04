using System;
using System.Collections;
using Other;
using UnityEngine;

public class Player : MonoBehaviour
{
    public static Player Instance { get; private set; }

    [SerializeField] private float movingSpeed = 5f;
    [SerializeField] private int _maxHealth = 100;
    [SerializeField] private float _damageCooldown = 1f;
    
    private float minMovingSpeed = 0.1f;
    private bool isRunning = false;
    private Rigidbody2D rb;
    private PlayerVisual playerVisual;
    private KnockBack _knockBack;

    private int _currentHealth;
    private bool _canTakeDamage;

    private EventHandler _onAttack;
    private Action _onAttackHeld;
    private Action _onAttackReleased;

    private void Awake()
    {
        Instance = this;
        rb = GetComponent<Rigidbody2D>(); 
        rb.freezeRotation = true;
        playerVisual = GetComponentInChildren<PlayerVisual>();

        _knockBack = GetComponent<KnockBack>();
    }

    private void Start()
    {
        _canTakeDamage = true;
        _currentHealth = _maxHealth;
        _onAttack = (s, e) => ActiveWeapon.Instance.Attack();
        _onAttackHeld = () => ActiveWeapon.Instance.AttackHeld();
        _onAttackReleased = () => ActiveWeapon.Instance.AttackReleased();

        GameInput.Instance.OnPlayerAttack += _onAttack;
        GameInput.Instance.OnPlayerAttackHeld += _onAttackHeld;
        GameInput.Instance.OnPlayerAttackReleased += _onAttackReleased;
    }

    private void FixedUpdate()
    {
        HandleMovement();
    }

    public void TakeDamage(Transform damageSource, int damage)
    {
        if (_canTakeDamage)
        {
            _canTakeDamage = false;
            _currentHealth = Mathf.Max(0, _currentHealth -= damage);
            _knockBack.GetKnockedBack(damageSource);
            
            StartCoroutine(DamageCooldown());
        }

        DetectDeath();
    }

    private void DetectDeath()
    {
        if (_currentHealth == 0)
        {
            Destroy(this.gameObject);
        }
    }
    
    private void OnDestroy()
    {
        GameInput.Instance.OnPlayerAttack -= _onAttack;
        GameInput.Instance.OnPlayerAttackHeld -= _onAttackHeld;
        GameInput.Instance.OnPlayerAttackReleased -= _onAttackReleased;
    }

    private IEnumerator DamageCooldown()
    {
        yield return new WaitForSeconds(_damageCooldown);
        _canTakeDamage = true;
    }

    private void HandleMovement()
    {
        if (_knockBack.IsGettingKnockedBack) return;
        Vector2 inputVector = GameInput.Instance.GetMovementVector();
        rb.MovePosition(rb.position + inputVector * (movingSpeed * Time.fixedDeltaTime));
    
        if (Mathf.Abs(inputVector.x) > minMovingSpeed || Mathf.Abs(inputVector.y) > minMovingSpeed)
        {
            isRunning = true;
        }
        else
        {
            isRunning = false;
        }
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