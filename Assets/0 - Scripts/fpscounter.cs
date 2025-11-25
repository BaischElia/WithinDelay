using System.Runtime.CompilerServices;
using System.Text;
using TMPro;
using UnityEngine;
using System.Runtime.InteropServices;


public class fpscounter : MonoBehaviour
{
    private bool isActive = false;
    private float totalTime = 0f;
    private int totalFrames = 0;
    private float preCountTime = 3f; // 3 seconds before the test to ensure the game is running smoothly
    private float preCounter = 0f;
    private float screenWidth;
    private float screenHeight;
    private double screenRefreshRate;
    private string GPUName;
    private string CPUName;
    private string OSName;

    [DllImport("__Internal")]
    private static extern void receiveHardwareData(string id, string data);
    void Update()
    {
        if (isActive)
        {
            if (preCounter < preCountTime)
            {
                preCounter += Time.deltaTime;
                return; // Wait for the pre-count time before starting the FPS test
            }

            else if (totalTime <= 10f)
            {
                totalTime += Time.deltaTime;
                totalFrames++;
            }
            else
            {
                GetHardwareData();
            }
        }
    }

    private void GetHardwareData()
    {
        isActive = false;
        int averageFPS = Mathf.RoundToInt(totalFrames / totalTime);

        string id = GetComponent<datamanager>().GetID();

        string hardwareData = $"{id},{screenWidth},{screenHeight},{screenRefreshRate},{GPUName},{CPUName},{OSName},{averageFPS};";

// Javascript function call to send data
#if UNITY_WEBGL == true && !UNITY_EDITOR
        receiveHardwareData(id, hardwareData);
#endif

        this.enabled = false; // Disable this script after sending data
    }

    public void StartFPSTest()
    {
        isActive = true;
    }

    // gets called after TutorialManager.CheckPassword()
    public void SaveHardwareData()
    {
        Resolution currentResolution = Screen.currentResolution;
        screenWidth = currentResolution.width;
        screenHeight = currentResolution.height;
        screenRefreshRate = currentResolution.refreshRateRatio.value;
        GPUName = SystemInfo.graphicsDeviceName;
        CPUName = SystemInfo.processorType;
        OSName = SystemInfo.operatingSystem;
    }

}
