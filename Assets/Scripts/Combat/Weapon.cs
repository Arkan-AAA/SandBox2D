using UnityEngine;

namespace Combat
{
    // Базовый класс для всех оружий
    public abstract class Weapon : MonoBehaviour
    {
        public abstract void Attack();          // одиночный удар / выстрел
        public virtual void AttackHeld() { }   // зажатие (для лука — натяжение)
        public virtual void AttackReleased() { } // отпускание (для лука — выстрел)
    }
}