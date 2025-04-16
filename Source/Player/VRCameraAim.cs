using HarmonyLib;
using RepoXR.Input;
using RepoXR.Patches;
using UnityEngine;

namespace RepoXR.Player;

public class VRCameraAim : MonoBehaviour
{
    public static VRCameraAim Instance;
    
    private CameraAim cameraAim;
    private Transform mainCamera;
    
    private Quaternion rotation;
    private float lerpSpeed = 10;

    private void Awake()
    {
        Instance = this;
        
        cameraAim = GetComponent<CameraAim>();
        mainCamera = GetComponentInChildren<Camera>().transform;
    }

    private void Update()
    {
        cameraAim.aimVertical = mainCamera.localEulerAngles.x;
        cameraAim.aimHorizontal = mainCamera.localEulerAngles.y;
        cameraAim.playerAim = mainCamera.localRotation;
        
        // TODO: Add support for consistently looking at a Transform (e.g. for the ceiling eye)
    }

    private void LateUpdate()
    {
        transform.localRotation = Quaternion.Lerp(transform.localRotation, rotation, lerpSpeed * Time.deltaTime);
    }

    /// <summary>
    /// Instantly append a set amount of degrees to the current aim on the Y axis
    /// </summary>
    public void TurnAimNow(float degrees)
    {
        var rot = Quaternion.Euler(transform.eulerAngles + Vector3.up * degrees);

        transform.localRotation = rot;
        rotation = rot;
    }

    /// <summary>
    /// Instantly change the aim rotation without any interpolation or smoothing
    /// </summary>
    public void ForceSetRotation(Vector3 newAngles)
    {
        var rot = Quaternion.Euler(newAngles);
        
        transform.localRotation = rot;
        rotation = rot;
    }

    /// <summary>
    /// Set a new aim target which will be applied using a smooth linear interpolation
    /// </summary>
    public void SmoothSetRotation(Vector3 newAngles, float speed = 10)
    {
        rotation = Quaternion.Euler(newAngles);
        lerpSpeed = speed;
    }

    /// <summary>
    /// Set spawn rotation, which takes into account the current Y rotation of the headset
    /// </summary>
    public void SetSpawnRotation(float yRot)
    {
        if (CameraNoPlayerTarget.instance)
            yRot = CameraNoPlayerTarget.instance.transform.eulerAngles.y;
        
        var angle = new Vector3(0, yRot - TrackingInput.Instance.HeadTransform.localEulerAngles.y, 0);
        
        ForceSetRotation(angle);
    }
}

[RepoXRPatch]
internal static class CameraAimPatches
{
    /// <summary>
    /// Attach a <see cref="VRCameraAim"/> script to all <see cref="CameraAim"/> objects
    /// </summary>
    [HarmonyPatch(typeof(CameraAim), nameof(CameraAim.Awake))]
    [HarmonyPostfix]
    private static void OnCameraAimAwake(CameraAim __instance)
    {
        __instance.gameObject.AddComponent<VRCameraAim>();
    }

    /// <summary>
    /// Set initial rotation on game start
    /// </summary>
    [HarmonyPatch(typeof(CameraAim), nameof(CameraAim.CameraAimSpawn))]
    [HarmonyPostfix]
    private static void OnCameraAimSpawn(float _rotation)
    {
        VRCameraAim.Instance.SetSpawnRotation(_rotation);
    }
    
    /// <summary>
    /// Disable the game's built in <see cref="CameraAim"/> functionality, as we'll implement that manually in VR 
    /// </summary>
    [HarmonyPatch(typeof(CameraAim), nameof(CameraAim.Update))]
    [HarmonyPrefix]
    private static bool DisableCameraAim(CameraAim __instance)
    {
        return false;
    }
}
