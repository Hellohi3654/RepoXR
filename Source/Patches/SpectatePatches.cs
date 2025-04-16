using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;

using static HarmonyLib.AccessTools;

namespace RepoXR.Patches;

[RepoXRPatch]
internal static class SpectatePatches
{
    /// <summary>
    /// Aim camera towards top-down view on death
    /// </summary>
    [HarmonyPatch(typeof(SpectateCamera), nameof(SpectateCamera.UpdateState))]
    [HarmonyPostfix]
    private static void OnUpdateState(SpectateCamera __instance, SpectateCamera.State _state)
    {
        var offsetTransform = CameraAimOffset.Instance.transform;
        
        if (_state == SpectateCamera.State.Death)
        {
            offsetTransform.localEulerAngles = Camera.main!.transform.localEulerAngles.y * Vector3.down;
            offsetTransform.localPosition = Vector3.back * 50;
        }
        else
        {
            offsetTransform.localRotation = Quaternion.identity;
            offsetTransform.localPosition = Vector3.zero;
        }
    }

    /// <summary>
    /// Add an offset to the near clip plane to fix some visual issues with VR
    /// </summary>
    [HarmonyPatch(typeof(SpectateCamera), nameof(SpectateCamera.DeathNearClipLogic))]
    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> NearClipPatches(IEnumerable<CodeInstruction> instructions)
    {
        return new CodeMatcher(instructions)
            .MatchForward(false, new CodeMatch(OpCodes.Ldc_R4, 0.5f))
            .SetOperandAndAdvance(-20f)
            .InstructionEnumeration();
    }

    /// <summary>
    /// Double the far clip plane to prevent some visual issues in VR
    /// </summary>
    [HarmonyPatch(typeof(SpectateCamera), nameof(SpectateCamera.StateDeath))]
    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> IncreaseFarPlanePatch(IEnumerable<CodeInstruction> instructions)
    {
        return new CodeMatcher(instructions)
            .MatchForward(false, new CodeMatch(OpCodes.Ldc_R4, 90f))
            .SetOperandAndAdvance(180f)
            .InstructionEnumeration();
    }

    /// <summary>
    /// Prevent the mouse from rotating the spectator camera during the top-down view
    /// </summary>
    [HarmonyPatch(typeof(SpectateCamera), nameof(SpectateCamera.StateDeath))]
    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> DisableMouseRotationPatch(IEnumerable<CodeInstruction> instructions)
    {
        return new CodeMatcher(instructions)
            .MatchForward(false,
                new CodeMatch(OpCodes.Call,
                    Method(typeof(Quaternion), nameof(Quaternion.Euler),
                        [typeof(float), typeof(float), typeof(float)])))
            .Advance(-3)
            .RemoveInstructions(2)
            .Insert(new CodeInstruction(OpCodes.Ldc_R4, 0f))
            .InstructionEnumeration();
    }
}