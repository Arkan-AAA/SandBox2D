using UnityEngine;

public class PitTrap : Trap
{
    protected override void OnTrapTriggered(Player player)
    {
        player.TakeDamage(transform, int.MaxValue);
    }
}
