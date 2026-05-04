using UnityEngine;

public class EnemyAnimationEvents : MonoBehaviour
{
    private EnemyAI _enemyAI;

    private void Awake()
    {
        _enemyAI = GetComponentInParent<EnemyAI>();
    }

    public void DealDamage() => _enemyAI.DealDamage();
    public void DestroyEnemy() => _enemyAI.DestroyEnemy();
}
