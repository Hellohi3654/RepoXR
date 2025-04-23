using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using RepoXR.Input;
using RepoXR.UI;
using UnityEngine;
using static HarmonyLib.AccessTools;

namespace RepoXR.Patches.UI;

[RepoXRPatch]
internal static class UIPatches
{
    /// <summary>
    /// Disable menu cursor
    /// </summary>
    [HarmonyPatch(typeof(MenuCursor), nameof(MenuCursor.Update))]
    [HarmonyPrefix]
    private static bool MenuCursorVRPatch(MenuCursor __instance)
    {
        __instance.gameObject.SetActive(false);

        return false;
    }

    /// <summary>
    /// Disable the menu page intro animation
    /// </summary>
    [HarmonyPatch(typeof(MenuPageMain), nameof(MenuPageMain.Start))]
    [HarmonyPostfix]
    private static void DisableMainMenuAnimation(MenuPageMain __instance)
    {
        __instance.menuPage.disableIntroAnimation = true;
        __instance.doIntroAnimation = false;
        __instance.transform.localPosition = Vector3.zero;
        __instance.waitTimer = 3;
        __instance.introDone = true;
    }

    /// <summary>
    /// Detect UI hits using VR pointers instead of mouse cursor
    /// </summary>
    [HarmonyPatch(typeof(SemiFunc), nameof(SemiFunc.UIMouseHover))]
    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> UIPointerHoverPatch(IEnumerable<CodeInstruction> instructions)
    {
        return new CodeMatcher(instructions)
            .MatchForward(false,
                new CodeMatch(OpCodes.Call, Method(typeof(SemiFunc), nameof(SemiFunc.UIMousePosToUIPos))))
            .SetOperandAndAdvance(PropertyGetter(typeof(XRRayInteractorManager), nameof(XRRayInteractorManager.Instance)))
            .InsertAndAdvance(
                new CodeInstruction(OpCodes.Ldarg_1),
                new CodeInstruction(OpCodes.Callvirt,
                    Method(typeof(XRRayInteractorManager), nameof(XRRayInteractorManager.GetUIHitPosition)))
            )
            .InstructionEnumeration();
    }

    /// <summary>
    /// Detect UI hits using VR pointers instead of mouse cursor
    /// </summary>
    [HarmonyPatch(typeof(SemiFunc), nameof(SemiFunc.UIMouseGetLocalPositionWithinRectTransform))]
    [HarmonyPrefix]
    private static bool UIMouseGetLocalPositionWithinRectTransformPatch(RectTransform rectTransform, ref Vector2 __result)
    {
        if (XRRayInteractorManager.Instance is not { } manager)
            return true;
        
        var pointer = manager.GetUIHitPosition(rectTransform);
        var rect = SemiFunc.UIGetRectTransformPositionOnScreen(rectTransform, false);

        __result = new Vector2(pointer.x - rect.x, pointer.y - rect.y);
        
        return false;
    }
    
    /// <summary>
    /// Calculate component position on canvasses in local space since screen space canvasses are disabled
    /// </summary>
    [HarmonyPatch(typeof(SemiFunc), nameof(SemiFunc.UIGetRectTransformPositionOnScreen))]
    [HarmonyPostfix]
    private static void UIGetRectTransformPositionOnCanvas(RectTransform rectTransform, ref Vector2 __result)
    {
        var canvas = rectTransform.GetComponentInParent<Canvas>().transform;
        var local = canvas.InverseTransformPoint(rectTransform.position);
        
        __result = new Vector2(local.x, local.y);
    }

    /// <summary>
    /// Reset rotation on opened pages so it matches canvas rotation
    /// </summary>
    [HarmonyPatch(typeof(MenuManager), nameof(MenuManager.PageOpen))]
    [HarmonyPostfix]
    private static void OnPageOpen(MenuPage __result)
    {
        __result.transform.localEulerAngles = Vector3.zero;
    }

    /// <summary>
    /// Fix the button hover outline position
    /// </summary>
    [HarmonyPatch(typeof(MenuSelectionBoxTop), nameof(MenuSelectionBoxTop.Update))]
    [HarmonyPostfix]
    private static void FixButtonOverlayPosition(MenuSelectionBoxTop __instance)
    {
        if (!MenuManager.instance.activeSelectionBox)
            return;

        var currentScale = __instance.GetComponentInParent<Canvas>().transform.localScale;
        var targetScale = MenuManager.instance.activeSelectionBox.GetComponentInParent<Canvas>().transform.localScale;
        var scaleFactor = new Vector3(
            currentScale.x != 0f ? targetScale.x / currentScale.x : 0f,
            currentScale.y != 0f ? targetScale.y / currentScale.y : 0f,
            currentScale.z != 0f ? targetScale.z / currentScale.z : 0f
        );
        
        __instance.rectTransform.position = MenuManager.instance.activeSelectionBox.rectTransform.position;
        // __instance.rectTransform.rotation = MenuManager.instance.activeSelectionBox.rectTransform.rotation;
        // __instance.rectTransform.parent.localScale = scaleFactor;
    }
    
