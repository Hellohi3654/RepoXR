using System;
using System.Collections.Generic;
using HarmonyLib;
using Photon.Pun;
using RepoXR.Networking.Frames;
using RepoXR.Patches;
using UnityEngine;

namespace RepoXR.Networking;

public class NetworkSystem : MonoBehaviour
{
    private const long REPOXR_MAGIC = 0x5245504F5852;
    private const int PROTOCOL_VERSION = 1;
    
    public static NetworkSystem instance;

    private List<IFrame> scheduledFrames = [];
    private Dictionary<int, NetworkPlayer> networkPlayers = [];
    private List<int> knownPhotonIds = [];
    
    private void Awake()
    {
        if (instance != null)
        {
            // On new scene load, clear the network players cache
            instance.networkPlayers.Clear();
            
            Destroy(gameObject);
            return;
        }
        
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public bool GetNetworkPlayer(PlayerAvatar player, out NetworkPlayer networkPlayer)
    {
        return networkPlayers.TryGetValue(player.photonView.ControllerActorNr, out networkPlayer);
    }

    public bool IsVRPlayer(PlayerAvatar player)
    {
        return knownPhotonIds.Contains(player.photonView.ControllerActorNr);
    }

    public bool IsVRView(PhotonView view)
    {
        return knownPhotonIds.Contains(view.ControllerActorNr);
    }

    // Sending
    
    public void AnnounceVRPlayer()
    {
        scheduledFrames.Add(new Announcement());
    }

    public void SendRigData(Vector3 leftPosition, Vector3 rightPosition, Quaternion leftRotation,
        Quaternion rightRotation)
    {
        EnqueueFrame(
            new Rig
            {
                LeftPosition = leftPosition, RightPosition = rightPosition, LeftRotation = leftRotation,
                RightRotation = rightRotation
            });
    }

    public void UpdateMapToolState(bool hideFlashlight, bool leftHanded)
    {
        EnqueueFrame(new MapTool
        {
            HideFlashlight = hideFlashlight,
            LeftHanded = leftHanded
        });
    }

    /// <summary>
    /// Enqueues a frame to be sent next serialization sequence. This function contains an optimization that removes
    /// duplicate frames to reduce network usage, which reduces server costs.
    /// </summary>
    private void EnqueueFrame<T>(T frame, bool removeDuplicate = true) where T : IFrame
    {
        if (removeDuplicate)
            scheduledFrames.ReplaceOrInsert(frame, f => f.GetType() == typeof(T));
        else
            scheduledFrames.Add(frame);
    }

    // Handling

    private void HandleFrame(PlayerAvatar player, IFrame frame)
    {
        try
        {
            if (FrameHelper.GetFrameID(frame) == FrameHelper.FRAME_ANNOUNCEMENT)
            {
                if (networkPlayers.ContainsKey(player.photonView.ControllerActorNr))
                    return;

                var networkPlayer =
                    new GameObject($"VR Player Rig - {player.playerName}").AddComponent<NetworkPlayer>();
                networkPlayer.playerAvatar = player;

                networkPlayers.Add(player.photonView.ControllerActorNr, networkPlayer);
                knownPhotonIds.Add(player.photonView.ControllerActorNr);
            }
            else if (FrameHelper.GetFrameID(frame) == FrameHelper.FRAME_RIG)
            {
                var rigFrame = (Rig)frame;

                if (!networkPlayers.TryGetValue(player.photonView.ControllerActorNr, out var networkPlayer))
                    return;

                networkPlayer.HandleRigFrame(rigFrame);
            }
            else if (FrameHelper.GetFrameID(frame) == FrameHelper.FRAME_MAPTOOL)
            {
                var mapFrame = (MapTool)frame;

                if (!networkPlayers.TryGetValue(player.photonView.controllerActorNr, out var networkPlayer))
                    return;

                networkPlayer.HandleMapFrame(mapFrame);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError($"Error while handling frame {FrameHelper.GetFrameID(frame)} ({frame.GetType().Name}): {ex.Message}");
            Logger.LogError(ex.StackTrace);
        }
    }

    // Internal stuff

    internal void ResetCache()
    {
        scheduledFrames.Clear();
        networkPlayers.Clear();
        knownPhotonIds.Clear();
    }

    internal void OnPlayerLeave(int actorNumber)
    {
        if (networkPlayers.Remove(actorNumber, out var networkPlayer))
            Destroy(networkPlayer.gameObject);
        
        knownPhotonIds.Remove(actorNumber);
    }
    
    internal void WriteAdditionalData(PhotonStream stream)
    {
        stream.SendNext(REPOXR_MAGIC);
        stream.SendNext(PROTOCOL_VERSION);

        stream.SendNext(scheduledFrames.Count);
        
        foreach (var frame in scheduledFrames)
        {
            stream.SendNext(FrameHelper.GetFrameID(frame));
            frame.Serialize(stream);
        }
        
        scheduledFrames.Clear();
    }

    internal void ReadAdditionalData(PlayerAvatar playerAvatar, PhotonStream stream)
    {
        try
        {
            if ((long)stream.PeekNext() != REPOXR_MAGIC)
                return;

            stream.ReceiveNext();

            if ((int)stream.ReceiveNext() != PROTOCOL_VERSION)
                return;
            
            var frames = (int)stream.ReceiveNext();
            
            for (var i = 0; i < frames; i++)
            {
                var frameId = (int)stream.ReceiveNext();
                var frame = FrameHelper.CreateFrame(frameId);
                
                frame.Deserialize(stream);
                HandleFrame(playerAvatar, frame);
            }
        }
        catch
        {
            // no-op
        }
    }
}

[RepoXRPatch(RepoXRPatchTarget.Universal)]
internal static class NetworkingPatches
{
    // The reason that this code is injected on PhysGrabber is that it's the last observed component on the
    // player avatar controller's PhotonView, meaning no additional data is available on the PhotonStream.
    // If we started injecting data too early, it would cause vanilla clients to no longer be able
    // to understand our photon data, and that breaks multiplayer.

    /// <summary>
    /// Inject additional code when serializing/deserializing a network component
    /// </summary>
    [HarmonyPatch(typeof(PhysGrabber), nameof(PhysGrabber.OnPhotonSerializeView))]
    [HarmonyPostfix]
    private static void OnAfterSerializeView(PhysGrabber __instance, PhotonStream stream)
    {
        if (stream.IsWriting)
            NetworkSystem.instance.WriteAdditionalData(stream);
        else
            NetworkSystem.instance.ReadAdditionalData(
                __instance.playerAvatar ?? __instance.GetComponent<PlayerAvatar>(), stream);
    }

    [HarmonyPatch(typeof(NetworkManager), nameof(NetworkManager.OnPlayerLeftRoom))]
    [HarmonyPostfix]
    private static void OnPlayerLeave(Photon.Realtime.Player otherPlayer)
    {
        NetworkSystem.instance.OnPlayerLeave(otherPlayer.ActorNumber);
    }

    /// <summary>
    /// When we enter the lobby, we clear the photon view cache
    /// </summary>
    [HarmonyPatch(typeof(MenuPageMain), nameof(MenuPageMain.Start))]
    [HarmonyPostfix]
    private static void OnMainMenuEntered()
    {
        NetworkSystem.instance.ResetCache();
    }
}