using Photon.Pun;

namespace RepoXR.Networking.Frames;

[Frame(FrameHelper.FRAME_MAPTOOL)]
public class MapTool : IFrame
{
    public bool HideFlashlight;
    public bool LeftHanded;
    
    public void Serialize(PhotonStream stream)
    {
        stream.SendNext(HideFlashlight);
        stream.SendNext(LeftHanded);
    }

    public void Deserialize(PhotonStream stream)
    {
        HideFlashlight = (bool)stream.ReceiveNext();
        LeftHanded = (bool)stream.ReceiveNext();
    }
}