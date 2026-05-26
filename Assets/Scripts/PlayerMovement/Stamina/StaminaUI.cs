using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class StaminaUI : MonoBehaviour
{
    [SerializeField] private StaminaManager staminaManager;
    [SerializeField] private Slider staminaSlider;

    [SerializeField] private Image sliderColor;
    [Header("UI")]
    [SerializeField] private Image SliderBackround;
    [SerializeField] private Image Slider;

    [SerializeField] public Color SliderColor = Color.green;
    [SerializeField] public Color SliderBackroundColor = Color.gray;

    [SerializeField] public float HidingCloudoun;

    [SerializeField] public float DefaultVisibility = 0.8f;
    [SerializeField] public float SliderVisibilityAnimationSpeed = 5f;

    private Color defaultSliderColor;

    private Coroutine currentBlink;
    private float backround_visiblity => Slider.color.a;
    private float slider_visibility => SliderBackround.color.a;
    private float total_visiblity;

    public void Start()
    {   
        staminaSlider.minValue = staminaManager.minStamina;
        staminaSlider.maxValue = staminaManager.maxStamina;
        staminaSlider.value = staminaManager.maxStamina;
        defaultSliderColor = sliderColor.color;
    }

    public void Update()
    {
        staminaSlider.minValue = staminaManager.minStamina;
        staminaSlider.maxValue = staminaManager.maxStamina;

        staminaSlider.value = Mathf.Lerp(
            staminaSlider.value,
            staminaManager.CurrentStamina,
            Time.deltaTime * 5f
        );
    }


    public void BlinkColor(Color color, float duration)
    {

        if (currentBlink != null)
            StopCoroutine(currentBlink);

        currentBlink = StartCoroutine(Blink(duration, color));
    }
    private IEnumerator Blink(float duration, Color blinkColor)
    {
        float halfDuration = duration * 0.5f;

        float timer = 0f;

        while (timer < halfDuration)
        {
            timer += Time.deltaTime;

            float t = timer / halfDuration;

            sliderColor.color = Color.Lerp(defaultSliderColor, blinkColor, t);

            yield return null;
        }

        timer = 0f;

        while (timer < halfDuration)
        {
            timer += Time.deltaTime;

            float t = timer / halfDuration;

            sliderColor.color = Color.Lerp(blinkColor, defaultSliderColor, t);

            yield return null;
        }

        sliderColor.color = defaultSliderColor;
    }


}