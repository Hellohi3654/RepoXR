using HarmonyLib;
using UnityEngine;

namespace RepoXR.Patches;

[RepoXRPatch]
internal static class CameraPatches
{
    /// <summary>
    /// Prevent setting camera target texture since in VR we need to render directly from the gameplay camera
    /// </summary>
    [HarmonyPatch(typeof(Camera), nameof(Camera.targetTexture), MethodType.Setter)]
    [HarmonyPrefix]
    private static void DisableTargetTextureOverride(Camera __instance, ref RenderTexture? value)
    {
        value = null;
    }

    /// <summary>
    /// Disable the main menu camera pan when booting the game
    /// </summary>
    [HarmonyPatch(typeof(CameraMainMenu), nameof(CameraMainMenu.Awake))]
    [HarmonyPostfix]
    private static void DisableMainMenuAnimation(CameraMainMenu __instance)
    {
        __instance.introLerp = 1;
    }
}