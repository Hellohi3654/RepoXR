using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using HarmonyLib;
using RepoXR.Managers;
using UnityEngine;

using static HarmonyLib.AccessTools;

namespace RepoXR.Patches;

[RepoXRPatch]
internal static class PhysGrabberPatches
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Transform GetHandTransform()
    {
        if (VRSession.Instance is { } session)
            return session.Player.MainHand;

        return Camera.main!.transform;
    }
    
    private static CodeMatcher ReplaceCameraWithHand(this CodeMatcher matcher)
    {
        var labels = matcher.Instruction.labels;

        return matcher.RemoveInstructions(2).InsertAndAdvance(
            new CodeInstruction(OpCodes.Call, ((Func<Transform>)GetHandTransform).Method).WithLabels(labels)
        );
    }

    [HarmonyPatch(typeof(PhysGrabber), nameof(PhysGrabber.Update))]
    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> UpdatePatches(IEnumerable<CodeInstruction> instructions)
    {
        return new CodeMatcher(instructions)
            .MatchForward(false,
                new CodeMatch(OpCodes.Ldfld, Field(typeof(PhysGrabber), nameof(PhysGrabber.playerCamera))))
            .Repeat(matcher => matcher.Advance(-1).ReplaceCameraWithHand())
            .Start()
            .MatchForward(false,
                new CodeMatch(OpCodes.Ldfld, Field(typeof(PlayerAvatar), nameof(PlayerAvatar.PlayerVisionTarget))))
            .Advance(-2)
            .RemoveInstructions(4)
            .Insert(
                new CodeInstruction(OpCodes.Call, ((Func<Transform>)GetHandTransform).Method)
            )
            .InstructionEnumeration();
    }

    /// <summary>
    /// Make sure the <see cref="PhysGrabber.physGrabPointPlane"/> and <see cref="PhysGrabber.physGrabPointPuller"/> are
    /// manually updated if we are holding something.
    ///
    /// This is normally done by having these be a child of the camera, however this doesn't work in VR since
    /// we use our hand to move items, not the main camera.
    /// </summary>
    [HarmonyPatch(typeof(PhysGrabber), nameof(PhysGrabber.Update))]
    [HarmonyPostfix]
    private static void UpdatePhysGrabPlane(PhysGrabber __instance)
    {
        if (!__instance.isLocal || !__instance.grabbedObjectTransform)
            return;

        var hand = GetHandTransform();
        var distancePlane = Vector3.Distance(hand.position, __instance.physGrabPointPlane.position);
        var distancePuller = Vector3.Distance(hand.position, __instance.physGrabPointPuller.position);
        
        __instance.physGrabPointPlane.position = hand.position + hand.forward * distancePlane;
        __instance.physGrabPointPuller.position = hand.position + hand.forward * distancePuller;
    }

    [HarmonyPatch(typeof(PhysGrabber), nameof(PhysGrabber.PhysGrabLogic))]
    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> PhysGrabLogicPatches(IEnumerable<CodeInstruction> instructions)
    {
        return new CodeMatcher(instructions)
            .MatchForward(false,
                new CodeMatch(OpCodes.Ldfld, Field(typeof(PhysGrabber), nameof(PhysGrabber.playerCamera))))
            .Repeat(matcher => matcher.Advance(-1).ReplaceCameraWithHand())
            .InstructionEnumeration();
    }
    
    /// <summary>
    /// When grabbing items, shoot rays out of the hand, instead of the camera
    /// </summary>
    [HarmonyPatch(typeof(PhysGrabber), nameof(PhysGrabber.RayCheck))]
    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> RayCheckPatches(IEnumerable<CodeInstruction> instructions)
    {
        return new CodeMatcher(instructions)
            .MatchForward(false,
                new CodeMatch(OpCodes.Call,
                    Method(typeof(Physics), nameof(Physics.Raycast),
                    [
                        typeof(Vector3), typeof(Vector3), typeof(RaycastHit).MakeByRefType(), typeof(float),
                        typeof(int),
                        typeof(QueryTriggerInteraction)
                    ])))
            .Advance(-10)
            .InsertAndAdvance(
                new CodeInstruction(OpCodes.Call, ((Func<PhysGrabber, Vector3>)CalculateNewForward).Method),
                new CodeInstruction(OpCodes.Stloc_1),
                new CodeInstruction(OpCodes.Ldarg_0)
            )
            .MatchForward(false,
                new CodeMatch(OpCodes.Ldfld, Field(typeof(PhysGrabber), nameof(PhysGrabber.playerCamera))))
            .Repeat(matcher => matcher.Advance(-1).ReplaceCameraWithHand())
            .Start()
            .MatchForward(false, new CodeMatch(OpCodes.Call, PropertyGetter(typeof(Camera), nameof(Camera.main))))
            .Repeat(matcher => matcher.ReplaceCameraWithHand())
            .InstructionEnumeration();

        static Vector3 CalculateNewForward(PhysGrabber grabber)
        {
            if (grabber.overrideGrab && grabber.overrideGrabTarget)
                return (grabber.overrideGrabTarget.transform.position - VRSession.Instance.Player.MainHand.position)
                    .normalized;

            return VRSession.Instance.Player.MainHand.forward;
        }
    }

    /// <summary>
    /// Make "scrolling" update the position based on the hand, instead of the camera
    /// </summary>
    [HarmonyPatch(typeof(PhysGrabber), nameof(PhysGrabber.OverridePullDistanceIncrement))]
    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> OverridePullDistanceIncrementPatches(
        IEnumerable<CodeInstruction> instructions)
    {
        return new CodeMatcher(instructions)
            .MatchForward(false,
                new CodeMatch(OpCodes.Ldfld, Field(typeof(PhysGrabber), nameof(PhysGrabber.playerCamera))))
            .Advance(-1)
            .ReplaceCameraWithHand()
            .InstructionEnumeration();
    }

    /// <summary>
    /// Move the grab beam origin to the hand
    /// </summary>
    [HarmonyPatch(typeof(PhysGrabBeam), nameof(PhysGrabBeam.Start))]
    [HarmonyPostfix]
    private static void OnPhysBeamStart(PhysGrabBeam __instance)
    {
        if (!__instance.playerAvatar.isLocal)
            return;
        
        __instance.PhysGrabPointOrigin.SetParent(VRSession.Instance.Player.MainHand);
        __instance.PhysGrabPointOrigin.localPosition = Vector3.zero;
    }
}