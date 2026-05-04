using System.Collections;
using System.Threading;
using UnityEngine;
using UnityEngine.Rendering;

public class FrameRateManager : MonoBehaviour
{
    public int MaxRate = 9999;
    public float TargetFrameRate = 120f;
    float currentFrameTime;

    private void Awake()
    {
        if (Application.isMobilePlatform) { 
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = MaxRate;
            currentFrameTime = Time.realtimeSinceStartup;
            StartCoroutine(WaitForNextFrame());
        }
    }
    IEnumerator WaitForNextFrame()
    {
        while (true)
        {
            yield return new WaitForEndOfFrame();
            currentFrameTime += 1.0f / TargetFrameRate * 2;
            var t = Time.realtimeSinceStartup;
            var sleepTime = currentFrameTime - t - 0.001f;
            if (sleepTime > 0) Thread.Sleep((int)(sleepTime * 1000));
            while (sleepTime > 0)
            {
                t = Time.realtimeSinceStartup;
            }
        }
    }
}
