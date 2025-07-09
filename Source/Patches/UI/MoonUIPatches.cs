using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using RepoXR.Assets;
using UnityEngine;
using static HarmonyLib.AccessTools;

namespace RepoXR.Patches.UI;

[RepoXRPatch]
internal static class MoonUIPatches
{
    /// <summary>
    /// Change some parameters for the Moon UI to work better in VR
    /// </summary>
    [HarmonyPatch(typeof(MoonUI), nameof(MoonUI.Awake))]
    [HarmonyPostfix]
    private static void OnMoonUICreate(MoonUI __instance)
    {
        __instance.showStartPosition = 300;
        __instance.skipStartPosition = -240;

        __instance.skipText.spriteAsset = AssetCollection.TMPInputsSpriteAsset;
    }

    /// <summary>
    /// Force the corners to have some special rotations in VR
    /// </summary>
    [HarmonyPatch(typeof(MoonUI), nameof(MoonUI.StateNone))]
    [HarmonyPostfix]
    private static void OnStateNone(MoonUI __instance)
    {
        __instance.topCornerTransformLeft.localEulerAngles = new Vector3(0, -25, 0);
        __instance.topCornerTransformRight.localEulerAngles = new Vector3(0, 25, 0);
        __instance.botCornerTransformLeft.localEulerAngles = new Vector3(0, -25, 0);
        __instance.botCornerTransformRight.localEulerAngles = new Vector3(0, 25, 0);
    }

    /// <summary>
    /// Move the Moon UI to the loading/pause canvas
    /// </summary>
    [HarmonyPatch(typeof(MoonUI), nameof(MoonUI.Update))]
    [HarmonyPostfix]
    private static void OnMoonUIUpdate(MoonUI __instance)
    {
        // Disable background (we use the fade overlay instead)
        __instance.backgroundImage.color = Color.clear;

        if (__instance.state is >= MoonUI.State.Show and < MoonUI.State.Hide)
            FadeOverlay.Instance.Image.color =
                new Color(0, 0, 0, __instance.showCurve.Evaluate(__instance.showLerp) * 0.5f);
        else if (__instance.state == MoonUI.State.Hide)
            FadeOverlay.Instance.Image.color =
                new Color(0, 0, 0, 0.5f - __instance.hideCurve.Evaluate(__instance.hideLerp) * 0.5f);
    }

    /// <summary>
    /// Perform some specific actions during state changes
    /// </summary>
    [HarmonyPatch(typeof(MoonUI), nameof(MoonUI.SetState))]
    [HarmonyPostfix]
    private static void OnSetState(MoonUI.State _state)
    {
        // Disable fade overlay if the Moon UI is done displaying
        if (_state == MoonUI.State.None)
            FadeOverlay.Instance.Image.color = Color.clear;
        // Reset the position and rotation of the canvas when we're about to show the UI
        else if (_state == MoonUI.State.Show)
            RepoXR.UI.LoadingUI.instance.ResetPosition();
    }

    /// <summary>
    /// Fix moons being rotated in world space instead of local space
    /// </summary>
    [HarmonyPatch(typeof(MoonUI), nameof(MoonUI.MoonRotateLogic))]
    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> MoonRotationFix(IEnumerable<CodeInstruction> instructions)
    {
        return new CodeMatcher(instructions)
            .MatchForward(false,
                new CodeMatch(OpCodes.Callvirt, PropertySetter(typeof(Transform), nameof(Transform.rotation))))
            .Repeat(matcher =>
                matcher.SetOperandAndAdvance(PropertySetter(typeof(Transform), nameof(Transform.localRotation))))
            .InstructionEnumeration();
    }
}