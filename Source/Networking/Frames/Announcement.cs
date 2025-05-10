using Photon.Pun;

namespace RepoXR.Networking.Frames;

/// <summary>
/// The announcement frame tells other client that the sender is a VR player
/// </summary>
[Frame(FrameHelper.FrameAnnouncement)]
public class Announcement : IFrame
{
    public void Serialize(PhotonStream _)
    {
    }

    public void Deserialize(PhotonStream _)
    {
    }
}