using UnityEngine;

public abstract class Trap : MonoBehaviour
{
    [SerializeField] protected int damage = 10;
    [SerializeField] protected float cooldown = 1f;
    
    protected bool canDamage = true;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (canDamage && other.TryGetComponent<Player>(out Player player))
        {
            OnTrapTriggered(player);
        }
    }

    protected abstract void OnTrapTriggered(Player player);
}
