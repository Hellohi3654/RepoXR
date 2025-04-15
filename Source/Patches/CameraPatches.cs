using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;

using static HarmonyLib.AccessTools;

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

    /// <summary>
    /// Disable aim offset (the small rotation animation when loading into a level)
    /// </summary>
    [HarmonyPatch(typeof(CameraAimOffset), nameof(CameraAimOffset.Awake))]
    [HarmonyPrefix]
    private static bool DisableCameraAimOffset(CameraAimOffset __instance)
    {
        __instance.enabled = false;
        
        return false;
    }
    
    /// <summary>
    /// Patch to see if something is visible in the VR camera space
    /// </summary>
    [HarmonyPatch(typeof(SemiFunc), nameof(SemiFunc.OnScreen))]
    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> OnScreenVR(IEnumerable<CodeInstruction> instructions)
    {
        return new CodeMatcher(instructions)
            .MatchForward(false,
                new CodeMatch(OpCodes.Callvirt,
                    Method(typeof(Camera), nameof(Camera.WorldToScreenPoint), [typeof(Vector3)])))
            .SetOperandAndAdvance(Method(typeof(Camera), nameof(Camera.WorldToViewportPoint), [typeof(Vector3)]))
            .InstructionEnumeration();
    }
}