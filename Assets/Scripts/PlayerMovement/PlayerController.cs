using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float sprintSpeed = 8f;
    [SerializeField] private float jumpForce = 5f;

    [SerializeField] private float groundCheckDistance = 0.1f;
    [SerializeField] private LayerMask groundLayer = ~0;

    [Header("Camera")]
    [SerializeField] private float MouseSensitivity = 100f;
    [SerializeField] private float GamepadSensitivity = 100f;
    [SerializeField] private Camera PlayerCamera;
    [SerializeField] private GameObject mobileControlsUI;

    private Rigidbody rb;
    private PlayerInput playerInput;
    private CapsuleCollider capsule;

    private InputAction sprintAction;

    private Vector2 moveInput;
    private Vector2 lookInput;

    private float currentSpeed;
    private bool isSprinting;
    private bool isGrounded;
    float xClamp = 85f;
    float xRotation;



    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        playerInput = GetComponent<PlayerInput>();
        capsule = GetComponent<CapsuleCollider>();
        if (PlayerCamera == null) PlayerCamera = Camera.main;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        sprintAction = playerInput.actions["Sprint"];

        currentSpeed = moveSpeed;
        isSprinting = false;
        isGrounded = false;


        SwitchMobileUI(Application.isMobilePlatform);
    }

    private void Update()
    {
        CameraLogic();
    }

    private void FixedUpdate()
    {
        if (playerInput != null && playerInput.actions != null)
        {
            if (sprintAction != null)
            {
                isSprinting = sprintAction.IsPressed();
            }
        }

        GroundCheck();

        currentSpeed = isSprinting ? sprintSpeed : moveSpeed;
        Movement();
    }

    public void OnMove(InputValue value) => moveInput = value.Get<Vector2>();
    public void OnCamera(InputValue value) => lookInput = value.Get<Vector2>();
    public void OnJump(InputValue value)
    {
        if (value.isPressed && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isGrounded = false;
        }
    }

    private void Movement()
    {
        Vector3 forward = PlayerCamera.transform.forward;
        Vector3 right = PlayerCamera.transform.right;
        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();

        Vector3 worldMove = (right * moveInput.x + forward * moveInput.y) * currentSpeed * Time.fixedDeltaTime;

        rb.MovePosition(rb.position + worldMove);
    }

    private void CameraLogic()
    {
        string scheme = playerInput.currentControlScheme;

        float x, y;

        if (scheme == "Keyboard&Mouse")
        {
            x = lookInput.x * MouseSensitivity;
            y = lookInput.y * MouseSensitivity;
        }
        else
        {
            x = lookInput.x * GamepadSensitivity * Time.deltaTime;
            y = lookInput.y * GamepadSensitivity * Time.deltaTime;
        }

        transform.Rotate(Vector3.up, x);

        xRotation -= y;
        xRotation = Mathf.Clamp(xRotation, -xClamp, xClamp);

        PlayerCamera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }

    private void GroundCheck()
    {
        if (capsule == null)
        {
            isGrounded = false;
            return;
        }

        Vector3 worldCenter = transform.TransformPoint(capsule.center);

        float radius = capsule.radius * Mathf.Max(transform.lossyScale.x, transform.lossyScale.z);
        float halfHeight = Mathf.Max(0f, capsule.height * 0.5f - capsule.radius);
        Vector3 up = transform.up;

        Vector3 capTop = worldCenter + up * halfHeight;
        Vector3 capBottom = worldCenter - up * halfHeight;

        Vector3 bottomSurface = capBottom - up * radius;

        const float startOffset = 0.01f;
        Vector3 rayOrigin = bottomSurface + up * startOffset;
        float rayDistance = groundCheckDistance + startOffset;

        RaycastHit hit;
        isGrounded = Physics.Raycast(rayOrigin, -up, out hit, rayDistance, groundLayer, QueryTriggerInteraction.Ignore);

        Color debugColor = isGrounded ? Color.green : Color.red;
        Debug.DrawLine(capTop, capBottom, debugColor);
        Debug.DrawRay(rayOrigin, -up * rayDistance, debugColor);
    }

    void SwitchMobileUI(bool mobile)
    {
        mobileControlsUI.SetActive(mobile);
    }
}