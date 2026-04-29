namespace Satyr.Utils
{
    public static class LookDirectionHelper
    {
        public static float GetLookX()
        {
            if (GameInput.Instance.IsGamepadActive())
                return GameInput.Instance.GetGamepadLookVector().x;

            float mouseX = GameInput.Instance.GetMousePosition().x;
            float playerX = Player.Instance.GetPlayerScreenPosition().x;
            return mouseX - playerX;
        }
    }
}