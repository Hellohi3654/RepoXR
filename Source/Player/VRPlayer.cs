using System;
using System.Collections;
using RepoXR.Assets;
using RepoXR.Input;
using RepoXR.Managers;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
using Vector3 = UnityEngine.Vector3;

namespace RepoXR.Player;

public class VRPlayer : MonoBehaviour
{
    // Camera stuff
    private VRCameraPosition cameraPosition;
    private VRCameraAim cameraAim;  
    
    private Transform mainCamera;
    private Transform leftHand;
    private Transform rightHand;

    private PlayerController localController;
    private FirstPersonVRRig localRig;
    
    public Transform MainHand => localRig.rightHandTip;

    private bool turnedLastInput;
    private Vector3 lastPosition;
    
    private void Awake()
    {
        cameraPosition = VRCameraPosition.Instance;
        cameraAim = VRCameraAim.Instance;

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

        Actions.Instance["ResetHeight"].performed += OnResetHeight;
    }

    private void OnDestroy()
    {
        Actions.Instance["ResetHeight"].performed -= OnResetHeight;
    }

    private IEnumerator Start()
    {
        yield return null;
        
        ResetHeight();
    }

    private void FixedUpdate()
    {
        HandleMovement();
    }

    private void Update()
    {
        // HandleMovement();
        HandleTurning();
    }

    public void SetRigVisible(bool visible)
    {
        localRig.SetVisible(visible);
    }
    
    public void SetColor(int colorIndex, Color color = default)
    {
        var customColor = colorIndex == -1;
        if (!customColor)
            color = AssetManager.instance.playerColors[colorIndex];

        localRig.SetColor(color);
    }

    public void SetHurtColor(Color color)
    {
        localRig.SetHurtColor(color);
    }
    
    public void SetHurtAmount(float amount)
    {
        localRig.SetHurtAmount(amount);
    }

    private void ResetHeight()
    {
        const float targetHeight = 1.5f;

        // The playerOffset field is actually smoothed, making the reset height sequence look a little bit nicer
        cameraPosition.original.playerOffset = new Vector3(0, targetHeight - mainCamera.transform.localPosition.y, 0);
    }

    private void HandleMovement()
    {
        // No tracking data yet
        // TODO: Can probably be removed
        if (mainCamera.transform.localPosition == Vector3.zero)
            return;

        // If this is our first frame with position data, just reset the position immediately
        if (lastPosition == Vector3.zero)
            lastPosition = mainCamera.transform.localPosition;

        var headPosition = mainCamera.transform.localPosition;
        var movement = new Vector3(headPosition.x - lastPosition.x, 0, headPosition.z - lastPosition.z);

        cameraPosition.additionalOffset = -(mainCamera.transform.parent.rotation *
                                          new Vector3(mainCamera.localPosition.x, 0,
                                              mainCamera.localPosition.z)); // Will make the game run like 3-DoF
        PlayerController.instance.rb.MovePosition(PlayerController.instance.rb.transform.position +
                                                  mainCamera.transform.parent.rotation * movement);

        lastPosition = headPosition;
    }

    private void HandleTurning()
    {
        var value = Actions.Instance["Turn"].ReadValue<float>();

        switch (Plugin.Config.TurnProvider.Value)
        {
            case Config.TurnProviderOption.Snap:
                var should = MathF.Abs(value) > 0.75f;
                
                if (!turnedLastInput && should)
                    if (value > 0)
                        cameraAim.TurnAimNow(Plugin.Config.SnapTurnSize.Value);
                    else
                        cameraAim.TurnAimNow(-Plugin.Config.SnapTurnSize.Value);

                turnedLastInput = should;
                
                break;
            
            case Config.TurnProviderOption.Smooth:
                cameraAim.TurnAimNow(180 * Time.deltaTime * Plugin.Config.SmoothTurnSpeedModifier.Value * value);
                break;
            
            case Config.TurnProviderOption.Disabled:
            default:
                break;
        }
    }

    private void OnResetHeight(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed)
            return;
        
        ResetHeight();
    }
}
