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

    [HarmonyPatch(typeof(LoadingUI), nameof(LoadingUI.Awake))]
    [HarmonyPrefix]
    private static void LLLLLL(LoadingUI __instance)
    {
        // __instance.enabled = false;
    }
}
#endif