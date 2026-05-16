using UnityEngine;

public enum PlayerState
{
    Idle,
    Walking,
    Running,
    Jumping,
    Falling
}

[RequireComponent(typeof(PlayerController))]
public class PlayerStateMachine : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerController playerController;
    [SerializeField] private Animator animator;

    [Header("Current State")]
    [SerializeField] private PlayerState currentState;

    [Header("Settings")]
    [SerializeField] private float animatorSmoothTime = 8f;
    [SerializeField] private bool enableDebugLogs;

    [Header("Debug")]
    [SerializeField] private float currentVelocityValue;

    private PlayerState previousState;
    private int velocityHash;
    private int isGroundedHash;
    private int isJumpingHash;
    private int isFallingHash;

    public PlayerState CurrentState => currentState;

    private void Awake()
    {
        if (playerController == null)
            playerController = GetComponent<PlayerController>();

        if (animator == null)
            animator = GetComponent<Animator>();

        velocityHash = Animator.StringToHash("Velocity");
        isGroundedHash = Animator.StringToHash("IsGrounded");
        isJumpingHash = Animator.StringToHash("IsJumping");
        isFallingHash = Animator.StringToHash("IsFalling");
    }

    private void Start()
    {
        currentState = PlayerState.Idle;
        previousState = currentState;
    }

    private void Update()
    {
        if (playerController == null) return;

        UpdateState();
        HandleStateChange();
        UpdateAnimator();
    }

    private void UpdateState()
    {
        float verticalVelocity = playerController.GetComponent<Rigidbody>().linearVelocity.y;

        if (!playerController.isGrounded && verticalVelocity < -0.1f)
        {
            currentState = PlayerState.Falling;
            return;
        }

        if (playerController.isJumping)
        {
            currentState = PlayerState.Jumping;
            return;
        }

        if (playerController.isSprinting)
        {
            currentState = PlayerState.Running;
            return;
        }

        if (playerController.isWalking)
        {
            currentState = PlayerState.Walking;
            return;
        }

        currentState = PlayerState.Idle;
    }

    private void HandleStateChange()
    {
        if (previousState == currentState) return;

        ExitState(previousState);
        EnterState(currentState);

        previousState = currentState;
    }

    private void EnterState(PlayerState newState)
    {
        if (!enableDebugLogs) return;

        switch (newState)
        {
            case PlayerState.Idle:
                Debug.Log("Entered Idle");
                break;

            case PlayerState.Walking:
                Debug.Log("Entered Walking");
                break;

            case PlayerState.Running:
                Debug.Log("Entered Running");
                break;

            case PlayerState.Jumping:
                Debug.Log("Entered Jumping");
                break;

            case PlayerState.Falling:
                Debug.Log("Entered Falling");
                break;
        }
    }

    private void ExitState(PlayerState oldState)
    {
    }

    private void UpdateAnimator()
    {
        if (animator == null) return;

        float targetVelocity = GetTargetVelocity();

        if (targetVelocity == 0f)
        {
            currentVelocityValue = 0f;
        }
        else
        {
            currentVelocityValue = Mathf.Lerp(
                currentVelocityValue,
                targetVelocity,
                animatorSmoothTime * Time.deltaTime
            );
        }

        if (Mathf.Abs(currentVelocityValue) < 0.01f)
            currentVelocityValue = 0f;

        animator.SetFloat(velocityHash, currentVelocityValue);
        animator.SetBool(isGroundedHash, playerController.isGrounded);
        animator.SetBool(isJumpingHash, currentState == PlayerState.Jumping);
        animator.SetBool(isFallingHash, currentState == PlayerState.Falling);
    }

    private float GetTargetVelocity()
    {
        switch (currentState)
        {
            case PlayerState.Running:
                return 1f;

            case PlayerState.Walking:
                return Mathf.Clamp01(
                    playerController.MoveSpeed / playerController.SprintSpeed
                );

            case PlayerState.Jumping:
                return Mathf.Clamp01(
                    playerController.moveInput.magnitude
                );

            case PlayerState.Falling:
                return Mathf.Clamp01(
                    playerController.moveInput.magnitude
                );

            default:
                return 0f;
        }
    }


}