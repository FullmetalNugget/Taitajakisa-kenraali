using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class Player : MonoBehaviour
{
    public float moveSpeed = 5f;

    Rigidbody rb;
    Collider col;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        rb.freezeRotation = true;
    }

    void Update()
    {
        Vector2 move = Vector2.zero;

        // KEYBOARD (WASD)
        if (Keyboard.current != null)
        {
            if (Keyboard.current.wKey.isPressed) move.y += 1;
            if (Keyboard.current.sKey.isPressed) move.y -= 1;
            if (Keyboard.current.aKey.isPressed) move.x -= 1;
            if (Keyboard.current.dKey.isPressed) move.x += 1;
        }

        // GAMEPAD (LEFT STICK)
        if (Gamepad.current != null)
        {
            move += Gamepad.current.leftStick.ReadValue();
        }

        move = Vector2.ClampMagnitude(move, 1f);

        Vector3 vel = rb.velocity;
        rb.velocity = new Vector3(
            move.x * moveSpeed,
            vel.y,
            move.y * moveSpeed
        );
    }
}

