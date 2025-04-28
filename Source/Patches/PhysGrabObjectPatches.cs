using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using RepoXR.Managers;
using RepoXR.Networking;
using UnityEngine;

using static HarmonyLib.AccessTools;

namespace RepoXR.Patches;

[RepoXRPatch(RepoXRPatchTarget.Universal)]
internal static class PhysGrabObjectPatches
{
    private static Transform GetTargetTransform(PlayerAvatar player)
    {
        if (player.isLocal)
            return VRSession.Instance is { } session ? session.Player.MainHand : player.localCameraTransform;

        return NetworkSystem.instance.GetNetworkPlayer(player, out var networkPlayer)
            ? networkPlayer.GrabberHand
            : player.localCameraTransform;
    }

    private static Transform GetTargetTransformGrabber(PhysGrabber grabber)
    {
        return GetTargetTransform(grabber.playerAvatar);
    }

    /// <summary>
    /// Apply object rotation based on hand rotation instead of camera rotation
    /// </summary>
    [HarmonyPatch(typeof(PhysGrabObject), nameof(PhysGrabObject.FixedUpdate))]
    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> HandRelativeMovementPatch(IEnumerable<CodeInstruction> instructions)
    {
        return new CodeMatcher(instructions)
            .MatchForward(false,
                new CodeMatch(OpCodes.Ldfld, Field(typeof(PlayerAvatar), nameof(PlayerAvatar.localCameraTransform))))
            .Repeat(matcher =>
                matcher.SetInstruction(new CodeInstruction(OpCodes.Call,
                    ((Func<PlayerAvatar, Transform>)GetTargetTransform).Method)))
            .InstructionEnumeration();
    }

    /// <summary>
    /// Apply cart steering rotation based on hand rotation instead of camera rotation
    /// </summary>
    [HarmonyPatch(typeof(PhysGrabCart), nameof(PhysGrabCart.CartSteer))]
    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> HandRelativeCartPatch(IEnumerable<CodeInstruction> instructions)
    {
        return new CodeMatcher(instructions)
            .MatchForward(false,
                new CodeMatch(OpCodes.Call,
                    Method(typeof(Mathf), nameof(Mathf.Clamp), [typeof(float), typeof(float), typeof(float)])))
            .Advance(-7)
            .SetInstruction(new CodeInstruction(OpCodes.Call,
                ((Func<PhysGrabber, Transform>)GetTargetTransformGrabber).Method))
            .MatchForward(false,
                new CodeMatch(OpCodes.Callvirt, PropertyGetter(typeof(Transform), nameof(Transform.rotation))))
            .Advance(-1)
            .SetInstruction(new CodeInstruction(OpCodes.Call,
                ((Func<PhysGrabber, Transform>)GetTargetTransformGrabber).Method))
            .MatchForward(false,
                new CodeMatch(OpCodes.Call,
                    Method(typeof(Quaternion), nameof(Quaternion.LookRotation), [typeof(Vector3), typeof(Vector3)])))
            .Advance(-7)
            .SetInstruction(new CodeInstruction(OpCodes.Call,
                ((Func<PhysGrabber, Transform>)GetTargetTransformGrabber).Method))
            .InstructionEnumeration();
    }
}