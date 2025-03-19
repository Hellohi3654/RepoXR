using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using RepoXR.Assets;
using Steamworks;
using TMPro;

namespace RepoXR;

internal static class Utils
{
    public static byte[] ComputeHash(byte[] input)
    {
        using var sha = SHA256.Create();

        return sha.ComputeHash(input);
    }

    public static string[] ParseConfig(string content)
    {
        var lines = content.Split("\n", StringSplitOptions.RemoveEmptyEntries);

        return (from line in lines
            where !line.TrimStart().StartsWith("#")
            let commentIndex = line.IndexOf('#')
            select commentIndex >= 0 ? line[..commentIndex].Trim() : line.Trim()
            into parsedLine
            where !string.IsNullOrEmpty(parsedLine)
            select parsedLine).ToArray();
    }
    
    public static void EnableSprites(this TextMeshProUGUI text)
    {
        text.spriteAsset = AssetCollection.TMPInputsSpriteAsset;
    }

    public static string GetControlSpriteString(string controlPath)
    {
        const string unknown = """<sprite name="unknown">""";
        
        if (string.IsNullOrEmpty(controlPath))
            return unknown;

        var path = Regex.Replace(controlPath.ToLowerInvariant(), @"<[^>]+>([^ ]+)", "$1");
        var hand = path.Split('/')[0].TrimStart('{').TrimEnd('}');
        controlPath = Regex.Replace(string.Join("/", path.Split('/').Skip(1)), @"{(.*)}", "$1");

        var id = (hand, controlPath) switch
        {
            ("lefthand", "primary2daxis" or "thumbstick") => "leftStick",
            ("lefthand", "primary2daxisclick" or "thumbstickclicked") => "leftStickClick",
            ("lefthand", "primary2daxis/up" or "thumbstick/up") => "leftStickUp",
            ("lefthand", "primary2daxis/down" or "thumbstick/down") => "leftStickDown",
            ("lefthand", "primary2daxis/left" or "thumbstick/left") => "leftStickLeft",
            ("lefthand", "primary2daxis/right" or "thumbstick/right") => "leftStickRight",
            ("lefthand", "primarybutton" or "primarypressed") => "leftPrimaryButton",
            ("lefthand", "secondarybutton" or "secondarypressed") => "leftSecondaryButton",
            ("lefthand", "triggerbutton" or "trigger" or "triggerpressed") => "leftTrigger",
            ("lefthand", "gripbutton" or "grip" or "grippressed") => "leftGrip",

            ("righthand", "primary2daxis" or "thumbstick") => "rightStick",
            ("righthand", "primary2daxisclick" or "thumbstickclicked") => "rightStickClick",
            ("righthand", "primary2daxis/up" or "thumbstick/up") => "rightStickUp",
            ("righthand", "primary2daxis/down" or "thumbstick/down") => "rightStickDown",
            ("righthand", "primary2daxis/left" or "thumbstick/left") => "rightStickLeft",
            ("righthand", "primary2daxis/right" or "thumbstick/right") => "rightStickRight",
            ("righthand", "primarybutton" or "primarypressed") => "rightPrimaryButton",
            ("righthand", "secondarybutton" or "secondarypressed") => "rightSecondaryButton",
            ("righthand", "triggerbutton" or "trigger" or "triggerpressed") => "rightTrigger",
            ("righthand", "gripbutton" or "grip" or "grippressed") => "rightGrip",

            (_, "menu" or "menubutton" or "menupressed") => "menuButton",

            _ => "unknown"
        };

        return $"""<sprite name="{id}">""";
    }

    public static T ExecuteWithSteamAPI<T>(Func<T> func)
    {
        var isValid = SteamClient.IsValid;
        
        if (!isValid)
            SteamClient.Init(3241660, false);

        var result = func();
        
        if (!isValid)
            SteamClient.Shutdown();

        return result;
    }
}