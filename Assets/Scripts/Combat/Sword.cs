using System;
using Combat;

public class Sword : Weapon
{
    public event EventHandler OnSwordSwing;
    public override void Attack()
    {
        OnSwordSwing?.Invoke(this, EventArgs.Empty);
    }
}
