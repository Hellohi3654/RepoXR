using UnityEngine;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.UI;

namespace RepoXR.UI;

public class MainMenu : MonoBehaviour
{
    private Camera mainCamera;
    private Canvas mainCanvas;
    
    private void Start()
    {
        DisableEventSystem();
        SetupMainCamera();
        SetupMainCanvas();
        SetupControllers();
    }
    
    private static void DisableEventSystem()
    {
        var input = GameObject.Find("EventSystem")?.GetComponent<InputSystemUIInputModule>();
        if (input != null)
            input.enabled = false;
    }
    
    private void SetupMainCamera()
    {
        // Camera rendering setup
        mainCamera = CameraUtils.Instance.MainCamera;

        var topCamera = mainCamera.transform.Find("Camera Top").GetComponent<Camera>();
        topCamera.depth = 1;
        topCamera.targetTexture = null;
    }

    private void SetupMainCanvas()
    {
        mainCanvas = HUDCanvas.instance.GetComponent<Canvas>();
        mainCanvas.renderMode = RenderMode.WorldSpace;
        mainCanvas.transform.position = new Vector3(-45, -0.75f, 6);
        mainCanvas.transform.eulerAngles = new Vector3(0, 45, 0);
        mainCanvas.transform.localScale = Vector3.one * 0.03f;
        mainCanvas.gameObject.AddComponent<Mask>();
        mainCanvas.gameObject.AddComponent<Image>().color = new Color(0, 0, 0, 0);
        
        Destroy(mainCanvas.GetComponent<GraphicRaycaster>());
        mainCanvas.gameObject.AddComponent<TrackedDeviceGraphicRaycaster>();
        
        // Remove game HUD elements
        mainCanvas.transform.Find("HUD/Game Hud").gameObject.SetActive(false);
        mainCanvas.transform.Find("HUD/Chat").gameObject.SetActive(false);
        mainCanvas.transform.Find("HUD/Chat Local").gameObject.SetActive(false);
    }

    private void SetupControllers()
    {
        mainCamera.transform.parent.gameObject.AddComponent<XRRayInteractorManager>();
    }
}