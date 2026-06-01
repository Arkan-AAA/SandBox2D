using UnityEngine;

public class StaffVisual : MonoBehaviour {
    [Header("References")]
    [SerializeField] private Camera _gameCamera;
    [SerializeField] private Transform _tipPoint;

    [Header("Settings")]
    [SerializeField] private float _rotationOffset = 0f;
    [SerializeField] private bool _flipOnAngle = true;

    private Transform _staffTransform;
    private bool _facingLeft = false;

    private void Awake() {
        _staffTransform = transform;

        if (_gameCamera == null)
            _gameCamera = Camera.main;

        if (_tipPoint == null)
            _tipPoint = transform;
    }

    private void Update() {
        if (_gameCamera == null) return;
        RotateTo(GetAimPoint());
    }

    private Vector2 GetAimPoint() {
        if (GameInput.Instance != null && GameInput.Instance.IsGamepadActive()) {
            Vector2 stickDir = GameInput.Instance.GetGamepadLookVector().normalized;
            return (Vector2)_staffTransform.position + stickDir * 10f;
        }

        Vector3 mouseScreen = GameInput.Instance != null
            ? GameInput.Instance.GetMousePosition()
            : Input.mousePosition;

        mouseScreen.z = Mathf.Abs(_gameCamera.transform.position.z);
        return _gameCamera.ScreenToWorldPoint(mouseScreen);
    }

    private void RotateTo(Vector2 targetWorldPoint) {
        Vector2 direction = (targetWorldPoint - (Vector2)_staffTransform.position).normalized;
        if (direction == Vector2.zero) return;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        if (!_flipOnAngle) {
            _staffTransform.rotation = Quaternion.Euler(0f, 0f, angle + _rotationOffset);
            return;
        }

        _facingLeft = Mathf.Abs(angle) > 90f;

        if (_facingLeft) {
            // При scaleY = -1 локальная Y-ось перевёрнута.
            // Чтобы посох следовал за мышью корректно — инвертируем Y при расчёте угла,
            // тогда вращение компенсирует переворот оси.
            float correctedAngle = Mathf.Atan2(-direction.y, -direction.x) * Mathf.Rad2Deg;
            _staffTransform.rotation = Quaternion.Euler(0f, 0f, correctedAngle + _rotationOffset);
            _staffTransform.localScale = new Vector3(1f, -1f, 1f);
        }
        else {
            _staffTransform.rotation = Quaternion.Euler(0f, 0f, angle + _rotationOffset);
            _staffTransform.localScale = new Vector3(1f, 1f, 1f);
        }
    }

    /// <summary>Направление выстрела — всегда от tipPoint к мировой позиции мыши.</summary>
    public Vector2 GetShootDirection() {
        if (_gameCamera == null) return Vector2.right;
        Vector2 aimPoint = GetAimPoint();
        Vector2 dir = aimPoint - (Vector2)_tipPoint.position;
        return dir.sqrMagnitude > 0.001f ? dir.normalized : (Vector2)_staffTransform.right;
    }

    /// <summary>Мировая позиция кончика посоха (точка спавна снаряда).</summary>
    public Vector2 GetSpawnPoint() => _tipPoint.position;
}