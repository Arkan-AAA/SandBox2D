using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [SerializeField] private float speed = 5f;
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        Vector2 input = Vector2.zero;

        if (Keyboard.current.wKey.isPressed) input.y =  1f;
        if (Keyboard.current.sKey.isPressed) input.y = -1f;
        if (Keyboard.current.aKey.isPressed) input.x = -1f;
        if (Keyboard.current.dKey.isPressed) input.x =  1f;

        rb.MovePosition(rb.position + input.normalized * speed * Time.fixedDeltaTime);
    }
}