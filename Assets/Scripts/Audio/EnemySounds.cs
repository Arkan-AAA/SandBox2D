using Audio;
using Enemies;
using UnityEngine;

/// <summary>
/// Вешается на каждого врага.
/// Подписывается на события EnemyEntity и EnemyAI.
/// </summary>
public class EnemySounds : MonoBehaviour
{
    [Header("SFX")]
    [SerializeField] private SoundSO _hitSFX;
    [SerializeField] private SoundSO _deathSFX;
    [SerializeField] private SoundSO _attackSFX;
    [SerializeField] private SoundSO _alertSFX;    // Когда враг замечает игрока (опционально)

    private EnemyEntity _enemyEntity;
    private EnemyAI     _enemyAI;
    private bool        _alerted = false;

    private void Start()
    {
        _enemyEntity = GetComponent<EnemyEntity>();
        _enemyAI     = GetComponent<EnemyAI>();

        if (_enemyEntity != null)
        {
            _enemyEntity.OnHit   += OnHit;
            _enemyEntity.OnDeath += OnDeath;
        }

        if (_enemyAI != null)
        {
            _enemyAI.OnFlashBlink += OnFlashBlink;
            _enemyAI.OnAlert      += OnAlert;
            _enemyAI.OnAttackSound += OnAttack;
        }
    }

    private void OnHit()
    {
        AudioManager.Instance?.PlaySFX(_hitSFX, transform.position);
    }

    private void OnDeath()
    {
        AudioManager.Instance?.PlaySFX(_deathSFX, transform.position);
    }

    private void OnAttack()
    {
        AudioManager.Instance?.PlaySFX(_attackSFX, transform.position);
    }

    private void OnAlert()
    {
        if (_alerted) return;
        _alerted = true;
        AudioManager.Instance?.PlaySFX(_alertSFX, transform.position);
    }

    private void OnFlashBlink(object sender, System.EventArgs e)
    {
        // FlashBlink уже подписан на OnHit через EnemyEntity,
        // этот хук оставлен для дополнительной логики если нужна
    }

    private void OnDestroy()
    {
        if (_enemyEntity != null)
        {
            _enemyEntity.OnHit   -= OnHit;
            _enemyEntity.OnDeath -= OnDeath;
        }

        if (_enemyAI != null)
        {
            _enemyAI.OnFlashBlink  -= OnFlashBlink;
            _enemyAI.OnAlert       -= OnAlert;
            _enemyAI.OnAttackSound -= OnAttack;
        }
    }
}
