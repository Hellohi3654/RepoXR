using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using RepoXR.Patches;
using UnityEngine;
using static HarmonyLib.AccessTools;

namespace RepoXR;

#if DEBUG
[RepoXRPatch(RepoXRPatchTarget.Universal)]
internal static class Experiments
{
    [HarmonyPatch(typeof(ReloadScene), nameof(ReloadScene.Awake))]
    [HarmonyPostfix]
    private static void DisableReload(ReloadScene __instance)
    {
        __instance.enabled = false;
    }

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

    [HarmonyPatch(typeof(ValuableDiscoverGraphic), nameof(ValuableDiscoverGraphic.Update))]
    [HarmonyPrefix]
    private static void KeepDiscoveryUI(ValuableDiscoverGraphic __instance)
    {
        __instance.waitTimer = 1;
    }
}
#endif