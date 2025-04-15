using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace RepoXR.UI;

public class GameHud : MonoBehaviour
{
    public Canvas OverlayCanvas { get; private set; }
    public Canvas SmoothedCanvas { get; private set; }

    private Transform smoothCanvasContainer;
    private Transform camera;

    private IEnumerator Start()
    {
        // Make sure all other Start() functions have been called
        yield return null;

        camera = CameraOverlay.instance.overlayCamera.transform;

        SetupOverlayCanvas();
        CreateSmoothedCanvas();

        // TODO: Temporary
        HUDCanvas.instance.transform.position = Vector3.down * 10000; // Move the world space hud far away
    }

    private void LateUpdate()
    {
        if (!camera) // Start has not yet finished
            return;

        // Keep the UI upright even during the short post-death "view from above" state
        var up = SpectateCamera.instance && SpectateCamera.instance.CheckState(SpectateCamera.State.Death)
            ? SpectateCamera.instance.transform.up
            : Vector3.up;

        var fwd = camera.position + camera.forward * 1.5f;
        var rot = Quaternion.LookRotation(camera.forward, up);

        SmoothedCanvas.transform.position = Vector3.Slerp(SmoothedCanvas.transform.position, fwd, 0.1f);
        SmoothedCanvas.transform.rotation = Quaternion.Slerp(SmoothedCanvas.transform.rotation, rot, 0.1f);
    }

    /// <summary>
    /// Creates the main overlay canvas, which is rendered in camera space
    /// </summary>
    private void SetupOverlayCanvas()
    {
        OverlayCanvas = GameObject.Find("VR Overlay Canvas").GetComponent<Canvas>();
        OverlayCanvas.worldCamera = CameraOverlay.instance.overlayCamera;
        OverlayCanvas.renderMode = RenderMode.ScreenSpaceCamera;

        // World space scale
        WorldSpaceUIParent.instance.transform.localScale = Vector3.one * 0.005f;

        // Put tumble UI on the overlay canvas, and move the top and bottom elements closer to the edge of the screen
        var tumbleUi = TumbleUI.instance;
        tumbleUi.transform.parent.SetParent(OverlayCanvas.transform, false);
        tumbleUi.transform.parent.localScale = Vector3.one * 300;
        tumbleUi.transform.parent.localPosition = Vector3.down * 20;
        tumbleUi.transform.localPosition = Vector3.back * 1.7f;
        tumbleUi.parts1[0].GetComponent<RectTransform>().anchoredPosition += Vector2.up * 0.3f;
        tumbleUi.parts1[1].GetComponent<RectTransform>().anchoredPosition += Vector2.down * 0.3f;
        tumbleUi.parts2[0].GetComponent<RectTransform>().anchoredPosition += Vector2.up * 0.3f;
        tumbleUi.parts2[1].GetComponent<RectTransform>().anchoredPosition += Vector2.down * 0.3f;
    }

    /// <summary>
    /// Creates the canvas that is displayed in front of the eyes, but far enough away so it can be smoothed
    /// </summary>
    private void CreateSmoothedCanvas()
    {
        smoothCanvasContainer = new GameObject("VR Smoothed Canvas - Container")
            { layer = 5, transform = { parent = camera.parent.parent.parent } }.transform;
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
        SmoothedCanvas.gameObject.AddComponent<RectMask2D>();

        var rect = SmoothedCanvas.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(712, 400);

        // TODO: For now we dump all the game hud onto this smoothed canvas
        var gameHud = HUDCanvas.instance.transform.Find("HUD/Game Hud");
        gameHud.transform.SetParent(rect, false);
    }
}