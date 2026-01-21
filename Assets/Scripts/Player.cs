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

    [Header("First Person Camera Settings")]
    [SerializeField] private Transform cameraTransform; // Assign the camera (usually child of player)
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float minVerticalAngle = -90f; // Maximum down angle
    [SerializeField] private float maxVerticalAngle = 90f;  // Maximum up angle
    [SerializeField] private bool invertYAxis = false;      // Option to invert mouse Y

    // Crouch settings (optional)
    [SerializeField] private bool enableCrouch = false;
    [SerializeField] private float crouchHeight = 1f;
    [SerializeField] private float normalHeight = 2f;
    [SerializeField] private float crouchSpeed = 2.5f;

    private Rigidbody rb;
    private Vector3 moveDirection;
    private bool isGrounded;
    private bool isChargingJump = false;
    private float jumpChargeTimer = 0f;

    // Camera variables
    private float cameraPitch = 0f; // Vertical rotation
    private float cameraYaw = 0f;   // Horizontal rotation
    private Vector3 originalCameraLocalPosition;
    private bool isCrouching = false;

    // Head bob variables (optional)
    [Header("Head Bob Settings (Optional)")]
    [SerializeField] private bool enableHeadBob = true;
    [SerializeField] private float headBobFrequency = 1.5f;
    [SerializeField] private float headBobHeight = 0.1f;
    private float headBobTimer = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
            rb.constraints = RigidbodyConstraints.FreezeRotation;
        }

        // Camera setup
        if (cameraTransform == null)
        {
            // Try to find camera in children
            cameraTransform = GetComponentInChildren<Camera>()?.transform;
            if (cameraTransform == null)
            {
                // Create a camera if none exists
                GameObject cameraObj = new GameObject("Player Camera");
                cameraObj.transform.parent = transform;
                cameraObj.transform.localPosition = Vector3.up * 0.5f;
                Camera cam = cameraObj.AddComponent<Camera>();
                cameraTransform = cam.transform;

                // Add audio listener if not present
                if (FindObjectOfType<AudioListener>() == null)
                {
                    cameraObj.AddComponent<AudioListener>();
                }
            }
        }

        // Store original camera position
        originalCameraLocalPosition = cameraTransform.localPosition;

        // Initialize camera rotation to current orientation
        cameraPitch = cameraTransform.localEulerAngles.x;
        cameraYaw = transform.eulerAngles.y;

        // Lock cursor to screen
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // Handle camera rotation
        HandleCameraRotation();

        // Check if grounded
        isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, Ground);

        // Handle crouch input
        if (enableCrouch && Input.GetKeyDown(KeyCode.LeftControl))
        {
            ToggleCrouch();
        }

        // Get movement input - now relative to camera direction
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        // Calculate movement direction relative to camera
        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;

        // Flatten vectors for ground movement
        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();

        moveDirection = (forward * vertical + right * horizontal).normalized;

        // Jump input
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded && !isChargingJump && !isCrouching)
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

        // Apply head bob if moving
        if (enableHeadBob && moveDirection.magnitude > 0.1f && isGrounded && !isChargingJump)
        {
            ApplyHeadBob();
        }
        else
        {
            // Reset camera position
            cameraTransform.localPosition = Vector3.Lerp(cameraTransform.localPosition,
                originalCameraLocalPosition, Time.deltaTime * 10f);
        }
    }

    void HandleCameraRotation()
    {
        // Get mouse input
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * (invertYAxis ? 1f : -1f);

        // Rotate player horizontally (yaw)
        cameraYaw += mouseX;
        transform.rotation = Quaternion.Euler(0f, cameraYaw, 0f);

        // Rotate camera vertically (pitch)
        cameraPitch += mouseY;
        cameraPitch = Mathf.Clamp(cameraPitch, minVerticalAngle, maxVerticalAngle);

        // Apply rotation to camera
        cameraTransform.localRotation = Quaternion.Euler(cameraPitch, 0f, 0f);

        // Optional: Escape key to show cursor
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = Cursor.lockState == CursorLockMode.Locked ?
                CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = !Cursor.visible;
        }
    }

    void StartJumpCharge()
    {
        isChargingJump = true;
        jumpChargeTimer = 0f;
        rb.linearVelocity = new Vector3(rb.linearVelocity.x * 0.1f, rb.linearVelocity.y, rb.linearVelocity.z * 0.1f); // Stop movement
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
        // Adjust speed if crouching
        float currentSpeed = isCrouching ? crouchSpeed : moveSpeed;

        if (moveDirection.magnitude >= 0.1f)
        {
            Vector3 moveVelocity = moveDirection * currentSpeed;
            rb.linearVelocity = new Vector3(moveVelocity.x, rb.linearVelocity.y, moveVelocity.z);
        }
        else
        {
            // Apply damping when no input
            rb.linearVelocity = new Vector3(rb.linearVelocity.x * 0.9f, rb.linearVelocity.y, rb.linearVelocity.z * 0.9f);
        }
    }

    void ToggleCrouch()
    {
        isCrouching = !isCrouching;

        // Adjust camera height when crouching
        Vector3 newCameraPosition = originalCameraLocalPosition;
        newCameraPosition.y = isCrouching ? crouchHeight : normalHeight;
        cameraTransform.localPosition = newCameraPosition;
    }

    void ApplyHeadBob()
    {
        // Simple head bob simulation
        headBobTimer += Time.deltaTime * moveSpeed;
        float bobOffset = Mathf.Sin(headBobTimer * headBobFrequency) * headBobHeight;

        Vector3 newPosition = originalCameraLocalPosition;
        newPosition.y += bobOffset;

        cameraTransform.localPosition = Vector3.Lerp(cameraTransform.localPosition,
            newPosition, Time.deltaTime * 10f);
    }

    // Helper method to check if player is falling (not grounded)
    public bool IsPlayerFalling()
    {
        return !isGrounded && rb.linearVelocity.y < 0;
    }

    // Optional: Add camera shake when landing
    public void AddCameraShake(float intensity, float duration)
    {
        StartCoroutine(CameraShake(intensity, duration));
    }

    IEnumerator CameraShake(float intensity, float duration)
    {
        Vector3 originalPos = cameraTransform.localPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * intensity;
            float y = Random.Range(-1f, 1f) * intensity;

            cameraTransform.localPosition = new Vector3(x, originalPos.y + y, originalPos.z);

            elapsed += Time.deltaTime;
            yield return null;
        }

        cameraTransform.localPosition = originalPos;
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }

    void OnDestroy()
    {
        // Reset cursor when object is destroyed
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}