    /// <summary>
    /// Handle VR inputs for UI buttons
    /// </summary>
    // TODO: Maybe look at also patching out the original code? Since the mouse can sometimes click on UI elements.
    [HarmonyPatch(typeof(MenuButton), nameof(MenuButton.HoverLogic))]
    [HarmonyPostfix]
    private static void HandleVRButtonLogic(MenuButton __instance)
    {
        var manager = XRRayInteractorManager.Instance;
        
        if (!__instance.hovering || manager == null)
            return;

        if (manager.GetTriggerDown())
        {
            __instance.OnSelect();
            __instance.holdTimer = 0;
            __instance.clickTimer = 0.2f;
        }

        if (!__instance.hasHold)
            return;
        
        if (manager.GetTriggerButton())
            __instance.holdTimer += Time.deltaTime;
        else
        {
            __instance.holdTimer = 0;
            __instance.clickFrequencyTicker = 0;
            __instance.clickFrequency = 0.2f;
        }
    }

    /// <summary>
    /// Handle VR inputs for scroll boxes
    /// </summary>
    [HarmonyPatch(typeof(MenuScrollBox), nameof(MenuScrollBox.Update))]
    [HarmonyPostfix]
    private static void HandleVRScrollLogic(MenuScrollBox __instance)
    {
        var manager = XRRayInteractorManager.Instance;

        if (!__instance.scrollBar.activeSelf || !__instance.scrollBoxActive || manager == null)
            return;

        if (manager.GetTriggerButton() && SemiFunc.UIMouseHover(__instance.parentPage, __instance.scrollBarBackground,
                __instance.menuSelectableElement.menuID))
        {
            var pos = SemiFunc.UIMouseGetLocalPositionWithinRectTransform(__instance.scrollBarBackground).y;

            if (pos < __instance.scrollHandle.sizeDelta.y / 2)
                pos = __instance.scrollHandle.sizeDelta.y / 2;

            if (pos > __instance.scrollBarBackground.rect.height - __instance.scrollHandle.sizeDelta.y / 2)
                pos = __instance.scrollBarBackground.rect.height - __instance.scrollHandle.sizeDelta.y / 2;

            __instance.scrollHandleTargetPosition = pos;
        }

        if (manager.GetUIScrollY() != 0)
        {
            __instance.scrollHandleTargetPosition += manager.GetUIScrollY() * 20 / (__instance.scrollHeight * 0.01f);
            if (__instance.scrollHandleTargetPosition < __instance.scrollHandle.sizeDelta.y / 2f)
                __instance.scrollHandleTargetPosition = __instance.scrollHandle.sizeDelta.y / 2f;
            if (__instance.scrollHandleTargetPosition >
                __instance.scrollBarBackground.rect.height - __instance.scrollHandle.sizeDelta.y / 2f)
                __instance.scrollHandleTargetPosition = __instance.scrollBarBackground.rect.height -
                                                        __instance.scrollHandle.sizeDelta.y / 2f;
        }
    }

    /// <summary>
    /// Disable scrolling using the built-in keybinds (Movement and Scroll), in favor of XR UI Scroll
    /// </summary>
    [HarmonyPatch(typeof(MenuScrollBox), nameof(MenuScrollBox.Update))]
    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> ScrollDisableInputs(IEnumerable<CodeInstruction> instructions)
    {
        return new CodeMatcher(instructions)
            .MatchForward(false, new CodeMatch(OpCodes.Call, Method(typeof(SemiFunc), nameof(SemiFunc.InputMovementY))))
            // Can't just replace the instruction as it has labels we need to keep
            .SetOpcodeAndAdvance(OpCodes.Ldc_R4).Advance(-1).SetOperandAndAdvance(0.0f)
            .MatchForward(false, new CodeMatch(OpCodes.Call, Method(typeof(SemiFunc), nameof(SemiFunc.InputScrollY))))
            // Don't have to keep labels for this one
            .SetInstruction(new CodeInstruction(OpCodes.Ldc_R4, 0.0f))
            .InstructionEnumeration();
    }

    /// <summary>
    /// Detect when the controls settings page is opened
    /// </summary>
    [HarmonyPatch(typeof(MenuPageSettingsControls), nameof(MenuPageSettingsControls.Start))]
    [HarmonyPostfix]
    private static void OnControlsPageOpened(MenuPageSettingsControls __instance)
    {
        __instance.gameObject.AddComponent<RebindManager>();
    }
}