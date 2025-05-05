using HarmonyLib;
using RepoXR.UI;

namespace RepoXR.Patches.UI;

[RepoXRPatch]
internal static class PasswordMenuPatches
{
    /// <summary>
    /// Make the password menu work in VR
    /// </summary>
    [HarmonyPatch(typeof(MenuPagePassword), nameof(MenuPagePassword.Start))]
    [HarmonyPostfix]
    private static void OnMenuPagePasswordShown(MenuPagePassword __instance)
    {
        __instance.gameObject.AddComponent<PasswordUI>();
    }

    /// <summary>
    /// Detect when the password has been submitted
    /// </summary>
    [HarmonyPatch(typeof(MenuPagePassword), nameof(MenuPagePassword.ConfirmButton))]
    [HarmonyPostfix]
    private static void OnMenuPagePasswordConfirm(MenuPagePassword __instance)
    {
        __instance.GetComponent<PasswordUI>().OnConfirm();
    }
}