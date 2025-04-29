using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;

using static HarmonyLib.AccessTools;

namespace RepoXR.Patches;

/// <summary>
/// The idea of all these patches are to reduce and nullify some of the camera effects as they can be nauseating in VR
/// </summary>
// TODO: Need to do a bunch more
[RepoXRPatch]
internal static class CameraEffectsPatches
{
    /// <summary>
    /// Camera noise is the idle sway, which is a nono in VR
    /// </summary>
    [HarmonyPatch(typeof(CameraNoise), nameof(CameraNoise.Awake))]
    [HarmonyPostfix]
    private static void DisableCameraNoise(CameraNoise __instance)
    {
        __instance.AnimNoise.enabled = false;
        __instance.transform.localPosition = Vector3.zero;
        __instance.transform.localRotation = Quaternion.identity;
    }

    /// <summary>
    /// Reduce camera bobbing by 75%
    /// </summary>
    [HarmonyPatch(typeof(CameraBob), nameof(CameraBob.Update))]
    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> ReduceCameraBob(IEnumerable<CodeInstruction> instructions)
    {
        return new CodeMatcher(instructions)
            .MatchForward(false,
                new CodeMatch(OpCodes.Ldfld, Field(typeof(GameplayManager), nameof(GameplayManager.cameraAnimation))))
            .Repeat(matcher => matcher.Advance(1).InsertAndAdvance(
                new CodeInstruction(OpCodes.Ldc_R4, 0.25f),
                new CodeInstruction(OpCodes.Mul)
            ))
            .InstructionEnumeration();
    }
}