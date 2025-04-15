using HarmonyLib;
using UnityEngine;

namespace RepoXR.Patches;

[RepoXRPatch]
internal static class WorldSpaceUIParentPatches
{
    /// <summary>
    /// Position elements actually in world space instead of screen space coordinates
    /// </summary>
    [HarmonyPatch(typeof(WorldSpaceUIChild), nameof(WorldSpaceUIChild.SetPosition))]
    [HarmonyPrefix]
    private static bool SetPositionPatch(WorldSpaceUIChild __instance)
    {
        var position = __instance.worldPosition + __instance.positionOffset;
        var direction = (position - AssetManager.instance.mainCamera.transform.position).normalized;

        __instance.myRect.position = position;
        __instance.myRect.rotation = Quaternion.LookRotation(direction, Vector3.up);

        return false;
    }
}