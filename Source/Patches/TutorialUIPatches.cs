using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using RepoXR.Assets;
using TMPro;

using static HarmonyLib.AccessTools;

namespace RepoXR.Patches;

[RepoXRPatch]
internal static class TutorialUIPatches
{
    private static TMP_SpriteAsset originalEmojis;

    [HarmonyPatch(typeof(TutorialUI), nameof(TutorialUI.Start))]
    [HarmonyPostfix]
    private static void OnTutorialStart(TutorialUI __instance)
    {
        // Copy a reference to the original sprite asset
        originalEmojis = __instance.Text.spriteAsset;

        // Update the dummy text to use our input icons
        __instance.dummyText.spriteAsset = AssetCollection.TMPInputsSpriteAsset;
    }
    
    /// <summary>
    /// Update the tutorial UI to use our input icons
    /// </summary>
    [HarmonyPatch(typeof(TutorialUI), nameof(TutorialUI.SetPage))]
    [HarmonyPrefix]
    private static void UpdateTextSpriteAtlas(TutorialUI __instance, ref string dummyTextString, bool transition)
    {
        __instance.Text.spriteAsset = transition ? originalEmojis : AssetCollection.TMPInputsSpriteAsset;

        dummyTextString = dummyTextString.Replace("keyboard", "controller");
    }

    /// <summary>
    /// Make sure the sprite asset is reverted back to ours after the "good job" message
    /// </summary>
    [HarmonyPatch(typeof(TutorialUI), nameof(TutorialUI.SwitchPage), MethodType.Enumerator)]
    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> SwitchPagePatch(IEnumerable<CodeInstruction> instructions)
    {
        return new CodeMatcher(instructions)
            .MatchForward(false,
                new CodeMatch(OpCodes.Callvirt, PropertySetter(typeof(TMP_Text), nameof(TMP_Text.text))))
            .InsertAndAdvance(
                new CodeInstruction(OpCodes.Ldloc_1),
                new CodeInstruction(OpCodes.Call, ((Action<TutorialUI>)SetSpriteAtlas).Method)
            )
            .InstructionEnumeration();

        static void SetSpriteAtlas(TutorialUI ui)
        {
            ui.Text.spriteAsset = AssetCollection.TMPInputsSpriteAsset;
        }
    }
}