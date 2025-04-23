using System;
using BepInEx.Configuration;
using RepoXR.Assets;
using RepoXR.Player.Camera;
using UnityEngine;
using Object = UnityEngine.Object;

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
    
    // Input configuration

    public ConfigEntry<TurnProviderOption> TurnProvider { get; } = file.Bind("Input", nameof(TurnProvider),
        TurnProviderOption.Smooth,
        new ConfigDescription("Specify which turning provider your player uses, if any.",
            new AcceptableValueEnum<TurnProviderOption>()));

    public ConfigEntry<float> SmoothTurnSpeedModifier { get; } = file.Bind("Input", nameof(SmoothTurnSpeedModifier), 1f,
        new ConfigDescription(
            "A multiplier that is added to the smooth turning speed. Requires turn provider to be set to smooth.",
            new AcceptableValueRange<float>(0.25f, 5)));
    
    public ConfigEntry<float> SnapTurnSize { get; } = file.Bind("Input", nameof(SnapTurnSize), 45f,
        new ConfigDescription(
            "The amount of rotation that is applied when performing a snap turn. Requires turn provider to be set to snap.",
            new AcceptableValueRange<float>(10, 180)));

    // Rendering configuration

    public ConfigEntry<bool> EnableCustomCamera { get; } =
        file.Bind("Rendering", nameof(EnableCustomCamera), false,
            "Adds a second camera mounted on top of the VR camera that will render separately from the VR camera to the display. This requires extra GPU power!");

    public ConfigEntry<float> CustomCameraFOV { get; } = file.Bind("Rendering", nameof(CustomCameraFOV), 75f,
        new ConfigDescription("The field of view that the custom camera should have.",
            new AcceptableValueRange<float>(45, 120)));

    public ConfigEntry<float> CustomCameraSmoothing { get; } = file.Bind("Rendering", nameof(CustomCameraSmoothing),
        0.5f,
        new ConfigDescription("The amount of smoothing that is applied to the custom camera.",
            new AcceptableValueRange<float>(0, 1)));
    
    // Internal configuration

    public ConfigEntry<string> ControllerBindingsOverride { get; } = file.Bind("Internal",
        nameof(ControllerBindingsOverride), "", "FOR INTERNAL USE ONLY, DO NOT EDIT");

    public ConfigEntry<string> InputToggleBindings { get; } = file.Bind("Internal", nameof(InputToggleBindings), "",
        "FOR INTERNAL USE ONLY, DO NOT EDIT");

    public ConfigEntry<string> OpenXRRuntimeFile { get; } = file.Bind("Internal", nameof(OpenXRRuntimeFile), "",
        "FOR INTERNAL USE ONLY, DO NOT EDIT");

    /// <summary>
    /// Create persistent callbacks that persist for the entire duration of the application
    /// </summary>
    public void SetupGlobalCallbacks()
    {
        EnableCustomCamera.SettingChanged += (_, _) =>
        {
            if (EnableCustomCamera.Value)
                Object.Instantiate(AssetCollection.CustomCamera, Camera.main!.transform.parent);
            else
                Object.Destroy(VRCustomCamera.instance.gameObject);
        };
    }

    public enum TurnProviderOption
    {
        Snap,
        Smooth,
        Disabled
    }
}

internal class AcceptableValueEnum<T>() : AcceptableValueBase(typeof(T))
    where T: Enum
{
    private readonly string[] names = Enum.GetNames(typeof(T));

    public override object Clamp(object value) => value;
    public override bool IsValid(object value) => true;
    public override string ToDescriptionString() => $"# Acceptable values: {string.Join(", ", names)}";
}