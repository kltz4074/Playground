 using UnityEngine;


public class CameraRecording : MonoBehaviour
{
    private WebCamTexture webcamTexture;
    public Material targetMaterial;

    private void Start()
    {
        webcamTexture = new WebCamTexture();
        targetMaterial.mainTexture = webcamTexture;
        webcamTexture.Play();
    }

    void OnDisable()
    {
        webcamTexture.Stop();
    }
}
