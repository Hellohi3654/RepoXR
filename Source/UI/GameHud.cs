using System.Collections;
using RepoXR.Managers;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.UI;

namespace RepoXR.UI;

public class GameHud : MonoBehaviour
{
    public Canvas OverlayCanvas { get; private set; }
    public Canvas SmoothedCanvas { get; private set; }

    private Transform smoothCanvasContainer;
    private Transform camera;
    
    private void Awake()
    {
        // Disable main render texture which obscures the flatscreen mirror view
        RenderTextureMain.instance.transform.GetComponentInParent<Canvas>().enabled = false;
    }

    private IEnumerator Start()
    {
        // Make sure all other Start() functions have been called
        yield return null;

        camera = CameraOverlay.instance.overlayCamera.transform;
        
        SetupOverlayCamera();
        CreateOverlayCanvas();
        CreateSmoothedCanvas();
        
        // TODO: Temporary
        HUDCanvas.instance.transform.position = Vector3.down * 10000; // Move the world space hud far away
    }

    private void LateUpdate()
    {
        if (!camera) // Start has not yet finished
            return;

        smoothCanvasContainer.transform.position = camera.position;

        var fwd = new Vector3(camera.forward.x, camera.forward.y, camera.forward.z).normalized * 1.5f;
        var rot = Quaternion.Euler(camera.eulerAngles.x, camera.eulerAngles.y, 0);

        SmoothedCanvas.transform.localPosition = Vector3.Slerp(SmoothedCanvas.transform.localPosition, fwd, 0.1f);
        SmoothedCanvas.transform.rotation = Quaternion.Slerp(SmoothedCanvas.transform.rotation, rot, 0.1f);
    }

    /// <summary>
    /// Set up a stacked camera that will handle rendering all UI (on top of the game)
    /// </summary>
    private void SetupOverlayCamera()
    {
        var uiCamera = CameraOverlay.instance.overlayCamera;
        
        uiCamera.CopyFrom(VRSession.Instance.MainCamera);
        uiCamera.targetTexture = null;
        uiCamera.depth = 2;
        uiCamera.transform.parent = VRSession.Instance.MainCamera.transform;
        uiCamera.clearFlags = CameraClearFlags.Depth;
        uiCamera.cullingMask = 1 << 5;
        uiCamera.farClipPlane = 1500;
        
        // Disable post-processing on UI layer
        Destroy(uiCamera.GetComponent<PostProcessLayer>());
    }

    /// <summary>
    /// Creates the main overlay canvas, which is rendered in camera space
    /// </summary>
    private void CreateOverlayCanvas()
    {
        OverlayCanvas = new GameObject("VR Overlay Canvas") { layer = 5 }.AddComponent<Canvas>();
        OverlayCanvas.worldCamera = CameraOverlay.instance.overlayCamera;
        OverlayCanvas.renderMode = RenderMode.ScreenSpaceCamera;
        
        // World space scale
        WorldSpaceUIParent.instance.transform.localScale = Vector3.one * 0.005f;
        
        // Video overlay
        VideoOverlay.Instance.transform.parent = OverlayCanvas.transform;
        VideoOverlay.Instance.transform.localPosition = Vector3.zero;
        VideoOverlay.Instance.transform.localEulerAngles = Vector3.zero;
        VideoOverlay.Instance.transform.localScale = Vector3.one * 2.5f;

        // Put tumble UI on the overlay canvas, and move the top and bottom elements closer to the edge of the screen
        var tumbleUi = TumbleUI.instance;
        tumbleUi.transform.parent.SetParent(OverlayCanvas.transform, false);
        tumbleUi.transform.localPosition = Vector3.back * 1.7f;
        tumbleUi.parts1[0].GetComponent<RectTransform>().anchoredPosition += Vector2.up * 0.1f;
        tumbleUi.parts1[1].GetComponent<RectTransform>().anchoredPosition += Vector2.down * 0.1f;
        tumbleUi.parts2[0].GetComponent<RectTransform>().anchoredPosition += Vector2.up * 0.1f;
        tumbleUi.parts2[1].GetComponent<RectTransform>().anchoredPosition += Vector2.down * 0.1f;
    }

    /// <summary>
    /// Creates the canvas that is displayed in front of the eyes, but far enough away so it can be smoothed
    /// </summary>
    private void CreateSmoothedCanvas()
    {
        smoothCanvasContainer = new GameObject("VR Smoothed Canvas - Container") { layer = 5 }.transform;
        SmoothedCanvas = new GameObject("VR Smoothed Canvas")
            {
                layer = 5,
                transform =
                {
                    parent = smoothCanvasContainer,
                    localScale = Vector3.one * 0.003f,
                    localPosition = Vector3.zero,
                    localEulerAngles = Vector3.zero
                }
            }
            .AddComponent<Canvas>();
        SmoothedCanvas.renderMode = RenderMode.WorldSpace;
        SmoothedCanvas.gameObject.AddComponent<Mask>();
        SmoothedCanvas.gameObject.AddComponent<Image>().color = new Color(0, 0, 0, 0);

        var rect = SmoothedCanvas.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(712, 400);

        // TODO: For now we dump all the game hud onto this smoothed canvas
        var gameHud = HUDCanvas.instance.transform.Find("HUD/Game Hud");
        gameHud.transform.SetParent(rect, false);
    }
}