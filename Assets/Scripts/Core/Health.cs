using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour, IDamageable
{
    [SerializeField] private int maxHealth = 100;

    public int Current { get; private set; }
    public UnityEvent<int, int> OnHealthChanged; // (current, max)
    public UnityEvent OnDeath;

    private void Awake()
    {
        Current = maxHealth;
    }

    public void TakeDamage(int amount)
    {
        if (Current <= 0) return;

        Current = Mathf.Max(0, Current - amount);
        OnHealthChanged?.Invoke(Current, maxHealth);

        if (Current == 0)
            Die();
    }

    public void Die()
    {
        OnDeath?.Invoke();
    }
}
