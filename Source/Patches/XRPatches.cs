using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.InputSystem.XR;
using UnityEngine.Rendering;
using UnityEngine.XR;
using static HarmonyLib.AccessTools;

namespace RepoXR.Patches;

[RepoXRPatch]
internal static class XRPatches
{
    private static readonly int ClearColor = Shader.PropertyToID("_ClearColor");

    /// <summary>
    /// Fixes some issues with the input system
    /// </summary>
    [HarmonyPatch(typeof(XRSupport), nameof(XRSupport.Initialize))]
    [HarmonyPrefix]
    private static bool OnBeforeInitialize()
    {
        return false;
    }

    /// <summary>
    /// Make the occlusion mesh color black
    /// </summary>
    [HarmonyPatch(typeof(XRSystem), nameof(XRSystem.Initialize))]
    [HarmonyPostfix]
    private static void OnXRSystemInitialize()
    {
        XRSystem.s_OcclusionMeshMaterial?.SetVector(ClearColor, new Vector4(1, 0, 0, 1));
    }
}