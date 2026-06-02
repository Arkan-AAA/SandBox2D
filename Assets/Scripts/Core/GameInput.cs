using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class GameInput : MonoBehaviour {
    public static GameInput Instance { get; private set; }

    private PlayerInputActions playerInputActions;

    public event EventHandler OnPlayerAttack;
    public event Action OnPlayerAttackHeld;
    public event Action OnPlayerAttackReleased;
    public event Action OnPlayerDash;
    public event Action OnNextWeapon;
    public event Action OnPreviousWeapon;
    public event Action<int> OnWeaponSlot;
    public event Action OnPause;
    public event Action OnInventory;

    private void Awake() {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        playerInputActions = new PlayerInputActions();
        playerInputActions.Enable();
        playerInputActions.UI.Disable();

        playerInputActions.Player.Fire.started += PlayerAttack_started;
        playerInputActions.Player.Fire.performed += _ => OnPlayerAttackHeld?.Invoke();
        playerInputActions.Player.Fire.canceled += _ => OnPlayerAttackReleased?.Invoke();
        playerInputActions.Player.Dash.started += _ => OnPlayerDash?.Invoke();
        playerInputActions.Player.NextWeapon.performed += _ => OnNextWeapon?.Invoke();
        playerInputActions.Player.PreviousWeapon.performed += _ => OnPreviousWeapon?.Invoke();
        playerInputActions.Player.Slot1.performed += _ => OnWeaponSlot?.Invoke(0);
        playerInputActions.Player.Slot2.performed += _ => OnWeaponSlot?.Invoke(1);
        playerInputActions.Player.Slot3.performed += _ => OnWeaponSlot?.Invoke(2);
        playerInputActions.Player.Pause.performed += _ => OnPause?.Invoke();
        playerInputActions.Player.Inventory.performed += _ => OnInventory?.Invoke();
    }

    private void PlayerAttack_started(InputAction.CallbackContext obj) {
        OnPlayerAttack?.Invoke(this, EventArgs.Empty);
    }

    public Vector2 GetMovementVector() => playerInputActions.Player.Move.ReadValue<Vector2>();
    public Vector3 GetMousePosition() => Mouse.current.position.ReadValue();
    public Vector2 GetGamepadLookVector() => Gamepad.current?.rightStick.ReadValue() ?? Vector2.zero;
    public bool IsGamepadActive() => Gamepad.current != null && GetGamepadLookVector().sqrMagnitude > 0.1f;
    public Vector2 GetGamepadMoveVector() => Gamepad.current?.leftStick.ReadValue() ?? Vector2.zero;
    public bool IsGamepadMoving() => Gamepad.current != null && GetGamepadMoveVector().sqrMagnitude > 0.1f;

    public void DisableInput() => playerInputActions.Player.Disable();
    public void EnableInput() => playerInputActions.Player.Enable();

    public void EnableUIInput() => playerInputActions.UI.Enable();
    public void DisableUIInput() => playerInputActions.UI.Disable();

    /// <summary>Сбросить текущий выбранный объект в EventSystem.</summary>
    public void ClearSelectedObject() {
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);
    }

    /// <summary>Установить выбранный объект в EventSystem.</summary>
    public void SetSelectedObject(GameObject obj) {
        if (EventSystem.current != null && obj != null)
            EventSystem.current.SetSelectedGameObject(obj);
    }
}