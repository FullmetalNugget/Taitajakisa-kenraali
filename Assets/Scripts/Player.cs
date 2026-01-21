using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class Player : MonoBehaviour
{
    public float moveSpeed = 5f;

    [Header("Hammer Jump")]
    public float chargeTime = 1f;
    public float floatHeight = 2f;
    public float slamForce = 25f;

    Rigidbody rb;
    Collider col;

    float chargeTimer;
    float startY;
    bool charging;
    bool slamming;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        rb.freezeRotation = true;
    }

    void FixedUpdate()
    {
        var gamepad = Gamepad.current;
        if (gamepad == null) return;

        // movement (left stick)
        Vector2 stick = gamepad.leftStick.ReadValue();
        Vector3 vel = rb.velocity;
        rb.velocity = new Vector3(stick.x * moveSpeed, vel.y, stick.y * moveSpeed);

        // jump input (A button)
        if (gamepad.buttonSouth.wasPressedThisFrame)
            BeginCharge();

        if (gamepad.buttonSouth.isPressed && charging)
            ChargeFloat();

        if (gamepad.buttonSouth.wasReleasedThisFrame)
            ReleaseJump();

        if (slamming && IsGrounded())
            slamming = false;
    }

    void BeginCharge()
    {
        if (!IsGrounded()) return;

        charging = true;
        slamming = false;
        chargeTimer = 0f;

        startY = rb.position.y;
        rb.velocity = Vector3.zero;
        rb.useGravity = false;
    }

    void ChargeFloat()
    {
        chargeTimer += Time.fixedDeltaTime;
        float t = Mathf.Clamp01(chargeTimer / chargeTime);

        float targetY = startY + floatHeight * t;
        rb.MovePosition(new Vector3(rb.position.x, targetY, rb.position.z));

        if (t >= 1f)
            ReleaseJump();
    }

    void ReleaseJump()
    {
        if (!charging) return;

        charging = false;
        slamming = true;

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

