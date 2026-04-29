using UnityEngine;
using Satyr.Utils;
using Combat;

public class ActiveWeapon : MonoBehaviour {
    
    public static ActiveWeapon Instance { get; private set; }

    [SerializeField] private Weapon currentWeapon;

    private void Awake() => Instance = this;

    public Weapon GetActiveWeapon() => currentWeapon;

    public void Attack() => currentWeapon.Attack();
    public void AttackHeld() => currentWeapon.AttackHeld();
    public void AttackReleased() => currentWeapon.AttackReleased();
    
        private void Update()
    {
        FollowLookDirection();
    }

    
    private void FollowLookDirection()
    {
        float lookX = LookDirectionHelper.GetLookX();

        if (lookX < 0f)
            transform.localScale = new Vector3(-1f, 1f, 1f);
        else if (lookX > 0f)
            transform.localScale = new Vector3(1f, 1f, 1f);
    }
}
