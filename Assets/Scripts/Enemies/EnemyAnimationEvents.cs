using Other;
using UnityEngine;

public class EnemyAnimationEvents : MonoBehaviour {
    private EnemyAI _enemyAI;
    private FlashBlink _flashBlink;

    private void Awake() {
        // Ищем компоненты на том же объекте, затем среди родителей, затем среди детей
        _enemyAI = GetComponent<EnemyAI>();
        if (_enemyAI == null) _enemyAI = GetComponentInParent<EnemyAI>();
        if (_enemyAI == null) _enemyAI = GetComponentInChildren<EnemyAI>();

        _flashBlink = GetComponent<FlashBlink>();
        if (_flashBlink == null) _flashBlink = GetComponentInParent<FlashBlink>();
        if (_flashBlink == null) _flashBlink = GetComponentInChildren<FlashBlink>();

        if (_enemyAI == null)
            Debug.LogError($"EnemyAI not found on {name} or its parent/children");
        if (_flashBlink == null)
            Debug.LogWarning($"FlashBlink not found on {name} - skipping stop blinking");
    }

    public void DealDamage() {
        if (Player.Instance == null) return;
        if (Player.Instance.IsDead) return;

        if (_enemyAI != null)
            _enemyAI.DealDamage();
    }

    public void DestroyEnemy() {
        if (_enemyAI != null)
            _enemyAI.DestroyEnemy();
        else
            Destroy(gameObject);

        if (_flashBlink != null)
            _flashBlink.StopBlinking();
    }
}