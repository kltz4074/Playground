using UnityEditor.TextCore.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Windows.WebCam;


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
