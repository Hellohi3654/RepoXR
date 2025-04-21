using HarmonyLib;
using RepoXR.UI;
using UnityEngine;

namespace RepoXR.Patches.UI;

[RepoXRPatch]
internal static class CrosshairPatches
{
    /// <summary>
    /// Create a VR crosshair
    /// </summary>
    [HarmonyPatch(typeof(Aim), nameof(Aim.Awake))]
    [HarmonyPostfix]
    private static void OnCrosshairCreate(Aim __instance)
    {
        var canvas = new GameObject("Crosshair").AddComponent<Canvas>();
        var rect = canvas.GetComponent<RectTransform>();
        
        canvas.renderMode = RenderMode.WorldSpace;
        rect.sizeDelta = new Vector2(40, 40);
        rect.localScale = Vector3.one * 0.01f;

        __instance.transform.parent = canvas.transform;
        __instance.transform.localPosition = Vector3.zero;
        __instance.transform.localRotation = Quaternion.identity;
        __instance.transform.localScale = Vector3.one;

        canvas.gameObject.AddComponent<Crosshair>();
    }
}