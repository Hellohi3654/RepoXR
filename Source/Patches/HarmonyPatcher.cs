using System;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace RepoXR.Patches;

internal static class HarmonyPatcher
{
    private static readonly Harmony VRPatcher = new("io.daxcess.repoxr");
    private static readonly Harmony UniversalPatcher = new("io.daxcess.repoxr-universal");

    public static void PatchUniversal()
    {
        Patch(UniversalPatcher, RepoXRPatchTarget.Universal);
    }

    public static void PatchVR()
    {
        Patch(VRPatcher, RepoXRPatchTarget.VROnly);
    }

    public static void UnpatchVR()
    {
        VRPatcher.UnpatchSelf();
    }

    private static void Patch(Harmony patcher, RepoXRPatchTarget target)
    {
        AccessTools.GetTypesFromAssembly(Assembly.GetExecutingAssembly()).Do(type =>
        {
            try
            {
                var attribute = (RepoXRPatchAttribute)Attribute.GetCustomAttribute(type, typeof(RepoXRPatchAttribute));

                if (attribute == null)
                    return;

                // TODO: Mod compat support
                /* if (attribute.Dependency != null && something)
                    return; */

                if (attribute.Target != target)
                    return;
                
                Logger.LogDebug($"Applying patches from: {type.FullName}");

                patcher.CreateClassProcessor(type, true).Patch();
            }
            catch (Exception e)
            {
                Logger.LogError($"Failed to apply patches from {type}: {e.Message}, {e.InnerException}");
            }
        });
    }
}

[AttributeUsage(AttributeTargets.Class)]
internal class RepoXRPatchAttribute(RepoXRPatchTarget target = RepoXRPatchTarget.VROnly, string? dependency = null)
    : Attribute
{
    public RepoXRPatchTarget Target { get; } = target;
    public string? Dependency { get; } = dependency;
}

internal enum RepoXRPatchTarget
{
    Universal,
    VROnly
}

[RepoXRPatch(RepoXRPatchTarget.Universal)]
internal static class HarmonyLibPatches
{
    private static readonly MethodInfo[] ForceUnpatchList =
    [
        AccessTools.PropertySetter(typeof(Camera), nameof(Camera.targetTexture)),
        AccessTools.PropertySetter(typeof(Cursor), nameof(Cursor.visible)),
        AccessTools.PropertySetter(typeof(Cursor), nameof(Cursor.lockState))
    ];

    /// <summary>
    /// Ironically, patching harmony like this fixes some issues with unpatching
    /// </summary>
    [HarmonyPatch(typeof(MethodBaseExtensions), nameof(MethodBaseExtensions.HasMethodBody))]
    [HarmonyPrefix]
    private static bool OnUnpatch(MethodBase member, ref bool __result)
    {
        if (!ForceUnpatchList.Contains(member))
            return true;

        __result = true;

        return false;
    }
}