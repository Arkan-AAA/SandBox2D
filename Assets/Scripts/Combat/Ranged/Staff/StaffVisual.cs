using UnityEngine;
using UnityEngine.InputSystem;

public class StaffVisual : MonoBehaviour {
    [Header("References")]
    [SerializeField] private Camera _gameCamera;          // Перетащите камеру
    [SerializeField] private Transform _tipPoint;        // Точка вылета снарядов

    [Header("Settings")]
    [SerializeField] private float _rotationOffset = 0f; // Для спрайта, смотрящего вправо — 0. Если вверх — 90.
    [SerializeField] private bool _flipOnAngle = true;   // Зеркалить при повороте >90°?

    private Transform _staffTransform;

    private void Awake() {
        _staffTransform = transform;

        // Назначаем камеру, если не указали вручную
        if (_gameCamera == null)
            _gameCamera = Camera.main;

        if (_gameCamera == null)
            Debug.LogError("StaffVisual: нет камеры! Перетащите её в поле _gameCamera или поставьте тег MainCamera.");

        if (_tipPoint == null)
            _tipPoint = transform; // запасной вариант — центр объекта
    }

    private void Update() {
        if (_gameCamera == null) return;

        Vector2 aimPoint = GetAimPoint();
        RotateTo(aimPoint);
    }

    private Vector2 GetAimPoint() {
        // Приоритет: если есть геймпад и правый стик отклонён — используем его
        Gamepad gamepad = Gamepad.current;
        if (gamepad != null) {
            Vector2 rightStick = gamepad.rightStick.ReadValue();
            if (rightStick.sqrMagnitude > 0.1f) {
                // Возвращаем точку в 10 единицах от посоха в направлении стика
                return (Vector2)_staffTransform.position + rightStick.normalized * 10f;
            }
        }

        // Иначе — мышь
        Vector2 mouseScreen = Mouse.current.position.ReadValue();
        return _gameCamera.ScreenToWorldPoint(mouseScreen);
    }

    private void RotateTo(Vector2 targetWorldPoint) {
        Vector2 direction = (targetWorldPoint - (Vector2)_staffTransform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        _staffTransform.rotation = Quaternion.Euler(0f, 0f, angle + _rotationOffset);

        if (_flipOnAngle) {
            // Зеркалим спрайт по Y, если угол больше 90 или меньше -90
            _staffTransform.localScale = new Vector3(1, Mathf.Abs(angle) > 90f ? -1 : 1, 1);
        }
    }

    // Метод для получения направления выстрела (от TipPoint к цели)
    public Vector2 GetShootDirection() {
        if (_gameCamera == null) return Vector2.right;

        Vector2 aimPoint = GetAimPoint();
        return (aimPoint - (Vector2)_tipPoint.position).normalized;
    }

    // Точка спавна снаряда
    public Vector2 GetSpawnPoint() => _tipPoint.position;
}