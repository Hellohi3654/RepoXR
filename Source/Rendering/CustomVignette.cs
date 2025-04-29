using System;
using RepoXR.Assets;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.Scripting;

namespace RepoXR.Rendering;

[PostProcess(typeof(CustomVignetteRenderer), PostProcessEvent.AfterStack, "Custom/Vignette")]
[Serializable]
public class CustomVignette : PostProcessEffectSettings
{
    public override bool IsEnabledAndSupported(PostProcessRenderContext context)
        => enabled.value && intensity.value > 0f;

    [Tooltip("Use the \"Classic\" mode for parametric controls. Use the \"Masked\" mode to use your own texture mask.")]
    public VignetteModeParameter mode = new()
    {
        value = VignetteMode.Classic
    };

    [Tooltip("Vignette color.")]
    public ColorParameter color = new()
    {
        value = new Color(0f, 0f, 0f, 1f)
    };

    [Range(0f, 1f)]
    [Tooltip("Amount of vignetting on screen.")]
    public FloatParameter intensity = new()
    {
        value = 0f
    };

    [Range(0.01f, 1f)]
    [Tooltip("Smoothness of the vignette borders.")]
    public FloatParameter smoothness = new()
    {
        value = 0.2f
    };

    [Range(0f, 1f)]
    [Tooltip("Lower values will make a square-ish vignette.")]
    public FloatParameter roundness = new()
    {
        value = 1f
    };

    [Tooltip("Set to true to mark the vignette to be perfectly round. False will make its shape dependent on the current aspect ratio.")]
    public BoolParameter rounded = new()
    {
        value = false
    };

    [Tooltip("A black and white mask to use as a vignette.")]
    public TextureParameter mask = new()
    {
        value = null
    };

    [Range(0f, 1f)]
    [Tooltip("Mask opacity.")]
    public FloatParameter opacity = new()
    {
        value = 1f
    };
}

[Preserve]
public class CustomVignetteRenderer : PostProcessEffectRenderer<CustomVignette>
{
    private static readonly int VignetteMode = Shader.PropertyToID("_Vignette_Mode");
    private static readonly int VignetteColor = Shader.PropertyToID("_Vignette_Color");
    private static readonly int VignetteSettings = Shader.PropertyToID("_Vignette_Settings");
    private static readonly int VignetteMask = Shader.PropertyToID("_Vignette_Mask");
    private static readonly int VignetteOpacity = Shader.PropertyToID("_Vignette_Opacity");
    private static readonly int VignetteActiveEye = Shader.PropertyToID("_Vignette_Active_Eye");

    public override void Render(PostProcessRenderContext context)
    {
        var sheet = context.propertySheets.Get(AssetCollection.VignetteShader);

        sheet.properties.SetColor(VignetteColor, settings.color.value);
        sheet.properties.SetFloat(VignetteActiveEye, context.xrActiveEye);

        if (settings.mode == UnityEngine.Rendering.PostProcessing.VignetteMode.Classic)
        {
            var num = (1f - settings.roundness.value) * 6f + settings.roundness.value;

            sheet.properties.SetFloat(VignetteMode, 0f);
            sheet.properties.SetVector(VignetteSettings,
                new Vector4(settings.intensity.value * 3f, settings.smoothness.value * 5f, num,
                    settings.rounded.value ? 1f : 0f));

            context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);

            return;
        }

        sheet.properties.SetFloat(VignetteMode, 1f);
        sheet.properties.SetTexture(VignetteMask, settings.mask.value);
        sheet.properties.SetFloat(VignetteOpacity, Mathf.Clamp01(settings.opacity.value));

        context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);
    }
}