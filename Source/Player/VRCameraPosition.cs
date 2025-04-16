using System;
using HarmonyLib;
using RepoXR.Patches;
using UnityEngine;

namespace RepoXR.Player;

[DefaultExecutionOrder(100)]
public class VRCameraPosition : MonoBehaviour
{
    public static VRCameraPosition Instance;

    public CameraPosition original;
    public Vector3 additionalOffset;

    private void Awake()
    {
        Instance = this;
        original = GetComponent<CameraPosition>();
    }

    private void Update()
    {
        transform.localPosition += additionalOffset;
    }
}

[RepoXRPatch]
internal static class CameraPositionPatches
{
    /// <summary>
    /// Attach a <see cref="VRCameraPosition"/> to any <see cref="CameraPosition"/> game object
    /// </summary>
    [HarmonyPatch(typeof(CameraPosition), nameof(CameraPosition.Awake))]
    [HarmonyPostfix]
    private static void OnCreateCameraPosition(CameraPosition __instance)
    {
        __instance.gameObject.AddComponent<VRCameraPosition>();
    }
}