using UnityEngine;

public class PlayerVisual : MonoBehaviour
{
    private Animator animator;

    private const string IS_RUNNING = "IsRunning";

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        animator.SetBool(IS_RUNNING, Player.Instance.IsRunning());
        AdjustPlayerFacingDirection();
    }

    private void AdjustPlayerFacingDirection()
    {
        Vector3 mousePos = GameInput.Instance.GetMousePosition();
        Vector3 playerPosition = Player.Instance.GetPlayerScreenPosition();

        if (mousePos.x < playerPosition.x)
        {
            transform.rotation = Quaternion.Euler(0f, 180f, 0f);
        }
        else
        {
            transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        }
    }
}
