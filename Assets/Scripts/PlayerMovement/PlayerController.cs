using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(Animator))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float sprintSpeed = 8f;
    [SerializeField] private float crouchSpeed = 2.5f;

    [Header("Acceleration")]
    [SerializeField] private float acceleration = 35f;
    [SerializeField] private float deceleration = 25f;
    [SerializeField] private float airControlMultiplier = 0.4f;

    [Header("Jump")]
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float jumpCooldown = 0.2f;
    [SerializeField] private float coyoteTime = 0.15f;

    [Header("Ground Check")]
    [SerializeField] private float groundCheckDistance = 0.2f;
    [SerializeField] private LayerMask groundLayer = ~0;

    [Header("Crouch")]
    [SerializeField] private float crouchHeight = 1f;
    [SerializeField] private float crouchSmoothSpeed = 12f;

    [Header("Camera")]
    [SerializeField] private Camera playerCamera;

    [SerializeField] private float mouseSensitivity = 0.1f;
    [SerializeField] private float gamepadSensitivity = 120f;

    [SerializeField] private float xClamp = 85f;

    [SerializeField] private bool smoothCamera = true;
    [SerializeField] private float horizontalSmoothSpeed = 15f;
    [SerializeField] private float verticalSmoothSpeed = 15f;

    [Header("Lean")]
    [SerializeField] private float maxLeanAngle = 8f;
    [SerializeField] private float leanSmoothSpeed = 8f;

    [Header("Camera Tilt")]
    [SerializeField] private float movementTiltAmount = 3f;
    [SerializeField] private float movementTiltSmooth = 6f;

    [Header("FOV")]
    [SerializeField] private float normalFOV = 60f;
    [SerializeField] private float sprintFOV = 70f;
    [SerializeField] private float crouchFOV = 50f;
    [SerializeField] private float fovSmooth = 5f;

    [Header("Input")]
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference lookAction;
    [SerializeField] private InputActionReference jumpAction;
    [SerializeField] private InputActionReference sprintAction;
    [SerializeField] private InputActionReference crouchAction;

    private Rigidbody rb;
    private CapsuleCollider capsule;
    private Animator animator;
    private PlayerInput playerInput;

    private Vector2 moveInput;
    private Vector2 lookInput;

    private bool isGrounded;
    private bool isSprinting;
    private bool isCrouching;

    private float currentSpeed;

    private float xRotation;
    private float yRotation;

    private float lastJumpTime;
    private float lastGroundedTime;

    private float standingHeight;
    private Vector3 standingCenter;

    private float targetCapsuleHeight;
    private Vector3 targetCapsuleCenter;

    private float currentLean;
    private float currentTilt;

    private static readonly int SpeedHash =
        Animator.StringToHash("Speed");

    private static readonly int GroundedHash =
        Animator.StringToHash("isGrounded");

    private static readonly int CrouchingHash =
        Animator.StringToHash("isCrouching");

    private static readonly int JumpHash =
        Animator.StringToHash("Jump");

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        capsule = GetComponent<CapsuleCollider>();
        animator = GetComponent<Animator>();
        playerInput = GetComponent<PlayerInput>();

        rb.freezeRotation = true;

        if (playerCamera == null)
            playerCamera = Camera.main;
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        standingHeight = capsule.height;
        standingCenter = capsule.center;

        targetCapsuleHeight = standingHeight;
        targetCapsuleCenter = standingCenter;

        currentSpeed = moveSpeed;
    }

    private void OnEnable()
    {
        moveAction.action.Enable();
        lookAction.action.Enable();
        jumpAction.action.Enable();
        sprintAction.action.Enable();
        crouchAction.action.Enable();

        moveAction.action.performed += OnMove;
        moveAction.action.canceled += OnMove;

        lookAction.action.performed += OnLook;
        lookAction.action.canceled += OnLook;

        jumpAction.action.performed += OnJump;
    }

    private void OnDisable()
    {
        moveAction.action.performed -= OnMove;
        moveAction.action.canceled -= OnMove;

        lookAction.action.performed -= OnLook;
        lookAction.action.canceled -= OnLook;

        jumpAction.action.performed -= OnJump;
    }

    private void Update()
    {
        GroundCheck();

        HandleCrouch();

        UpdateMovementState();

        UpdateAnimator();

        CameraLook();

        UpdateCameraTilt();

        UpdateFOV();
    }

    private void FixedUpdate()
    {
        HandleMovement();
    }

    private void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    private void OnLook(InputAction.CallbackContext context)
    {
        lookInput = context.ReadValue<Vector2>();
    }

    private void OnJump(InputAction.CallbackContext context)
    {
        Jump();
    }

    private void UpdateMovementState()
    {
        bool moving = moveInput.sqrMagnitude > 0.01f;

        bool sprintPressed =
            sprintAction != null &&
            sprintAction.action.IsPressed();

        isSprinting =
            sprintPressed &&
            moving &&
            !isCrouching &&
            isGrounded;

        if (isCrouching)
            currentSpeed = crouchSpeed;
        else if (isSprinting)
            currentSpeed = sprintSpeed;
        else
            currentSpeed = moveSpeed;
    }

    private void HandleMovement()
    {
        if (playerCamera == null)
            return;

        Vector3 forward = playerCamera.transform.forward;
        Vector3 right = playerCamera.transform.right;

        forward.y = 0f;
        right.y = 0f;

        forward.Normalize();
        right.Normalize();

        Vector3 moveDirection =
            (forward * moveInput.y +
             right * moveInput.x).normalized;

        float control =
            isGrounded
            ? 1f
            : airControlMultiplier;

        Vector3 targetVelocity =
            moveDirection *
            currentSpeed *
            control;

        Vector3 velocity = rb.linearVelocity;

        Vector3 horizontalVelocity =
            new Vector3(
                velocity.x,
                0f,
                velocity.z
            );

        Vector3 targetHorizontal =
            new Vector3(
                targetVelocity.x,
                0f,
                targetVelocity.z
            );

        float accel =
            moveDirection.sqrMagnitude > 0.01f
            ? acceleration
            : deceleration;

        horizontalVelocity = Vector3.MoveTowards(
            horizontalVelocity,
            targetHorizontal,
            accel * Time.fixedDeltaTime
        );

        rb.linearVelocity = new Vector3(
            horizontalVelocity.x,
            velocity.y,
            horizontalVelocity.z
        );
    }

    private void Jump()
    {
        bool canJump =
            Time.time <
            lastGroundedTime + coyoteTime;

        if (!canJump)
            return;

        if (Time.time <
            lastJumpTime + jumpCooldown)
            return;

        lastJumpTime = Time.time;

        Vector3 velocity = rb.linearVelocity;
        velocity.y = 0f;

        rb.linearVelocity = velocity;

        rb.AddForce(
            Vector3.up * jumpForce,
            ForceMode.Impulse
        );

        animator.ResetTrigger(JumpHash);
        animator.SetTrigger(JumpHash);

        isGrounded = false;
    }

    private void GroundCheck()
    {
        Vector3 origin =
            transform.position +
            Vector3.up * 0.1f;

        float rayLength =
            (capsule.height / 2f) +
            groundCheckDistance;

        isGrounded = Physics.SphereCast(
            origin,
            capsule.radius * 0.9f,
            Vector3.down,
            out _,
            rayLength,
            groundLayer,
            QueryTriggerInteraction.Ignore
        );

        if (isGrounded)
            lastGroundedTime = Time.time;
    }

    private void HandleCrouch()
    {
        bool crouchPressed =
            crouchAction != null &&
            crouchAction.action.IsPressed();

        if (crouchPressed)
        {
            isCrouching = true;

            targetCapsuleHeight = crouchHeight;

            targetCapsuleCenter = new Vector3(
                standingCenter.x,
                crouchHeight / 2f,
                standingCenter.z
            );
        }
        else if (CanStandUp())
        {
            isCrouching = false;

            targetCapsuleHeight = standingHeight;
            targetCapsuleCenter = standingCenter;
        }

        capsule.height = Mathf.Lerp(
            capsule.height,
            targetCapsuleHeight,
            Time.deltaTime * crouchSmoothSpeed
        );

        capsule.center = Vector3.Lerp(
            capsule.center,
            targetCapsuleCenter,
            Time.deltaTime * crouchSmoothSpeed
        );
    }

    private bool CanStandUp()
    {
        float radius = capsule.radius * 0.95f;

        float halfHeight =
            standingHeight / 2f;

        Vector3 center =
            transform.position +
            Vector3.up * halfHeight;

        Vector3 point1 =
            center +
            Vector3.up *
            (halfHeight - radius);

        Vector3 point2 =
            center -
            Vector3.up *
            (halfHeight - radius);

        Collider[] hits =
            Physics.OverlapCapsule(
                point1,
                point2,
                radius,
                ~0,
                QueryTriggerInteraction.Ignore
            );

        foreach (Collider hit in hits)
        {
            if (hit.transform.root == transform)
                continue;

            return false;
        }

        return true;
    }

    private void UpdateAnimator()
    {
        bool moving =
            moveInput.sqrMagnitude > 0.01f;

        float speed = 0f;

        if (moving)
        {
            if (isCrouching)
                speed = 0.3f;
            else if (isSprinting)
                speed = 1f;
            else
                speed = 0.5f;
        }

        animator.SetFloat(
            SpeedHash,
            speed,
            0.1f,
            Time.deltaTime
        );

        animator.SetBool(
            GroundedHash,
            isGrounded
        );

        animator.SetBool(
            CrouchingHash,
            isCrouching
        );
    }

    private void CameraLook()
    {
        bool mouseScheme =
            playerInput.currentControlScheme != null &&
            playerInput.currentControlScheme.Contains("Mouse");

        float x;
        float y;

        if (mouseScheme)
        {
            x = lookInput.x * mouseSensitivity;
            y = lookInput.y * mouseSensitivity;
        }
        else
        {
            x = lookInput.x *
                gamepadSensitivity *
                Time.deltaTime;

            y = lookInput.y *
                gamepadSensitivity *
                Time.deltaTime;
        }

        yRotation += x;

        xRotation -= y;

        xRotation = Mathf.Clamp(
            xRotation,
            -xClamp,
            xClamp
        );

        Quaternion bodyRotation =
            Quaternion.Euler(
                0f,
                yRotation,
                0f
            );

        transform.rotation = smoothCamera
            ? Quaternion.Lerp(
                transform.rotation,
                bodyRotation,
                Time.deltaTime *
                horizontalSmoothSpeed
            )
            : bodyRotation;

        float targetLean =
            -moveInput.x * maxLeanAngle;

        currentLean = Mathf.Lerp(
            currentLean,
            targetLean,
            Time.deltaTime * leanSmoothSpeed
        );

        Quaternion cameraRotation =
            Quaternion.Euler(
                xRotation,
                0f,
                currentLean + currentTilt
            );

        playerCamera.transform.localRotation =
            smoothCamera
            ? Quaternion.Lerp(
                playerCamera.transform.localRotation,
                cameraRotation,
                Time.deltaTime *
                verticalSmoothSpeed
            )
            : cameraRotation;

        playerCamera.transform.localPosition = Vector3.Lerp(
                playerCamera.transform.localPosition,
                capsule.center + Vector3.up * (capsule.height * 0.5f - 0.1f),
                Time.deltaTime * crouchSmoothSpeed
            );
    }

    private void UpdateCameraTilt()
    {
        float targetTilt =
            -rb.linearVelocity.x *
            movementTiltAmount *
            0.1f;

        currentTilt = Mathf.Lerp(
            currentTilt,
            targetTilt,
            Time.deltaTime *
            movementTiltSmooth
        );
    }

    private void UpdateFOV()
    {
        float targetFOV =
            isSprinting
            ? sprintFOV
            : (isCrouching
                ? crouchFOV
                : normalFOV);

        playerCamera.fieldOfView =
            Mathf.Lerp(
                playerCamera.fieldOfView,
                targetFOV,
                Time.deltaTime * fovSmooth
            );
    }
}