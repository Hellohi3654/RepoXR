using BepInEx.Configuration;

namespace RepoXR;

public class Config(string assemblyPath, ConfigFile file)
{
    public string AssemblyPath { get; } = assemblyPath;
    public ConfigFile File { get; } = file;

    // General configuration

    public ConfigEntry<bool> DisableVR { get; } = file.Bind("General", nameof(DisableVR), false,
        "Disabled the main functionality of this mod, can be used if you want to play without VR while keeping the mod installed.");

    public ConfigEntry<bool> EnableVerboseLogging { get; } = file.Bind("General", nameof(EnableVerboseLogging), false,
        "Enables verbose debug logging during OpenXR initialization");

    // Performance configuration

    public ConfigEntry<bool> EnableOcclusionMesh { get; } = file.Bind("Performance", nameof(EnableOcclusionMesh), true,
        "The occlusion mesh will make the game stop rendering pixels otuside of the lens' views, which increases performance.");

    // Rendering configuration

    public ConfigEntry<float> MirrorXOffset { get; } = file.Bind("Rendering", nameof(MirrorXOffset), 0f,
        new ConfigDescription(
            "The X offset that is added to the XR Mirror View shader. Do not touch if you don't know what this means.",
            new AcceptableValueRange<float>(-1, 1)));

    public ConfigEntry<float> MirrorYOffset { get; } = file.Bind("Rendering", nameof(MirrorYOffset), 0f,
        new ConfigDescription(
            "The Y offset that is added to the XR Mirror View shader. Do not touch if you don't know what this means.",
            new AcceptableValueRange<float>(-1, 1)));
    
    // Internal configuration

    public ConfigEntry<string> ControllerBindingsOverride { get; } = file.Bind("Internal",
        nameof(ControllerBindingsOverride), "", "FOR INTERNAL USE ONLY, DO NOT EDIT");

    public ConfigEntry<string> InputToggleBindings { get; } = file.Bind("Internal", nameof(InputToggleBindings), "",
        "FOR INTERNAL USE ONLY, DO NOT EDIT");

    public ConfigEntry<string> OpenXRRuntimeFile { get; } = file.Bind("Internal", nameof(OpenXRRuntimeFile), "",
        "FOR INTERNAL USE ONLY, DO NOT EDIT");
}