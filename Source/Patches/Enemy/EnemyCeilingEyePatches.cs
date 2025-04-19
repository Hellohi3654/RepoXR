using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using RepoXR.Player.Camera;
using static HarmonyLib.AccessTools;

namespace RepoXR.Patches.Enemy;

[RepoXRPatch]
internal static class EnemyCeilingEyePatches
{
    /// <summary>
    /// Replace <see cref="CameraAim.AimTargetSoftSet"/> with <see cref="VRCameraAim.SetAimTargetSoft"/>
    /// </summary>
    [HarmonyPatch(typeof(EnemyCeilingEye), nameof(EnemyCeilingEye.Update))]
    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> SetCameraSoftRotationPatch(IEnumerable<CodeInstruction> instructions)
    {
        return new CodeMatcher(instructions)
            .MatchForward(false,
                new CodeMatch(OpCodes.Callvirt, Method(typeof(CameraAim), nameof(CameraAim.AimTargetSoftSet))))
            .Advance(-11)
            .SetOperandAndAdvance(Field(typeof(VRCameraAim), nameof(VRCameraAim.instance)))
            .Advance(10)
            .SetOperandAndAdvance(Method(typeof(VRCameraAim), nameof(VRCameraAim.SetAimTargetSoft)))
            .InstructionEnumeration();
    }
    
    /// <summary>
    /// Replace <see cref="CameraAim.AimTargetSet"/> with <see cref="VRCameraAim.SetAimTarget"/>
    /// </summary>
    [HarmonyPatch(typeof(EnemyCeilingEye), nameof(EnemyCeilingEye.UpdateStateRPC))]
    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> SetCameraRotationPatch(IEnumerable<CodeInstruction> instructions)
    {
        return new CodeMatcher(instructions)
            .MatchForward(false,
                new CodeMatch(OpCodes.Callvirt, Method(typeof(CameraAim), nameof(CameraAim.AimTargetSet))))
            .Advance(-10)
            .SetOperandAndAdvance(Field(typeof(VRCameraAim), nameof(VRCameraAim.instance)))
            .Advance(9)
            .SetOperandAndAdvance(Method(typeof(VRCameraAim), nameof(VRCameraAim.SetAimTarget)))
            .InstructionEnumeration();
    }

    /// <summary>
    /// Zoom the camera a bit towards the eye when in trance
    /// </summary>
    [HarmonyPatch(typeof(EnemyCeilingEye), nameof(EnemyCeilingEye.Update))]
    [HarmonyPostfix]
    private static void CeilingEyeZoomPatch(EnemyCeilingEye __instance)
    {
        if (__instance.currentState != EnemyCeilingEye.State.HasTarget || !__instance.targetPlayer ||
            !__instance.targetPlayer.isLocal)
            return;
        
        VRCameraZoom.instance.SetZoomTarget(-0.5f, 0.1f, 1, 1, __instance.enemy.CenterTransform, 50);
    }
}