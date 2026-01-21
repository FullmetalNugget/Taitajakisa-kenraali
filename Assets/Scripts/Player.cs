using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class Player : MonoBehaviour
{
    public float moveSpeed = 5f;

    [Header("Hammer Jump")]
    public float chargeTime = 1f;
    public float floatSpeed = 2f;
    public float slamForce = 25f;

    Rigidbody rb;
    Collider col;

    bool charging;
    float chargeTimer;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        rb.freezeRotation = true;
    }

    void Update()
    {
        Vector2 move = Vector2.zero;
        bool jumpHeld = false;
        bool jumpPressed = false;
        bool jumpReleased = false;

        // KEYBOARD
        if (Keyboard.current != null)
        {
            if (Keyboard.current.wKey.isPressed) move.y += 1;
            if (Keyboard.current.sKey.isPressed) move.y -= 1;
            if (Keyboard.current.aKey.isPressed) move.x -= 1;
            if (Keyboard.current.dKey.isPressed) move.x += 1;

            jumpHeld = Keyboard.current.spaceKey.isPressed;
            jumpPressed = Keyboard.current.spaceKey.wasPressedThisFrame;
            jumpReleased = Keyboard.current.spaceKey.wasReleasedThisFrame;
        }

        // GAMEPAD
        if (Gamepad.current != null)
        {
            move += Gamepad.current.leftStick.ReadValue();

            jumpHeld |= Gamepad.current.buttonSouth.isPressed;
            jumpPressed |= Gamepad.current.buttonSouth.wasPressedThisFrame;
            jumpReleased |= Gamepad.current.buttonSouth.wasReleasedThisFrame;
        }

        move = Vector2.ClampMagnitude(move, 1f);

        rb.velocity = new Vector3(
            move.x * moveSpeed,
            rb.velocity.y,
            move.y * moveSpeed
        );

        // HAMMER JUMP LOGIC
        if (jumpPressed && IsGrounded())
        {
            charging = true;
            chargeTimer = 0f;
            rb.useGravity = false;
            rb.velocity = Vector3.zero;
        }

        if (charging && jumpHeld)
        {
            chargeTimer += Time.deltaTime;
            rb.velocity = Vector3.up * floatSpeed;

            if (chargeTimer >= chargeTime)
                Slam();
        }

        if (charging && jumpReleased)
        {
            Slam();
        }
    }

    void Slam()
    {
        charging = false;
        rb.useGravity = true;
        rb.velocity = Vector3.zero;
        rb.AddForce(Vector3.down * slamForce, ForceMode.Impulse);
    }

    bool IsGrounded()
    {
        return Physics.Raycast(
            col.bounds.center,
            Vector3.down,
            col.bounds.extents.y + 0.05f
        );
    }
}

