using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameInput : MonoBehaviour {
    public static GameInput Instance { get; private set; }

    private PlayerInputActions playerInputActions;

    public event EventHandler OnPlayerAttack;
    public event Action OnPlayerAttackHeld;
    public event Action OnPlayerAttackReleased;
    public event Action OnPlayerDash;

    private void Awake() {
        Instance = this;
        playerInputActions = new PlayerInputActions();
        playerInputActions.Enable();

        playerInputActions.Player.Fire.started += PlayerAttack_started;
        playerInputActions.Player.Fire.performed += _ => OnPlayerAttackHeld?.Invoke();
        playerInputActions.Player.Fire.canceled += _ => OnPlayerAttackReleased?.Invoke();
        playerInputActions.Player.Dash.started += _ => OnPlayerDash?.Invoke();
    }

    private void PlayerAttack_started(InputAction.CallbackContext obj) {
        OnPlayerAttack?.Invoke(this, EventArgs.Empty);
    }

    public Vector2 GetMovementVector() {
        Vector2 inputVector = playerInputActions.Player.Move.ReadValue<Vector2>();
        return inputVector;
    }

    public Vector3 GetMousePosition() {
        Vector3 mousePos = Mouse.current.position.ReadValue();
        return mousePos;
    }

    public Vector2 GetGamepadLookVector() {
        if (Gamepad.current == null)
            return Vector2.zero;
        return Gamepad.current.rightStick.ReadValue();
    }

    public bool IsGamepadActive() {
        return Gamepad.current != null
            && Gamepad.current.rightStick.ReadValue().sqrMagnitude > 0.1f;
    }

    public Vector2 GetGamepadMoveVector() {
        if (Gamepad.current == null)
            return Vector2.zero;
        return Gamepad.current.leftStick.ReadValue();
    }

    public bool IsGamepadMoving() {
        return Gamepad.current != null && Gamepad.current.leftStick.ReadValue().sqrMagnitude > 0.1f;
    }
}
