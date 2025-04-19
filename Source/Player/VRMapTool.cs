using RepoXR.Managers;
using UnityEngine;

namespace RepoXR.Player;

public class VRMapTool : MonoBehaviour
{
    private MapToolController controller;

    private RenderTexture displayTexture;
    private Light light;   
    
    private void Awake()
    {
        controller = GetComponent<MapToolController>();
        
        var container = controller.HideTransform.Find("Main Spring/Base Offset/Bob/Main Unit/Display Spring");
        var display = container.Find("display_1x1");
        
        displayTexture = (RenderTexture)container.Find("display_1x1").GetComponent<MeshRenderer>().material.mainTexture;
        light = container.Find("Light").GetComponent<Light>();

        // FREE FIX SINCE THIS IS AN ISSUE IN THE BASE GAME AS WELL
        display.transform.localPosition = Vector3.back * 0.006f;
    }

    private void Update()
    {
        if (controller.Active)
        {
            light.intensity = Mathf.Lerp(light.intensity, 1, 4 * Time.deltaTime);
            
            VRSession.Instance.Player.DisableGrabRotate(0.1f);
        }
        else
        {
            displayTexture.Release();
            light.intensity = Mathf.Lerp(light.intensity, 0, 4 * Time.deltaTime);
        }
    }
}