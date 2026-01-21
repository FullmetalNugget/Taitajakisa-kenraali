using UnityEngine;
using System.Collections;

public class Movement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private float jumpChargeTime = 1f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask Ground;

    private Rigidbody rb;
    private Vector3 moveDirection;
    private bool isGrounded;
    private bool isChargingJump = false;
    private float jumpChargeTimer = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
            rb.constraints = RigidbodyConstraints.FreezeRotation;
        }
    }

    void Update()
    {
        // Check if grounded
        isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, Ground);

        // Get movement input
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        moveDirection = new Vector3(horizontal, 0f, vertical).normalized;

        // Jump input
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded && !isChargingJump)
        {
            StartJumpCharge();
        }

        // Charge jump while holding space
        if (isChargingJump)
        {
            jumpChargeTimer += Time.deltaTime;

            // Auto-jump after charge time
            if (jumpChargeTimer >= jumpChargeTime)
            {
                ExecuteJump();
            }

            // Or jump on key release
            if (Input.GetKeyUp(KeyCode.Space))
            {
                ExecuteJump();
            }
        }

        // Move player (if not charging jump)
        if (!isChargingJump)
        {
            MovePlayer();
        }
    }

    void StartJumpCharge()
    {
        isChargingJump = true;
        jumpChargeTimer = 0f;
        rb.velocity = new Vector3(rb.velocity.x * 0.1f, rb.velocity.y, rb.velocity.z * 0.1f); // Stop movement
    }

    void ExecuteJump()
    {
        if (isChargingJump)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isChargingJump = false;
            jumpChargeTimer = 0f;
        }
    }

    void MovePlayer()
    {
        if (moveDirection.magnitude >= 0.1f)
        {
            Vector3 moveVelocity = moveDirection * moveSpeed;
            rb.velocity = new Vector3(moveVelocity.x, rb.velocity.y, moveVelocity.z);
        }
        else
        {
            // Apply damping when no input
            rb.velocity = new Vector3(rb.velocity.x * 0.9f, rb.velocity.y, rb.velocity.z * 0.9f);
        }
    }

    // Helper method to check if player is falling (not grounded)
    public bool IsPlayerFalling()
    {
        return !isGrounded && rb.velocity.y < 0;
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}