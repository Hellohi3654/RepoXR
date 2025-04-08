using HarmonyLib;
using RepoXR.Managers;

namespace RepoXR.Patches;

[RepoXRPatch]
internal static class PlayerHealthPatches
{
    /// <summary>
    /// Make the VR rig inherit the hurt animation from the base game
    /// </summary>
    // TODO: This doesn't seem to work yet
    [HarmonyPatch(typeof(PlayerHealth), nameof(PlayerHealth.Update))]
    [HarmonyPostfix]
    private static void OnPlayerHealthUpdate(PlayerHealth __instance)
    {
        if (VRSession.Instance is not { } session)
            return;

        if (!__instance.materialEffect)
            session.Player.SetHurtAmount(0);
        else
            session.Player.SetHurtAmount(__instance.materialEffectCurve.Evaluate(__instance.materialEffectLerp));
    }
}