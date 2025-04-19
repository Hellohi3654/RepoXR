using HarmonyLib;

namespace RepoXR.Patches.Player;

[RepoXRPatch]
internal static class FlashlightPatches
{
    /// <summary>
    /// Disable a few of the flashlight script locally since it doesn't really work well with VR
    /// </summary>
    [HarmonyPatch(typeof(FlashlightController), nameof(FlashlightController.Start))]
    [HarmonyPrefix]
    private static void OnFlashlightStart(FlashlightController __instance)
    {
        if (!__instance.PlayerAvatar.isLocal)
            return;

        __instance.GetComponentInChildren<FlashlightBob>().enabled = false;
        __instance.GetComponentInChildren<FlashlightSprint>().enabled = false;
        __instance.GetComponentInChildren<FlashlightTilt>().enabled = false;
    }
}