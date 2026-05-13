using Misc;
using UnityEngine;

public class EnemyAnimationEvents : MonoBehaviour
{
    private EnemyAI _enemyAI;
    private FlashBlink _flashBlink;

    private void Awake()
    {
        _enemyAI = GetComponentInParent<EnemyAI>();
        _flashBlink = GetComponentInParent<FlashBlink>();
    }

    public void DealDamage() => _enemyAI.DealDamage();

    public void DestroyEnemy()
    {
        _enemyAI.DestroyEnemy();
        _flashBlink.StopBlinking();
    }
}
