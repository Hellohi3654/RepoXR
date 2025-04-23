using System;
using UnityEngine;

namespace RepoXR.Player.Camera;

/// <summary>
/// Contrary to the name, this script actually deals with non-vr cameras (but they only exist if VR is enabled so lalalala)
/// </summary>
public class VRCustomCamera : MonoBehaviour
{
    public static VRCustomCamera instance;
    
    [SerializeField] protected UnityEngine.Camera mainCamera;
    [SerializeField] protected UnityEngine.Camera topCamera;
    [SerializeField] protected UnityEngine.Camera uiCamera;
    
    private Transform gameplayCamera;

    private void Awake()
    {
        instance = this;
        
        var fov = Plugin.Config.CustomCameraFOV.Value;

        mainCamera.fieldOfView = fov;
        topCamera.fieldOfView = fov;
        uiCamera.fieldOfView = fov;
        
        gameplayCamera = UnityEngine.Camera.main!.transform;

        Plugin.Config.CustomCameraFOV.SettingChanged += OnFOVChanged;
    }

    private void OnDestroy()
    {
        instance = null!;
        
        Plugin.Config.CustomCameraFOV.SettingChanged -= OnFOVChanged;
    }
    
    private void OnFOVChanged(object sender, EventArgs e)
    {
        var fov = Plugin.Config.CustomCameraFOV.Value;

        mainCamera.fieldOfView = fov;
        topCamera.fieldOfView = fov;
        uiCamera.fieldOfView = fov;
    }

    private void Update()
    {
        var strength = Mathf.Lerp(50, 8, Plugin.Config.CustomCameraSmoothing.Value);

        transform.localPosition = gameplayCamera.localPosition;
        transform.localRotation =
            Quaternion.Slerp(transform.localRotation, gameplayCamera.localRotation, strength * Time.deltaTime);
    }
}