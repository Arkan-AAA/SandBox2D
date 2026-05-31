using UnityEngine;

namespace Satyr.Utils {
    public static class LookDirectionHelper {
        public static float GetLookX() {
            // Проверяем существование GameInput и Player
            if (GameInput.Instance == null) return 0f;
            if (Player.Instance == null) return 0f;

            // Правый стик — приоритет (прицеливание)
            if (GameInput.Instance.IsGamepadActive())
                return GameInput.Instance.GetGamepadLookVector().x;

            // Левый стик — если правый не используется
            if (GameInput.Instance.IsGamepadMoving())
                return GameInput.Instance.GetGamepadMoveVector().x;

            // Мышь — fallback
            Vector3 mousePos = GameInput.Instance.GetMousePosition();
            Vector3 playerScreenPos = Player.Instance.GetPlayerScreenPosition();

            return mousePos.x - playerScreenPos.x;
        }

        public static Vector2 GetLookDirection() {
            if (GameInput.Instance == null) return Vector2.right;
            if (Player.Instance == null) return Vector2.right;

            if (GameInput.Instance.IsGamepadActive()) {
                return GameInput.Instance.GetGamepadLookVector();
            }

            Vector3 mousePos = GameInput.Instance.GetMousePosition();
            Vector3 playerScreenPos = Player.Instance.GetPlayerScreenPosition();
            return (mousePos - playerScreenPos).normalized;
        }
    }
}