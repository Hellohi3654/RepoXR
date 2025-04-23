using HarmonyLib;
using RepoXR.UI;

namespace RepoXR.Patches.UI;

[RepoXRPatch]
internal static class ArrowPatches
{
    /// <summary>
    /// Replace <see cref="ArrowUI"/> with <see cref="VRArrowUI"/> for better world position calculations
    /// </summary>
    // TODO: The arrow doesn't seem to work in the base game? Will handle this once I've finished the other UI stuff
    [HarmonyPatch(typeof(ArrowUI), nameof(ArrowUI.Awake))]
    [HarmonyPostfix]
    private static void OnArrowUICreate(ArrowUI __instance)
    {
        __instance.enabled = false;
        __instance.gameObject.AddComponent<VRArrowUI>();
    }
}