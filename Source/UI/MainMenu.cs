using RepoXR.Input;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.InputSystem.XR;
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

    private void SetupMainCamera()
    {
        // Camera rendering setup
        mainCamera = CameraUtils.Instance.MainCamera;
        mainCamera.targetTexture = null;
        // mainCamera.cullingMask |= 1 << 5; // TODO: Add stacked UI camera

        var uiCamera = CameraOverlay.instance.overlayCamera;
        uiCamera.CopyFrom(mainCamera);
        uiCamera.cullingMask = 1 << 5; // UI Only
        uiCamera.transform.parent = mainCamera.transform;
        uiCamera.transform.localPosition = Vector3.zero;
        uiCamera.transform.localEulerAngles = Vector3.zero;
        uiCamera.clearFlags = CameraClearFlags.Depth;
        uiCamera.farClipPlane = 150;
        uiCamera.depth = 1;
        
        // Camera tracking
        var poseDriver = mainCamera.gameObject.AddComponent<TrackedPoseDriver>();
        poseDriver.positionAction = Actions.Instance.HeadPosition;
        poseDriver.rotationAction = Actions.Instance.HeadRotation;
        poseDriver.trackingStateInput = new InputActionProperty(Actions.Instance.HeadTrackingState);
    }

    private void SetupMainCanvas()
    {
        mainCanvas = HUDCanvas.instance.GetComponent<Canvas>();
        mainCanvas.renderMode = RenderMode.WorldSpace;
        mainCanvas.transform.position = new Vector3(-45, -0.75f, 6);
        mainCanvas.transform.eulerAngles = new Vector3(0, 45, 0);
        mainCanvas.transform.localScale = Vector3.one * 0.03f;
        
        Destroy(mainCanvas.GetComponent<GraphicRaycaster>());
        mainCanvas.gameObject.AddComponent<TrackedDeviceGraphicRaycaster>();
        
        // Remove blocking UI elements
        mainCanvas.transform.Find("Fade").gameObject.SetActive(false);
        mainCanvas.transform.Find("Render Texture Video").gameObject.SetActive(false);
        
        // Remove PC UI Canvas
        RenderTextureMain.instance.transform.GetComponentInParent<Canvas>().enabled = false;
        
        // Remove game HUD elements
        mainCanvas.transform.Find("HUD/Game Hud").gameObject.SetActive(false);
        mainCanvas.transform.Find("HUD/Chat").gameObject.SetActive(false);
        mainCanvas.transform.Find("HUD/Chat Local").gameObject.SetActive(false);
    }

    private void SetupControllers()
    {
        mainCamera.transform.parent.gameObject.AddComponent<XRRayInteractorManager>();
    }
    
    private static void DisableEventSystem()
    {
        var input = GameObject.Find("EventSystem")?.GetComponent<InputSystemUIInputModule>();
        if (input != null)
            input.enabled = false;
    }
}