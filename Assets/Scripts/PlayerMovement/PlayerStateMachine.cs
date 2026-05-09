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
    private float animatorVelocity = 0f;

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

        // Приоритет 1: Прыжок имеет наивысший приоритет
        if (playerController.isJumping)
        {
            currentState = PlayerState.Jumping;
        }
        // Приоритет 2: Спринт/Бег
        else if (playerController.isSprinting)
        {
            currentState = PlayerState.Running;
        }
        // Приоритет 3: Ходьба
        else if (playerController.isWalking)
        {
            currentState = PlayerState.Walking;
        }
        // Приоритет 4: Стояние на месте (по умолчанию)
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

        // Определяем целевую скорость на основе состояния
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
        else // Idle
        {
            targetVelocity = 0f;
        }

        // Очищаем микроскопические значения
        if (Mathf.Abs(targetVelocity) < 0.01f)
        {
            targetVelocity = 0f;
        }

        animator.SetFloat("Velocity", targetVelocity);
    }
}