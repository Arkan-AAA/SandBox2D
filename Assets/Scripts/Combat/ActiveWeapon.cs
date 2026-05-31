using Combat;
using Satyr.Utils;
using UnityEngine;

public class ActiveWeapon : MonoBehaviour {
    public static ActiveWeapon Instance { get; private set; }

    private Weapon _currentWeapon;

    private void Awake() {
        if (Instance == null) {
            Instance = this;
        }
        else {
            Destroy(gameObject);
        }
    }

    public Weapon GetActiveWeapon() => _currentWeapon;

    public void SetWeapon(Weapon weapon) {
        _currentWeapon = weapon;
        Debug.Log($"ActiveWeapon set to: {weapon?.name ?? "null"}");
    }

    public void Attack() {
        if (_currentWeapon != null) {
            Debug.Log($"Attacking with: {_currentWeapon.name}");
            _currentWeapon.Attack();
        }
    }

    public void AttackHeld() {
        if (_currentWeapon != null) _currentWeapon.AttackHeld();
    }

    public void AttackReleased() {
        if (_currentWeapon != null) _currentWeapon.AttackReleased();
    }

    private void Update() {
        FollowLookDirection();
    }

    private void FollowLookDirection() {
        if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Playing) return;
        if (Player.Instance == null) return;

        float lookX = LookDirectionHelper.GetLookX();

        if (lookX < 0f) {
            transform.localScale = new Vector3(-1f, 1f, 1f);
        }
        else if (lookX > 0f) {
            transform.localScale = new Vector3(1f, 1f, 1f);
        }
    }
}