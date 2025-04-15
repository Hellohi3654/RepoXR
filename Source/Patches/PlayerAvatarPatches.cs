using HarmonyLib;
using RepoXR.Managers;

namespace RepoXR.Patches;

[RepoXRPatch]
internal static class PlayerAvatarPatches
{
    /// <summary>
    /// Detect when the player has died
    /// </summary>
    [HarmonyPatch(typeof(PlayerAvatar), nameof(PlayerAvatar.PlayerDeathRPC))]
    [HarmonyPostfix]
    private static void OnPlayerDeath(PlayerAvatar __instance)
    {
        if (!__instance.isLocal || VRSession.Instance is not {} session)
            return;

        session.Player.SetRigVisible(false);
    }

    /// <summary>
    /// Detect when the player has been revived
    /// </summary>
    [HarmonyPatch(typeof(PlayerAvatar), nameof(PlayerAvatar.ReviveRPC))]
    [HarmonyPostfix]
    private static void OnPlayerRevive(PlayerAvatar __instance)
    {
        if (!__instance.isLocal || VRSession.Instance is not { } session)
            return;
        
        session.Player.SetRigVisible(true);
    }
}