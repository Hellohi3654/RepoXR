using BepInEx.Bootstrap;

namespace RepoXR;

public static class Compat
{
    public static bool IsLoaded(string modId)
    {
        return Chainloader.PluginInfos.ContainsKey(modId);
    }
}