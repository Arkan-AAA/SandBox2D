namespace Satyr.Utils {
    public static class LookDirectionHelper {
        public static float GetLookX() {
            // Правый стик — приоритет (прицеливание)
            if (GameInput.Instance.IsGamepadActive())
                return GameInput.Instance.GetGamepadLookVector().x;

            // Левый стик — если правый не используется
            if (GameInput.Instance.IsGamepadMoving())
                return GameInput.Instance.GetGamepadMoveVector().x;

            // Мышь — fallback
            float mouseX = GameInput.Instance.GetMousePosition().x;
            float playerX = Player.Instance.GetPlayerScreenPosition().x;
            return mouseX - playerX;
        }
    }
}