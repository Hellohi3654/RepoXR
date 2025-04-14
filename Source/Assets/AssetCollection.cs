using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace RepoXR.Assets;

internal static class AssetCollection
{
    private static AssetBundle assetBundle;

    public static GameObject RemappableControls;
    public static GameObject RebindHeader;
    public static GameObject RebindButton;
    public static GameObject RebindButtonToggle;
    public static GameObject VRRig;
    public static GameObject Cube;
    public static GameObject Imgage;
    
    public static InputActionAsset DefaultXRActions;
    public static InputActionAsset VRInputs;

    public static Material DefaultLine;

    public static TMP_SpriteAsset TMPInputsSpriteAsset;

    public static bool LoadAssets()
    {
        assetBundle =
            AssetBundle.LoadFromFile(Path.Combine(Path.GetDirectoryName(Plugin.Config.AssemblyPath)!, "repoxrassets"));

        if (assetBundle == null)
        {
            Logger.LogError("Failed to load asset bundle!");
            return false;
        }

        RemappableControls = assetBundle.LoadAsset<GameObject>("RemappableControls");
        RebindHeader = assetBundle.LoadAsset<GameObject>("Rebind Header");
        RebindButton = assetBundle.LoadAsset<GameObject>("Rebind Button");
        RebindButtonToggle = assetBundle.LoadAsset<GameObject>("Rebind Button Toggle");
        VRRig = assetBundle.LoadAsset<GameObject>("VRRig");
        Cube = assetBundle.LoadAsset<GameObject>("Cube");
        Imgage = assetBundle.LoadAsset<GameObject>("Imgage");

        DefaultXRActions = assetBundle.LoadAsset<InputActionAsset>("DefaultXRActions");
        VRInputs = assetBundle.LoadAsset<InputActionAsset>("VRInputs");
        
        DefaultLine = assetBundle.LoadAsset<Material>("Default-Line");

        TMPInputsSpriteAsset = assetBundle.LoadAsset<TMP_SpriteAsset>("TMPInputsSpriteAsset");
        
        return true;
    }
}