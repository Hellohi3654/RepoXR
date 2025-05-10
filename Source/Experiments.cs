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
        // var enemy = __instance.enemiesDifficulty1[0];
        
        // Only allow mouth spawning
        // var enemy = __instance.enemiesDifficulty1[4];
        
        // Only allow thin-man spawning
        // var enemy = __instance.enemiesDifficulty1[1];
        
        // Only allow upSCREAM! spawning
        // var enemy = __instance.enemiesDifficulty2[2];
        
        // Only allow beamer spawning
        var enemy = __instance.enemiesDifficulty3[4];

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

    [HarmonyPatch(typeof(PlayerHealth), nameof(PlayerHealth.Hurt))]
    [HarmonyPrefix]
    private static bool NoDamage()
    {
        return false;
    }

    [HarmonyPatch(typeof(RunManager), nameof(RunManager.Awake))]
    [HarmonyPrefix]
    private static void HeheMuseum(RunManager __instance)
    {
        if (RunManager.instance || true)
            return;
        
        __instance.levels.RemoveRange(0, 1);
        __instance.levels.RemoveRange(1, 2);
        __instance.levels[0].NarrativeName = "Wie dit leest is gek";

        __instance.levels[0].ValuablePresets.RemoveAt(0);

        var hourglass = __instance.levels[0].ValuablePresets[0].medium[1];
        
        __instance.levels[0].ValuablePresets[0].tiny.Clear();
        __instance.levels[0].ValuablePresets[0].small.Clear();
        __instance.levels[0].ValuablePresets[0].medium.Clear();
        __instance.levels[0].ValuablePresets[0].big.Clear();
        __instance.levels[0].ValuablePresets[0].wide.Clear();
        __instance.levels[0].ValuablePresets[0].tall.Clear();
        __instance.levels[0].ValuablePresets[0].veryTall.Clear();
        
        __instance.levels[0].ValuablePresets[0].tiny.Add(hourglass);
        __instance.levels[0].ValuablePresets[0].small.Add(hourglass);
        __instance.levels[0].ValuablePresets[0].medium.Add(hourglass);
        __instance.levels[0].ValuablePresets[0].big.Add(hourglass);
        __instance.levels[0].ValuablePresets[0].wide.Add(hourglass);
        __instance.levels[0].ValuablePresets[0].tall.Add(hourglass);
        __instance.levels[0].ValuablePresets[0].veryTall.Add(hourglass);
    }
}
#endif