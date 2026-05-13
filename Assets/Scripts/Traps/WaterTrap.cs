using System.Collections;
using UnityEngine;

public class WaterTrap : Trap
{
    private Coroutine _damageRoutine;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent<Player>(out Player player))
            _damageRoutine = StartCoroutine(DamageOverTime(player));
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.TryGetComponent<Player>(out _) && _damageRoutine != null)
            StopCoroutine(_damageRoutine);
    }

    protected override void OnTrapTriggered(Player player) { }

    private IEnumerator DamageOverTime(Player player)
    {
        while (true)
        {
            player.TakeDamage(transform, damage);
            yield return new WaitForSeconds(cooldown);
        }
    }
}
