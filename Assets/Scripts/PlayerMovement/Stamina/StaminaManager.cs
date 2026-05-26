using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class StaminaManager : MonoBehaviour
{
    [Header("SETTINGS")]
    [SerializeField] public float RunningStaminaCost = 5f;
    [SerializeField] public float JumpStaminaCost;
    [Space]
    [SerializeField] public float maxStamina = 100f;
    [SerializeField] public float minStamina = 0f;
    [Space]
    [SerializeField] public float StaminaRegenerationAmount = 10f;
    [SerializeField] public float CrouchingStaminaRegenerationAmount = 15f;
    [SerializeField] public float StaminaRegenerationDelay = 2f;

    [SerializeField] public StaminaUI staminaUI;

    [Header("UI")]
    [SerializeField] private Image staminaSlider;
    [SerializeField] private Image staminaSliderBackround;
    
    [SerializeField] private Color staminaColor = Color.green;
    [SerializeField] private Color staminaBacrkoundColor = Color.gray;

    [SerializeField] private float HidingCountdown = 1f;

    [SerializeField] public float DefaultVisibility = 0.8f;
    [SerializeField] private float maxVisibility = 1f;
    [SerializeField] public float SliderVisibilityAnimationSpeed = 5f;

    [HideInInspector] public float CurrentStamina = 100;

    private float lastStaminaChangeTime;

    public void Start()
    {
        CurrentStamina = 100;
    }

    public void Update()
    {
        HandleBarVisiblity();
    }

    public void SetStamina(float amount)
    {
        if (CurrentStamina != amount)
        {
            CurrentStamina = amount;
            lastStaminaChangeTime = Time.time;
        }
    }
    public bool CanUseStamina(float amount)
    {
        bool canUse;

        if (CurrentStamina >= amount)
            canUse = true;
        else
            canUse = false;

        return canUse;
    }

    private void HandleBarVisiblity()
    {
        bool visible =
            Time.time - lastStaminaChangeTime < HidingCountdown;

        float targetAlpha = visible ? maxVisibility : 0f;

        Color sliderColor = staminaSlider.color;
        sliderColor.a = Mathf.Lerp(
            sliderColor.a,
            targetAlpha,
            Time.deltaTime * SliderVisibilityAnimationSpeed
        );
        staminaSlider.color = sliderColor;

        Color backgroundColor = staminaSliderBackround.color;
        backgroundColor.a = Mathf.Lerp(
            backgroundColor.a,
            targetAlpha,
            Time.deltaTime * SliderVisibilityAnimationSpeed
        );
        staminaSliderBackround.color = backgroundColor;
    }
}