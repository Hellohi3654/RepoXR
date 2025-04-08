using RepoXR.Input;
using RepoXR.Player;
using RepoXR.UI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.InputSystem.XR;
using UnityEngine.XR;

namespace RepoXR.Managers;

public class VRSession : MonoBehaviour
{
    public static VRSession Instance { get; private set; }

    /// <summary>
    /// Whether the game has VR enabled. THis field will only be populated after RepoXR has loaded.
    /// </summary>
    public static bool InVR => Plugin.Flags.HasFlag(Flags.VR);

    public Camera MainCamera { get; private set; }
    public VRPlayer Player { get; private set; }
    public GameHud HUD { get; private set; }
    
    private void Awake()
    {
        Instance = this;
        
        if (InVR)
            InitializeVRSession();
    }

    private void InitializeVRSession()
    {
        // Disable base UI input system
        var input = GameObject.Find("EventSystem")?.GetComponent<InputSystemUIInputModule>();
        if (input != null)
            input.enabled = false;

        MainCamera = CameraUtils.Instance.MainCamera;

        // Setup camera tracking
        var cameraPoseDriver = MainCamera.gameObject.AddComponent<TrackedPoseDriver>();
        cameraPoseDriver.positionAction = Actions.Instance.HeadPosition;
        cameraPoseDriver.rotationAction = Actions.Instance.HeadRotation;
        cameraPoseDriver.trackingStateInput = new InputActionProperty(Actions.Instance.HeadTrackingState);
        
        // Initialize VR Player
        Player = PlayerController.instance.gameObject.AddComponent<VRPlayer>();
        
        // Initialize VR HUD
        HUD = global::HUD.instance.gameObject.AddComponent<GameHud>();
    }

    public static void VibrateController(XRNode hand, float duration, float amplitude)
    {
        var device = InputDevices.GetDeviceAtXRNode(hand);

        if (device.isValid && device.TryGetHapticCapabilities(out var capabilities) && capabilities.supportsImpulse)
            device.SendHapticImpulse(0, amplitude, duration);
    }
}