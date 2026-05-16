using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float sprintSpeed = 8f;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float jumpCooldown = 0.2f;
    [SerializeField] private float groundCheckDistance = 0.15f;
    [SerializeField] private LayerMask groundLayer = ~0;

    [Header("Crouch")]
    [SerializeField] private float crouchHeight = 1f;
    [SerializeField] private float standingHeight = 2f;
    [SerializeField] private float crouchSpeed = 2.5f;

    [Header("Camera")]
    [SerializeField] private float mouseSensitivity = 0.1f;
    [SerializeField] private float gamepadSensitivity = 120f;
    [SerializeField] private float cameraOffsetY = 0.1f;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float xClamp = 85f;

    [Header("Input")]
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference lookAction;
    [SerializeField] private InputActionReference jumpAction;
    [SerializeField] private InputActionReference sprintAction;
    [SerializeField] private InputActionReference crouchAction;

    [Header("Footstep Sounds")]
    [SerializeField] private AudioSource footstepAudioSource;
    [SerializeField] private float footstepCooldown = 0.5f;
    [SerializeField] private List<SurfaceFootstepData> surfaceFootsteps = new List<SurfaceFootstepData>();

    [HideInInspector] public float currentSpeed;
    [HideInInspector] public bool isWalking;
    [HideInInspector] public bool isJumping;
    [HideInInspector] public bool isSprinting;
    [HideInInspector] public bool isGrounded;
    [HideInInspector] public Vector2 moveInput;

    public float MoveSpeed => moveSpeed;
    public float SprintSpeed => sprintSpeed;

    private Rigidbody rb;
    private PlayerInput playerInput;
    private CapsuleCollider capsule;
    private Vector2 lookInput;
    private float xRotation;
    private float lastJumpTime;
    private float preservedAirSpeed;
    private float lastFootstepTime;
    private string currentGroundTag = "Untagged";
    private RaycastHit lastGroundHit;
    private bool isCrouching;
    private Vector3 standingCenter;
    private float originalCameraOffset;

    [System.Serializable]
    public class SurfaceFootstepData
    {
        public string surfaceTag = "Default";
        public AudioClip[] footstepClips;
        public float volumeMultiplier = 1f;
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        playerInput = GetComponent<PlayerInput>();
        capsule = GetComponent<CapsuleCollider>();

        if (playerCamera == null)
            playerCamera = Camera.main;

        rb.freezeRotation = true;

        if (footstepAudioSource == null)
            footstepAudioSource = GetComponent<AudioSource>();
    }

    private void OnEnable()
    {
        if (moveAction != null)
        {
            moveAction.action.Enable();
            moveAction.action.performed += OnMove;
            moveAction.action.canceled += OnMove;
        }

        if (lookAction != null)
        {
            lookAction.action.Enable();
            lookAction.action.performed += OnLook;
            lookAction.action.canceled += OnLook;
        }

        if (jumpAction != null)
        {
            jumpAction.action.Enable();
            jumpAction.action.performed += OnJump;
        }

        if (sprintAction != null)
            sprintAction.action.Enable();
        if (crouchAction != null)
            crouchAction.action.Enable();
    }

    private void OnDisable()
    {
        if (moveAction != null)
        {
            moveAction.action.performed -= OnMove;
            moveAction.action.canceled -= OnMove;
        }

        if (lookAction != null)
        {
            lookAction.action.performed -= OnLook;
            lookAction.action.canceled -= OnLook;
        }

        if (jumpAction != null)
            jumpAction.action.performed -= OnJump;
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        currentSpeed = moveSpeed;


        standingCenter = capsule.center;
        standingHeight = capsule.height;
        originalCameraOffset = cameraOffsetY;
    }
    private void Update()
    {
        GroundCheck();
        HandleCrouch();      
        UpdateStateFlags();
        CameraLogic();       
        UpdateAudioSourcePosition();
        UpdateFootsteps();
    }

    private void FixedUpdate()
    {
        Movement();
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
        if (!isGrounded || isJumping) return;
        if (Time.time < lastJumpTime + jumpCooldown) return;

        lastJumpTime = Time.time;

        isGrounded = false;
        isJumping = true;

        preservedAirSpeed = currentSpeed;

        Vector3 velocity = rb.linearVelocity;
        velocity.y = 0f;
        rb.linearVelocity = velocity;

        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    private void UpdateStateFlags()
    {
        isWalking = moveInput.sqrMagnitude > 0.01f && isGrounded;

        bool sprintPressed = sprintAction != null && sprintAction.action.IsPressed();

        if (isGrounded)
        {
            if (isCrouching)
            {
                isSprinting = false;
                currentSpeed = crouchSpeed;
            }
            else
            {
                isSprinting = sprintPressed && moveInput.sqrMagnitude > 0.01f;
                currentSpeed = isSprinting ? sprintSpeed : moveSpeed;
            }
        }

        if (isGrounded && rb.linearVelocity.y <= 0.05f)
            isJumping = false;
    }
    private void Movement()
    {
        if (playerCamera == null) return;

        Vector3 forward = playerCamera.transform.forward;
        Vector3 right = playerCamera.transform.right;

        forward.y = 0f;
        right.y = 0f;

        forward.Normalize();
        right.Normalize();

        Vector3 moveDirection = (right * moveInput.x + forward * moveInput.y).normalized;

        float speedToUse = isGrounded ? currentSpeed : preservedAirSpeed;

        Vector3 velocity = rb.linearVelocity;

        velocity.x = moveDirection.x * speedToUse;
        velocity.z = moveDirection.z * speedToUse;

        rb.linearVelocity = velocity;
    }
    private void CameraLogic()
    {
        if (playerCamera == null) return;

        float targetCameraY = capsule.center.y + (capsule.height * 0.5f) - cameraOffsetY;

        playerCamera.transform.localPosition = Vector3.Lerp(
            playerCamera.transform.localPosition,
            new Vector3(capsule.center.x, targetCameraY, capsule.center.z),
            Time.deltaTime * 10f
        );

        bool mouseScheme = playerInput.currentControlScheme != null &&
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
            x = lookInput.x * gamepadSensitivity * Time.deltaTime;
            y = lookInput.y * gamepadSensitivity * Time.deltaTime;
        }

        transform.Rotate(Vector3.up * x);

        xRotation -= y;
        xRotation = Mathf.Clamp(xRotation, -xClamp, xClamp);

        playerCamera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }
    private void GroundCheck()
    {
        Vector3 center = transform.TransformPoint(capsule.center);

        float radius = capsule.radius * Mathf.Max(transform.lossyScale.x, transform.lossyScale.z);
        float height = Mathf.Max(capsule.height * transform.lossyScale.y, radius * 2f);

        Vector3 point1 = center + Vector3.up * (height / 2f - radius);
        Vector3 point2 = center - Vector3.up * (height / 2f - radius);

        isGrounded = Physics.CapsuleCast(
            point1,
            point2,
            radius * 0.95f,
            Vector3.down,
            out lastGroundHit,
            groundCheckDistance,
            groundLayer,
            QueryTriggerInteraction.Ignore
        );

        if (isGrounded && lastGroundHit.collider != null)
        {
            currentGroundTag = lastGroundHit.collider.tag;
        }
    }

    private void UpdateFootsteps()
    {
        if (!isWalking || !isGrounded) return;
        if (Time.time < lastFootstepTime + footstepCooldown) return;

        PlayFootstep();
        lastFootstepTime = Time.time;
    }

    private void PlayFootstep()
    {
        if (footstepAudioSource == null) return;

        SurfaceFootstepData surfaceData = GetSurfaceFootstepData(currentGroundTag);

        if (surfaceData == null || surfaceData.footstepClips == null || surfaceData.footstepClips.Length == 0)
            return;

        AudioClip clip = surfaceData.footstepClips[Random.Range(0, surfaceData.footstepClips.Length)];

        if (clip != null)
        {
            footstepAudioSource.PlayOneShot(clip, surfaceData.volumeMultiplier);
        }
    }

    private SurfaceFootstepData GetSurfaceFootstepData(string tag)
    {
        foreach (SurfaceFootstepData data in surfaceFootsteps)
        {
            if (data.surfaceTag == tag)
                return data;
        }

        return null;
    }

    private void HandleCrouch()
    {
        if (crouchAction == null) return;

        bool crouchPressed = crouchAction.action.IsPressed();

        if (crouchPressed)
        {
            if (!isCrouching)
            {
                isCrouching = true;
                float bottomY = capsule.bounds.min.y;

                capsule.height = crouchHeight;

                float newCenterY = (bottomY - transform.position.y) + (crouchHeight / 2f);

                capsule.center = new Vector3(
                    standingCenter.x,
                    newCenterY,
                    standingCenter.z
                );

                currentSpeed = crouchSpeed;
            }
        }
        else
        {
            if (isCrouching)
            {
                if (CanStandUp())
                {
                    isCrouching = false;

                    float bottomY = capsule.bounds.min.y;

                    capsule.height = standingHeight;

                    float newCenterY = (bottomY - transform.position.y) + (standingHeight / 2f);

                    capsule.center = new Vector3(
                        standingCenter.x,
                        newCenterY,
                        standingCenter.z
                    );
                }
            }
        }
    }
    private bool CanStandUp()
    {
        float bottomY = capsule.bounds.min.y;

        float radius = capsule.radius * 0.95f;
        float halfHeight = standingHeight / 2f;

        Vector3 center = new Vector3(
            transform.position.x,
            bottomY + halfHeight,
            transform.position.z
        );

        Vector3 point1 = center + Vector3.up * (halfHeight - radius);
        Vector3 point2 = center - Vector3.up * (halfHeight - radius);

        Collider[] hits = Physics.OverlapCapsule(
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
    private void UpdateAudioSourcePosition()
    {
        if (footstepAudioSource == null) return;

        Bounds bounds = capsule.bounds;

        Vector3 bottomPosition = new Vector3(
            bounds.center.x,
            bounds.min.y,
            bounds.center.z
        );

        footstepAudioSource.transform.position = bottomPosition;
    }
}