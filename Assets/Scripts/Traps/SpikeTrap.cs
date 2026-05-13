using System.Collections;
using UnityEngine;

public class SpikeTrap : Trap
{
    protected override void OnTrapTriggered(Player player)
    {
        player.TakeDamage(transform, damage);
        StartCoroutine(DamageCooldown());
    }

    private IEnumerator DamageCooldown()
    {
        canDamage = false;
        yield return new WaitForSeconds(cooldown);
        canDamage = true;
    }
}
