using UnityEngine;

public class WeaponInput : MonoBehaviour {
    private WeaponInventory inventory;
    private bool _subscribed = false;

    private void Start() {
        inventory = GetComponent<WeaponInventory>();
        Subscribe();
    }

    private void Subscribe() {
        if (_subscribed) return;

        if (GameInput.Instance == null) {
            Debug.LogWarning("GameInput.Instance is null, will retry...");
            Invoke(nameof(Subscribe), 0.1f);
            return;
        }

        GameInput.Instance.OnPlayerAttack += OnAttack;
        GameInput.Instance.OnPlayerAttackHeld += OnAttackHeld;
        GameInput.Instance.OnPlayerAttackReleased += OnAttackReleased;
        GameInput.Instance.OnNextWeapon += OnNextWeapon;
        GameInput.Instance.OnPreviousWeapon += OnPreviousWeapon;
        GameInput.Instance.OnWeaponSlot += OnWeaponSlot;

        _subscribed = true;
        Debug.Log("WeaponInput subscribed to GameInput events");
    }

    private void OnAttack(object sender, System.EventArgs e) {
        ActiveWeapon.Instance?.Attack();
    }

    private void OnAttackHeld() {
        ActiveWeapon.Instance?.AttackHeld();
    }

    private void OnAttackReleased() {
        ActiveWeapon.Instance?.AttackReleased();
    }

    private void OnNextWeapon() {
        inventory?.NextWeapon();
    }

    private void OnPreviousWeapon() {
        inventory?.PreviousWeapon();
    }

    private void OnWeaponSlot(int slot) {
        inventory?.EquipWeapon(slot);
    }
}