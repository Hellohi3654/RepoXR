using HarmonyLib;
using RepoXR.Assets;
using RepoXR.Input;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.UI;

namespace RepoXR.Managers;

/// <summary>
/// This manager is always present in every scene, whether it is a main menu, loading scene, or a level
/// </summary>
public class VRManager : MonoBehaviour
{
    private Camera mainCamera;
    private Camera overlayCamera;

    private void Awake()
    {
        // We grab all these references manually as most of the instances aren't set yet
        // Since most of them run in the "Start" lifetime function

        // Disable blocking UI
        GameObject.Find("UI/UI/Canvas").GetComponent<Canvas>().enabled = false;

        var canvas = GameObject.Find("UI/HUD/HUD Canvas").transform;
        var fade = canvas.Find("Fade");
        var video = canvas.Find("Render Texture Video");
        var loading = canvas.Find("Loading");

        // The overlay camera is always in the same position in the hierarchy, in every scene
        overlayCamera = canvas.parent.Find("Camera Overlay").GetComponent<Camera>();
        mainCamera = Camera.main!;

        // Create blocking overlay (fade + static video)
        var overlayCanvas = new GameObject("VR Overlay Canvas") { layer = 5 }.AddComponent<Canvas>();
        overlayCanvas.renderMode = RenderMode.ScreenSpaceCamera;
        overlayCanvas.worldCamera = overlayCamera;
        overlayCanvas.sortingOrder = 5; // Put a little higher up the order so it renders on top

        fade.SetParent(overlayCanvas.transform, false);
        video.SetParent(overlayCanvas.transform, false);

        // Replace original material since that one has some transparency issues
        video.GetComponent<RawImage>().material = AssetCollection.VideoOverlay;

        // Add tracking to camera
        var poseDriver = mainCamera.gameObject.AddComponent<TrackedPoseDriver>();
        poseDriver.positionAction = Actions.Instance.HeadPosition;
        poseDriver.rotationAction = Actions.Instance.HeadRotation;
        poseDriver.trackingStateInput = new InputActionProperty(Actions.Instance.HeadTrackingState);

        // Parent overlay to main camera
        overlayCamera.transform.SetParent(mainCamera.transform, false);
        overlayCamera.transform.localPosition = Vector3.zero;

        overlayCamera.depth = 2;
        overlayCamera.farClipPlane = 1000;
        overlayCamera.orthographic = false;
        overlayCamera.clearFlags = CameraClearFlags.Depth;
        overlayCamera.targetTexture = null;
        overlayCamera.nearClipPlane = 0.01f;
        
        // Disable post-processing layer on UI camera (it's sort of broken)
        Destroy(overlayCamera.GetComponent<PostProcessLayer>());
        
        // Make sure main camera renders to VR
        mainCamera.targetTexture = null;
        
        // Make sure the components on the overlay fill the entire screen
        overlayCanvas.transform.GetComponentsInChildren<RectTransform>(true).Do(rect =>
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
        });
    }
}