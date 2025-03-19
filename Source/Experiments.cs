using HarmonyLib;
using RepoXR.Patches;

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
}
#endif