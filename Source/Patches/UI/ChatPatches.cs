using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using RepoXR.Managers;

using static HarmonyLib.AccessTools;

namespace RepoXR.Patches.UI;

[RepoXRPatch]
internal static class ChatPatches
{
    /// <summary>
    /// Attach a custom VR chat script to the chat UI
    /// </summary>
    [HarmonyPatch(typeof(ChatUI), nameof(ChatUI.Start))]
    [HarmonyPostfix]
    private static void OnChatUICreate(ChatUI __instance)
    {
        __instance.gameObject.AddComponent<RepoXR.UI.ChatUI>();
    }

    /// <summary>
    /// Make the chat button also close the chat
    /// </summary>
    [HarmonyPatch(typeof(ChatManager), nameof(ChatManager.StateActive))]
    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> ChatCloseButtonPatch(IEnumerable<CodeInstruction> instructions)
    {
        return new CodeMatcher(instructions)
            .MatchForward(false, new CodeMatch(OpCodes.Ldc_I4_S, (sbyte)InputKey.Back))
            .SetOperandAndAdvance((sbyte)InputKey.Chat)
            .InstructionEnumeration();
    }
}