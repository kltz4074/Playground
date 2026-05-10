using UnityEngine;

public enum PlayerState
{
    Idle,
    Walking,
    Running,
    Jumping
}

public class PlayerStateMachine : MonoBehaviour
{
    [Header("References")]
    public PlayerController playerController;
    private Animator animator;

    [Header("Current State")]
    public PlayerState currentState;

    [Header("Debug")]
    [SerializeField] private float currentSpeed;

    private PlayerState previousState;

    private void Start()
    {
        animator = GetComponent<Animator>();

        if (playerController == null)
            playerController = GetComponent<PlayerController>();

        previousState = currentState;
    }

    private void Update()
    {
        UpdateState();
        HandleStateChange();
        UpdateAnimator();
    }

    private void UpdateState()
    {
        currentSpeed = playerController.currentSpeed;

        if (playerController.isJumping)
        {
            currentState = PlayerState.Jumping;
        }
        else if (playerController.isSprinting)
        {
            currentState = PlayerState.Running;
        }
        else if (playerController.isWalking)
        {
            currentState = PlayerState.Walking;
        }
        else
        {
            currentState = PlayerState.Idle;
        }
    }

    private void HandleStateChange()
    {
        if (previousState != currentState)
        {
            ExitState(previousState);
            EnterState(currentState);

            previousState = currentState;
        }
    }

    private void EnterState(PlayerState newState)
    {
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
        }
    }

    private void ExitState(PlayerState oldState)
    {
        switch (oldState)
        {
            case PlayerState.Idle:
                break;

            case PlayerState.Walking:
                break;

            case PlayerState.Running:
                break;

            case PlayerState.Jumping:
                break;
        }
    }

    private void UpdateAnimator()
    {
        if (animator == null) return;

        float targetVelocity = 0f;

        if (currentState == PlayerState.Running)
        {
            targetVelocity = 1f;
        }
        else if (currentState == PlayerState.Walking)
        {
            targetVelocity = playerController.moveSpeed / playerController.sprintSpeed;
        }
        else if (currentState == PlayerState.Jumping)
        {
            targetVelocity = playerController.moveSpeed / playerController.sprintSpeed;
        }
        else 
        {
            targetVelocity = 0f;
        }

        if (Mathf.Abs(targetVelocity) < 0.01f)
        {
            targetVelocity = 0f;
        }

        animator.SetFloat("Velocity", targetVelocity);
    }
}