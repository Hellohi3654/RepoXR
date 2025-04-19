using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using RepoXR.Managers;
using RepoXR.Player;
using UnityEngine;

using static HarmonyLib.AccessTools;

namespace RepoXR.Patches.Player;

[RepoXRPatch]
internal static class MapToolPatches
{
    private const float MAP_HOLD_ANGLE = 300f;
    
    [HarmonyPatch(typeof(MapToolController), nameof(MapToolController.Start))]
    [HarmonyPostfix]
    private static void OnMapToolCreated(MapToolController __instance)
    {
        if (!__instance.PlayerAvatar.isLocal || VRSession.Instance is not {} session)
            return;

        __instance.transform.parent.parent = session.Player.MapParent;
        __instance.transform.parent.localPosition = Vector3.zero;
        __instance.transform.parent.localRotation = Quaternion.identity;
        __instance.gameObject.AddComponent<VRMapTool>();
    }

    /// <summary>
    /// Disable all the input detection code in the map tool as we're shipping our own
    /// </summary>
    [HarmonyPatch(typeof(MapToolController), nameof(MapToolController.Update))]
    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> MapToolDisableInput(IEnumerable<CodeInstruction> instructions)
    {
        return new CodeMatcher(instructions)
            .Advance(1)
            .RemoveInstructions(87)
            .InstructionEnumeration();
    }

    /// <summary>
    /// Set the minimum size of the map tool to be 25% instead of 0%
    /// </summary>
    [HarmonyPatch(typeof(MapToolController), nameof(MapToolController.Update))]
    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> MapToolScalePatch(IEnumerable<CodeInstruction> instructions)
    {
        return new CodeMatcher(instructions)
            .MatchForward(false,
                new CodeMatch(OpCodes.Ldfld, Field(typeof(MapToolController), nameof(MapToolController.IntroCurve))))
            .Advance(-3)
            .SetInstructionAndAdvance(new CodeInstruction(OpCodes.Ldarg_0))
            .InsertAndAdvance(new CodeInstruction(OpCodes.Call,
                ((Func<MapToolController, float>)GetMaximumScale).Method))
            .SetInstructionAndAdvance(new CodeInstruction(OpCodes.Ldarg_0))
            .InsertAndAdvance(new CodeInstruction(OpCodes.Call,
                ((Func<MapToolController, float>)GetMinimumScale).Method))
            .MatchForward(false,
                new CodeMatch(OpCodes.Ldfld, Field(typeof(MapToolController), nameof(MapToolController.OutroCurve))))
            .Advance(-3)
            .SetInstructionAndAdvance(new CodeInstruction(OpCodes.Ldarg_0))
            .InsertAndAdvance(new CodeInstruction(OpCodes.Call,
                ((Func<MapToolController, float>)GetMaximumScale).Method))
            .SetInstructionAndAdvance(new CodeInstruction(OpCodes.Ldarg_0))
            .InsertAndAdvance(new CodeInstruction(OpCodes.Call,
                ((Func<MapToolController, float>)GetMinimumScale).Method))
            .InstructionEnumeration();

        static float GetMaximumScale(MapToolController controller) =>
            controller.PlayerAvatar.isLocal && !SemiFunc.MenuLevel() ? 0.75f : 0;

        static float GetMinimumScale(MapToolController controller) =>
            controller.PlayerAvatar.isLocal && !SemiFunc.MenuLevel() ? 0.25f : 0;
    }

    /// <summary>
    /// Make sure the map tool doesn't disappear when it's not held
    /// </summary>
    [HarmonyPatch(typeof(MapToolController), nameof(MapToolController.Update))]
    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> MapToolVisibilityPatch(IEnumerable<CodeInstruction> instructions)
    {
        return new CodeMatcher(instructions)
            .MatchForward(false,
                new CodeMatch(OpCodes.Ldfld,
                    Field(typeof(MapToolController), nameof(MapToolController.VisualTransform))))
            .Advance(3)
            .InsertAndAdvance(
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Call, ((Func<bool, MapToolController, bool>)FuckYouSpraty).Method)
            )
            .MatchForward(false,
                new CodeMatch(OpCodes.Ldfld,
                    Field(typeof(MapToolController), nameof(MapToolController.VisualTransform))))
            .Advance(3)
            .InsertAndAdvance(
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Call, ((Func<bool, MapToolController, bool>)FuckYouSpraty).Method)
            )
            .InstructionEnumeration();

        // For lore reasons this name cannot change
        static bool FuckYouSpraty(bool original, MapToolController controller)
        {
            return controller.PlayerAvatar.isLocal || original;
        }
    }

    /// <summary>
    /// Fixes the pickup animation for the map tool in VR
    /// </summary>
    [HarmonyPatch(typeof(MapToolController), nameof(MapToolController.Update))]
    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> MapToolAnimationPatch(IEnumerable<CodeInstruction> instructions)
    {
        return new CodeMatcher(instructions)
            // Fix intro animation
            .MatchForward(false, new CodeMatch(OpCodes.Ldc_R4, 90f))
            .SetOperandAndAdvance(MAP_HOLD_ANGLE)
            .MatchForward(false,
                new CodeMatch(OpCodes.Call,
                    Method(typeof(Quaternion), nameof(Quaternion.Euler),
                        [typeof(float), typeof(float), typeof(float)])))
            .Advance(3)
            .Insert(new CodeInstruction(OpCodes.Ldc_R4, 1f))
            .Advance(3)
            .Insert(new CodeInstruction(OpCodes.Sub))
            // Fix outro animation
            .MatchForward(false, new CodeMatch(OpCodes.Ldfld, Field(typeof(MapToolController), nameof(MapToolController.OutroCurve))))
            .Advance(5)
            .Insert(
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Call, ((Action<MapToolController>)OutroAnimation).Method)
            )
            .InstructionEnumeration();

        static void OutroAnimation(MapToolController controller)
        {
            controller.HideTransform.localRotation = Quaternion.Slerp(Quaternion.Euler(MAP_HOLD_ANGLE, 0, 0),
                Quaternion.identity, controller.OutroCurve.Evaluate(controller.HideLerp));
        }
    }
}