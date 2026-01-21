using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;

    [Header("Hammer Jump")]
    public float chargeTime = 1f;
    public float floatUpForce = 3f;
    public float slamForce = 20f;

    Rigidbody rb;
    Vector2 moveInput;

    PlayerInputActions input;

    float jumpCharge;
    bool charging;
    bool grounded;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = true;

        input = new PlayerInputActions();

        input.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        input.Player.Move.canceled += _ => moveInput = Vector2.zero;

        input.Player.Jump.performed += _ => StartCharge();
        input.Player.Jump.canceled += _ => ReleaseJump();
    }

    void OnEnable() => input.Player.Enable();
    void OnDisable() => input.Player.Disable();

    void FixedUpdate()
    {
        grounded = IsGrounded();

        // movement
        Vector3 vel = rb.velocity;
        Vector3 move = new Vector3(moveInput.x, 0f, moveInput.y) * moveSpeed;
        rb.velocity = new Vector3(move.x, vel.y, move.z);

        // charging: float up
        if (charging)
        {
            jumpCharge += Time.fixedDeltaTime;
            jumpCharge = Mathf.Min(jumpCharge, chargeTime);

            rb.AddForce(Vector3.up * floatUpForce, ForceMode.Acceleration);
            rb.velocity = new Vector3(rb.velocity.x, Mathf.Min(rb.velocity.y, 2f), rb.velocity.z);
        }
    }

    void StartCharge()
    {
        if (!grounded) return;

        charging = true;
        jumpCharge = 0f;
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
    }

    void ReleaseJump()
    {
        if (!charging) return;

        charging = false;

        float power = jumpCharge / chargeTime;
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        rb.AddForce(Vector3.down * slamForce * Mathf.Max(power, 0.3f), ForceMode.Impulse);

        jumpCharge = 0f;
    }

    bool IsGrounded()
    {
        return Physics.Raycast(transform.position, Vector3.down, 1.1f);
    }
}

