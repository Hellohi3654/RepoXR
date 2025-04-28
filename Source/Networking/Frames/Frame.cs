using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Photon.Pun;

namespace RepoXR.Networking.Frames;

public static class FrameHelper
{
    public const int FRAME_ANNOUNCEMENT = 1;
    public const int FRAME_RIG = 2;
    public const int FRAME_MAPTOOL = 3;
    
    private static Dictionary<int, Type> cachedTypes = [];

    static FrameHelper()
    {
        Assembly.GetExecutingAssembly().GetTypes().Do(type =>
        {
            if (type.GetCustomAttribute<FrameAttribute>() is not { } frame)
                return;
            
            cachedTypes.Add(frame.FrameID, type);
        });
    }

    public static Type GetFrameType(int frameId)
    {
        return cachedTypes[frameId];
    }

    public static IFrame CreateFrame(int frameId)
    {
        return (IFrame)Activator.CreateInstance(GetFrameType(frameId));
    }

    public static int GetFrameID(IFrame frame)
    {
        return cachedTypes.First(types => types.Value == frame.GetType()).Key;
    }
}

public interface IFrame
{
    public void Serialize(PhotonStream stream);
    public void Deserialize(PhotonStream stream);
}

[AttributeUsage(AttributeTargets.Class)]
public class FrameAttribute(int frameId) : Attribute
{
    public int FrameID => frameId;
}