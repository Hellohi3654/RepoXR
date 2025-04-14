using System.Collections;
using RepoXR.Input;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
using UnityEngine.Rendering.PostProcessing;

namespace RepoXR.UI;

public class ReloadMenu : MonoBehaviour
{
    private IEnumerator Start()
    {
        yield return null;
        
        var camera = Camera.main!;
        var overlayCamera = CameraOverlay.instance.overlayCamera;
        var hud = HUDCanvas.instance.GetComponent<Canvas>();
        
        hud.renderMode = RenderMode.WorldSpace;
        hud.transform.position = new Vector3(-3.55f, -1, 2.5f);
        hud.transform.eulerAngles = Vector3.zero;
        hud.transform.localScale = Vector3.one * 0.01f;
        
        overlayCamera.CopyFrom(camera);
        overlayCamera.targetTexture = null;
        overlayCamera.depth = 1;
        overlayCamera.transform.parent = transform;

        camera.enabled = false;
        
        var poseDriver = overlayCamera.gameObject.AddComponent<TrackedPoseDriver>();
        poseDriver.positionAction = Actions.Instance.HeadPosition;
        poseDriver.rotationAction = Actions.Instance.HeadRotation;
        poseDriver.trackingStateInput = new InputActionProperty(Actions.Instance.HeadTrackingState);
        poseDriver.PerformUpdate();
        
        // Overlay Canvas
        var overlayCanvas = new GameObject("Overlay Canvas").AddComponent<Canvas>();
        overlayCanvas.renderMode = RenderMode.WorldSpace;
        overlayCanvas.transform.parent = overlayCamera.transform;
        overlayCanvas.transform.localPosition = Vector3.forward * 0.3f;
        overlayCanvas.transform.localEulerAngles = Vector3.zero;
        overlayCanvas.transform.localScale = Vector3.one * 0.002f;
        overlayCanvas.GetComponent<RectTransform>().sizeDelta = new Vector2(640, 360);

        VideoOverlay.Instance.transform.parent = overlayCanvas.transform;
        VideoOverlay.Instance.transform.localPosition = Vector3.zero;
        VideoOverlay.Instance.transform.localEulerAngles = Vector3.zero;
        VideoOverlay.Instance.transform.localScale = Vector3.one;
        
        yield return null;
        
        // Remove PC UI Canvas
        RenderTextureMain.instance.transform.GetComponentInParent<Canvas>().enabled = false;

        transform.localEulerAngles = new Vector3(0, -overlayCamera.transform.localEulerAngles.y, 0);
        transform.localPosition += Actions.Instance.HeadPosition.ReadValue<Vector3>();
    }
}