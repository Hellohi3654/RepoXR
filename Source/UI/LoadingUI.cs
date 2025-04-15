using System;
using System.Collections;
using HarmonyLib;
using RepoXR.Input;
using RepoXR.Patches;
using UnityEngine;
using Object = UnityEngine.Object;

namespace RepoXR.UI;

public class LoadingUI : MonoBehaviour
{
    private static Vector3 lastLocalPosition;
    private static float lastLocalRotation;
    
    private Transform camera;

    private void Awake()
    {
        camera = Camera.main!.transform;

        Logger.LogDebug($"Frame: {Time.frameCount}, Pos: {Camera.main!.transform.localPosition}");

        RestorePosition();
    }

    private IEnumerator Start()
    {
        RestorePosition();
        
        Logger.LogDebug($"Frame: {Time.frameCount}, Pos: {Camera.main!.transform.localPosition}");
        
        yield return null;

        Logger.LogDebug($"Frame: {Time.frameCount}, Pos: {Camera.main!.transform.localPosition}");
        
        RestorePosition();
    }

    private void OnDisable()
    {
        lastLocalPosition = transform.localPosition;
        lastLocalRotation = transform.localEulerAngles.y;
    }

    public void ResetPosition()
    {
        var fwd = (camera.localRotation * Vector3.forward).normalized;
        fwd.y = 0;
        fwd.Normalize();
        
        transform.localPosition = camera.transform.localPosition + fwd * 5 + Vector3.up * 0.15f;
        
        var targetPos = new Vector3(camera.localPosition.x, transform.localPosition.y, camera.localPosition.z);
        var dirToCam = -(targetPos - transform.localPosition).normalized;
        
        transform.localRotation = Quaternion.LookRotation(dirToCam);
    }

    private void RestorePosition()
    {
        transform.localPosition = lastLocalPosition;
        transform.localEulerAngles = lastLocalRotation * Vector3.up;
    }
}

// TODO: Idk something I guess
[RepoXRPatch]
internal static class LoadingUIPatches
{
    [HarmonyPatch(typeof(global::LoadingUI), nameof(global::LoadingUI.StartLoading))]
    [HarmonyPostfix]
    private static void idkasndasjkdas()
    {
        Logger.LogDebug("StartLoading");
        
        Object.FindObjectOfType<LoadingUI>().ResetPosition();
    }
}