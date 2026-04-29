using UnityEngine;
using Satyr.Utils;

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
        float lookX = LookDirectionHelper.GetLookX();

        if (lookX < 0f)
            transform.localScale = new Vector3(-1f, 1f, 1f);
        else if (lookX > 0f)
            transform.localScale = new Vector3(1f, 1f, 1f);
    }
}
