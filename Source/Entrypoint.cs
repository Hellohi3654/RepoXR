using HarmonyLib;
using RepoXR.Input;
using RepoXR.Managers;
using RepoXR.Patches;
using RepoXR.UI;
using UnityEngine;

namespace RepoXR;

[RepoXRPatch]
internal static class Entrypoint
{
    public static void OnSceneLoad(string sceneName)
    {
        Logger.LogDebug($"Scene name: {sceneName}");
        
        // Global add UI aaaaaah
        GameObject.Find("UI").AddComponent<VRManager>();

        switch (sceneName)
        {
            case "Main":
                break;
            
            case "Reload":
                new GameObject("Reload Scene VR").AddComponent<ReloadMenu>();
                break;
        }
    }
    
    /// <summary>
    /// <see cref="GameDirector"/> is always present in the `Main` scene, so we use it as entrypoint
    /// </summary>
    [HarmonyPatch(typeof(GameDirector), nameof(GameDirector.Start))]
    [HarmonyPostfix]
    private static void OnStartup(GameDirector __instance)
    {
        VRInputSystem.Instance.ActivateInput();
        
        if (RunManager.instance.levelCurrent == RunManager.instance.levelMainMenu ||
            RunManager.instance.levelCurrent == RunManager.instance.levelLobbyMenu)
            OnStartupMainMenu();
        else
            OnStartupInGame();
    }

    /// <summary>
    /// The reload scene is a scene used for a short amount of time while waiting for players to load
    /// </summary>
    [HarmonyPatch(typeof(ReloadScene), nameof(ReloadScene.Awake))]
    [HarmonyPostfix]
    private static void OnReloadScene()
    {
        // Do this in a new game object, as this fixes some timing issues
        // new GameObject("VR Reload Scene Menu").AddComponent<ReloadMenu>();
    }

    private static void OnStartupMainMenu()
    {
        HUDCanvas.instance.gameObject.AddComponent<MainMenu>();
    }

    private static void OnStartupInGame()
    {
        GameDirector.instance.gameObject.AddComponent<VRSession>();
    }
}