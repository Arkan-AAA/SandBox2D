using Audio;
using Enemies;
using UnityEngine;

/// <summary>
/// Вешается на босса. Расширяет EnemySounds фазовой музыкой и уникальными атаками.
/// </summary>
public class BossSounds : MonoBehaviour
{
    [Header("Combat SFX")]
    [SerializeField] private SoundSO _hitSFX;
    [SerializeField] private SoundSO _deathSFX;
    [SerializeField] private SoundSO _meleeSFX;
    [SerializeField] private SoundSO _rangedSFX;
    [SerializeField] private SoundSO _slamSFX;
    [SerializeField] private SoundSO _summonSFX;
    [SerializeField] private SoundSO _comboSFX;
    [SerializeField] private SoundSO _roarSFX;       // Переход фазы

    [Header("Music per Phase")]
    [SerializeField] private SoundSO _phase1Music;
    [SerializeField] private SoundSO _phase2Music;
    [SerializeField] private SoundSO _phase3Music;

    private EnemyEntity _enemyEntity;
    private BossAI      _bossAI;

    private void Start()
    {
        _enemyEntity = GetComponent<EnemyEntity>();
        _bossAI      = GetComponent<BossAI>();

        if (_enemyEntity != null)
        {
            _enemyEntity.OnHit   += OnHit;
            _enemyEntity.OnDeath += OnDeath;
        }

        if (_bossAI != null)
        {
            _bossAI.OnPhaseChanged   += OnPhaseChanged;
            _bossAI.OnRangedAttack   += OnRangedAttack;
            _bossAI.OnGroundSlam     += OnGroundSlam;
            _bossAI.OnSummon         += OnSummon;
            _bossAI.OnFullCombo      += OnFullCombo;
            _bossAI.OnMeleeAttack    += OnMeleeAttack;
        }

        // Стартовая музыка босса
        AudioManager.Instance?.PlayMusic(_phase1Music);
    }

    private void OnHit()
    {
        AudioManager.Instance?.PlaySFX(_hitSFX, transform.position);
    }

    private void OnDeath()
    {
        AudioManager.Instance?.PlaySFX(_deathSFX, transform.position);
        AudioManager.Instance?.StopMusic();
    }

    private void OnPhaseChanged(int phase)
    {
        AudioManager.Instance?.PlaySFX(_roarSFX, transform.position);

        var music = phase switch {
            2 => _phase2Music,
            3 => _phase3Music,
            _ => _phase1Music
        };
        AudioManager.Instance?.PlayMusic(music);
    }

    private void OnRangedAttack() => AudioManager.Instance?.PlaySFX(_rangedSFX,  transform.position);
    private void OnGroundSlam()   => AudioManager.Instance?.PlaySFX(_slamSFX,    transform.position);
    private void OnSummon()       => AudioManager.Instance?.PlaySFX(_summonSFX,  transform.position);
    private void OnFullCombo()    => AudioManager.Instance?.PlaySFX(_comboSFX,   transform.position);
    private void OnMeleeAttack()  => AudioManager.Instance?.PlaySFX(_meleeSFX,   transform.position);

    private void OnDestroy()
    {
        if (_enemyEntity != null)
        {
            _enemyEntity.OnHit   -= OnHit;
            _enemyEntity.OnDeath -= OnDeath;
        }

        if (_bossAI != null)
        {
            _bossAI.OnPhaseChanged  -= OnPhaseChanged;
            _bossAI.OnRangedAttack  -= OnRangedAttack;
            _bossAI.OnGroundSlam    -= OnGroundSlam;
            _bossAI.OnSummon        -= OnSummon;
            _bossAI.OnFullCombo     -= OnFullCombo;
            _bossAI.OnMeleeAttack   -= OnMeleeAttack;
        }
    }
}
