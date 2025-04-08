using System;
using System.Collections;
using RepoXR.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace RepoXR.UI;

public class GameHud : MonoBehaviour
{
    private void Awake()
    {
        Logger.LogWarning("GameHud::Awake");
        
        // Disable main render texture which obscures the flatscreen mirror view
        RenderTextureMain.instance.transform.GetComponentInParent<Canvas>().enabled = false;
        
        // Set up UI camera
        var camera = CameraOverlay.instance.overlayCamera;
        
        Logger.LogDebug(camera);
    }

    private IEnumerator Start()
    {
        // Make sure all other Start() functions have been called
        yield return null;
        
        SetupOverlayCamera();
        CreateOverlayCanvas();
    }

    /// <summary>
    /// Set up a stacked camera that will handle rendering all UI (on top of the game)
    /// </summary>
    private void SetupOverlayCamera()
    {
        var uiCamera = CameraOverlay.instance.overlayCamera;
        
        uiCamera.CopyFrom(VRSession.Instance.MainCamera);
        uiCamera.targetTexture = null;
        uiCamera.depth = 1;
        uiCamera.transform.parent = VRSession.Instance.MainCamera.transform;
        uiCamera.clearFlags = CameraClearFlags.Depth;
        uiCamera.cullingMask = 1 << 5;
    }

    /// <summary>
    /// Creates the main overlay canvas, which is rendered in camera space
    /// </summary>
    private void CreateOverlayCanvas()
    {
        var canvas = new GameObject("VR Overlay Canvas") { layer = 5 }.AddComponent<Canvas>();
        canvas.worldCamera = CameraOverlay.instance.overlayCamera;
        canvas.renderMode = RenderMode.ScreenSpaceCamera;

        ValuableDiscover.instance.transform.SetParent(canvas.transform, false);
    }
}