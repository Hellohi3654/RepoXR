using System.Collections;
using RepoXR.Assets;
using RepoXR.Input;
using RepoXR.Managers;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;

namespace RepoXR.Player;

public class VRPlayer : MonoBehaviour
{
    // Camera stuff
    private CameraPosition cameraPosition;
    private CameraAim cameraAim;
    
    private Transform mainCamera;
    private Transform leftHand;
    private Transform rightHand;

    private PlayerController localController;
    private FirstPersonVRRig localRig;

    public Transform MainHand => localRig.rightHandTip;
    
    private void Awake()
    {
        cameraPosition = CameraPosition.instance;
        cameraAim = CameraAim.Instance;

        mainCamera = VRSession.Instance.MainCamera.transform;
        
        localController = PlayerController.instance;

        // This will make the player rotate towards where we are looking
        localController.cameraGameObject = mainCamera.gameObject;
        localController.cameraGameObjectLocal = mainCamera.gameObject;
        
        // Set up hands and stuff
        localRig = Instantiate(AssetCollection.VRRig).GetComponent<FirstPersonVRRig>();
        
        leftHand = new GameObject("Left Hand").transform;
        rightHand = new GameObject("Right Hand").transform;

        leftHand.transform.parent = rightHand.transform.parent = mainCamera.transform.parent;
        
        var leftHandTracker = leftHand.gameObject.AddComponent<TrackedPoseDriver>();
        var rightHandTracker = rightHand.gameObject.AddComponent<TrackedPoseDriver>();

        leftHandTracker.positionAction = Actions.Instance.LeftHandPosition;
        leftHandTracker.rotationAction = Actions.Instance.LeftHandRotation;
        leftHandTracker.trackingStateInput = new InputActionProperty(Actions.Instance.LeftHandTrackingState);

        rightHandTracker.positionAction = Actions.Instance.RightHandPosition;
        rightHandTracker.rotationAction = Actions.Instance.RightHandRotation;
        rightHandTracker.trackingStateInput = new InputActionProperty(Actions.Instance.RightHandTrackingState);
        
        localRig.head = mainCamera;
        localRig.leftArmTarget = leftHand;
        localRig.rightArmTarget = rightHand;

        // TODO: Move this to Start() coroutine with a 1 frame delay
        ResetHeight();
    }
    
    // TODO: There's a chance that CameraAim overrides are gonna be put in their own class

    private void Update()
    {
        UpdateCameraAim();
    }

    public void SetColor(int colorIndex, Color color = default)
    {
        var customColor = colorIndex == -1;
        if (!customColor)
            color = AssetManager.instance.playerColors[colorIndex];

        localRig.SetColor(color);
    }

    public void SetHurtAmount(float amount)
    {
        localRig.SetHurtAmount(amount);
    }

    private void UpdateCameraAim()
    {
        cameraAim.aimVertical = mainCamera.localEulerAngles.x;
        cameraAim.aimHorizontal = mainCamera.localEulerAngles.y;
        cameraAim.playerAim = mainCamera.localRotation;
    }

    private void ResetHeight()
    {
        StartCoroutine(ResetHeightRoutine());
    }

    private IEnumerator ResetHeightRoutine()
    {
        const float targetHeight = 1.5f;

        yield return new WaitForSeconds(0.1f);

        var currentHeight = cameraPosition.playerTransform.InverseTransformPoint(mainCamera.transform.position).y -
                            cameraPosition.playerOffset.y;
        cameraPosition.playerOffset = new Vector3(0, targetHeight - currentHeight, 0);
    }
}
