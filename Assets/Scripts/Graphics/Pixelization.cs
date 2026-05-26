using UnityEngine;

public class Pixelization : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RenderTexture renderTexture;

    [Header("Graphics Settings")]
    [Range(1, 50)]
    [SerializeField] private int pixelizationFactor = 1;

    [SerializeField] private bool pixelizationEnabled = true;

    [SerializeField] private FilterMode filterMode = FilterMode.Point;


    private int referenceHeight = 1080;

    private int currentScreenWidth;
    private int currentScreenHeight;

    private int renderWidth;
    private int renderHeight;

    private void Start()
    {
        currentScreenWidth = Screen.width;
        currentScreenHeight = Screen.height;

        ApplyPixelization();
    }

    private void Update()
    {
        if (currentScreenWidth != Screen.width ||
            currentScreenHeight != Screen.height)
        {
            currentScreenWidth = Screen.width;
            currentScreenHeight = Screen.height;

            ApplyPixelization();
        }
    }

    private void OnValidate()
    {
        pixelizationFactor = Mathf.Clamp(pixelizationFactor, 1, 50);

        ApplyPixelization();
    }

    private void ApplyPixelization()
    {
        if (renderTexture == null)
            return;

        if (pixelizationEnabled)
        {
            float aspect = (float)Screen.width / Screen.height;

            renderHeight = Mathf.Max(1, referenceHeight / pixelizationFactor);
            renderWidth = Mathf.Max(1, Mathf.RoundToInt(renderHeight * aspect));
        }
        else
        {
            renderWidth = Screen.width;
            renderHeight = Screen.height;
        }

        RenderGraphics();
    }

    private void RenderGraphics()
    {
        if (renderTexture.width == renderWidth &&
            renderTexture.height == renderHeight &&
            renderTexture.filterMode == filterMode)
        {
            return;
        }

        if (renderTexture.IsCreated())
        {
            renderTexture.Release();
        }

        renderTexture.width = renderWidth;
        renderTexture.height = renderHeight;

        renderTexture.filterMode = filterMode;
        renderTexture.wrapMode = TextureWrapMode.Clamp;

        renderTexture.Create();
    }

    public void SetPixelization(bool enabled)
    {
        pixelizationEnabled = enabled;
        ApplyPixelization();
    }

    public void SetPixelizationFactor(int factor)
    {
        pixelizationFactor = Mathf.Clamp(factor, 1, 50);
        ApplyPixelization();
    }

    public void SetFilterMode(FilterMode mode)
    {
        filterMode = mode;
        RenderGraphics();
    }
}