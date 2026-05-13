using System;

namespace Combat {
    // Лук (в будущем)
    public class Bow : Weapon {
        public event EventHandler OnBowDraw;    // натяжение
        public event EventHandler OnBowRelease; // выстрел

        public override void Attack() {
            AttackHeld();
        }
        public override void AttackHeld() {
            OnBowDraw?.Invoke(this, EventArgs.Empty);
        }

        public override void AttackReleased() {
            OnBowRelease?.Invoke(this, EventArgs.Empty);
        }
    }
}