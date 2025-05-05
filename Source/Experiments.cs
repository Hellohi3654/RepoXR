using System.Diagnostics;
using HarmonyLib;
using RepoXR.Patches;

namespace RepoXR;

#if DEBUG
// [RepoXRPatch(RepoXRPatchTarget.Universal)]
internal static class Experiments
{
    [HarmonyPatch(typeof(EnemyDirector), nameof(EnemyDirector.Awake))]
    [HarmonyPostfix]
    private static void FuckLolEnemy(EnemyDirector __instance)
    {
        // Only allow eyeyeyeyeyeye spawning
        var enemy = __instance.enemiesDifficulty1[2];
        
        // Only allow thin-man spawning
        // var enemy = __instance.enemiesDifficulty1[1];
        
        // Only allow upSCREAM! spawning
        // var enemy = __instance.enemiesDifficulty2[2];
        
        __instance.enemiesDifficulty1.Clear();
        __instance.enemiesDifficulty2.Clear();
        __instance.enemiesDifficulty3.Clear();
        
        __instance.enemiesDifficulty1.Add(enemy);
        __instance.enemiesDifficulty2.Add(enemy);
        __instance.enemiesDifficulty3.Add(enemy);
    }

    [HarmonyPatch(typeof(PlayerController), nameof(PlayerController.FixedUpdate))]
    [HarmonyPostfix]
    private static void InfiniteSprintPatch(PlayerController __instance)
    {
        __instance.EnergyCurrent = __instance.EnergyStart;
    }

    [HarmonyPatch(typeof(MenuPageSettings), nameof(MenuPageSettings.ButtonEventControls))]
    [HarmonyPrefix]
    private static void A()
    {
        Logger.LogDebug(new StackTrace());
    }
}
#endif