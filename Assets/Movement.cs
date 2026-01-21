using UnityEngine;
using System.Collections;

public class Movement : MonoBehaviour
{

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
            rb.constraints = RigidbodyConstraints.FreezeRotation;
        }

        // Lock cursor for first-person
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        HandleCamera();

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

    void HandleCamera()
    {
        if (playerCamera == null) return;

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // Rotate player horizontally
        transform.Rotate(Vector3.up * mouseX);

        // Rotate camera vertically
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -maxLookAngle, maxLookAngle);
        playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
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