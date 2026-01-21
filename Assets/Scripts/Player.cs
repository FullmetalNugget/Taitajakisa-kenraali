using UnityEngine;
using System.Collections;

public class Movement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] public float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private float jumpChargeTime = 1f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask Ground;

    [Header("First Person Camera Settings")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float minVerticalAngle = -90f;
    [SerializeField] private float maxVerticalAngle = 90f;
    [SerializeField] private bool invertYAxis = false;

    [Header("Head Bob Settings")]
    [SerializeField] private bool enableHeadBob = true;
    [SerializeField] private float headBobFrequency = 1.5f;
    [SerializeField] private float headBobHeight = 0.1f;

    private Rigidbody rb;
    private Vector3 moveDirection;
    private bool isGrounded;
    private bool isChargingJump = false;
    private float jumpChargeTimer = 0f;
    private float cameraPitch = 0f;
    private float cameraYaw = 0f;
    private Vector3 originalCameraLocalPosition;
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
            cameraTransform = GetComponentInChildren<Camera>()?.transform;
            if (cameraTransform == null)
            {
                GameObject cameraObj = new GameObject("Player Camera");
                cameraObj.transform.parent = transform;
                cameraObj.transform.localPosition = Vector3.up * 1.6f;
                Camera cam = cameraObj.AddComponent<Camera>();
                cameraTransform = cam.transform;

                if (FindObjectOfType<AudioListener>() == null)
                {
                    cameraObj.AddComponent<AudioListener>();
                }
            }
        }

        originalCameraLocalPosition = cameraTransform.localPosition;
        cameraPitch = cameraTransform.localEulerAngles.x;
        cameraYaw = transform.eulerAngles.y;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        HandleCameraRotation();

        // Check if grounded
        isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, Ground);

        // Get movement input
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        // Calculate movement direction relative to camera
        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;

        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();

        moveDirection = (forward * vertical + right * horizontal).normalized;

        // Jump input - simplified
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

        // Apply head bob if moving
        if (enableHeadBob && moveDirection.magnitude > 0.1f && isGrounded && !isChargingJump)
        {
            ApplyHeadBob();
        }
        else
        {
            cameraTransform.localPosition = Vector3.Lerp(cameraTransform.localPosition,
                originalCameraLocalPosition, Time.deltaTime * 10f);
        }
    }

    void HandleCameraRotation()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * (invertYAxis ? 1f : -1f);

        cameraYaw += mouseX;
        transform.rotation = Quaternion.Euler(0f, cameraYaw, 0f);

        cameraPitch += mouseY;
        cameraPitch = Mathf.Clamp(cameraPitch, minVerticalAngle, maxVerticalAngle);

        cameraTransform.localRotation = Quaternion.Euler(cameraPitch, 0f, 0f);

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
        rb.linearVelocity = new Vector3(rb.linearVelocity.x * 0.1f, rb.linearVelocity.y, rb.linearVelocity.z * 0.1f);
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
            rb.linearVelocity = new Vector3(moveVelocity.x, rb.linearVelocity.y, moveVelocity.z);
        }
        else
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x * 0.9f, rb.linearVelocity.y, rb.linearVelocity.z * 0.9f);
        }
    }

    void ApplyHeadBob()
    {
        headBobTimer += Time.deltaTime * moveSpeed;
        float bobOffset = Mathf.Sin(headBobTimer * headBobFrequency) * headBobHeight;

        Vector3 newPosition = originalCameraLocalPosition;
        newPosition.y += bobOffset;

        cameraTransform.localPosition = Vector3.Lerp(cameraTransform.localPosition,
            newPosition, Time.deltaTime * 10f);
    }

    public bool IsPlayerFalling()
    {
        return !isGrounded && rb.linearVelocity.y < 0;
    }

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
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}