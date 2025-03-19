using HarmonyLib;
using RepoXR.Patches;
using TMPro;
using UnityEngine;

namespace RepoXR.Player;

public class VRCameraAim : MonoBehaviour
{
    public static VRCameraAim Instance;
    
    // TODO: Might not need this
    private CameraAim cameraAim;
    
    public Vector3 eulerAngles;

    private void Awake()
    {
        Instance = this;
        
        cameraAim = GetComponent<CameraAim>();
    }

    private void Update()
    {
        // TODO: Account for current head rotation
    
        if (SemiFunc.MenuLevel() && CameraNoPlayerTarget.instance)
            transform.localRotation = CameraNoPlayerTarget.instance.transform.rotation;
        else
            transform.localEulerAngles = eulerAngles;
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
        // TODO: Make helper function that accounts for current head rotation
        VRCameraAim.Instance.eulerAngles = new Vector3(0, _rotation, 0);
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
