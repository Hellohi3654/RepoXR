using RepoXR.Networking.Frames;
using UnityEngine;

namespace RepoXR.Networking;

[DefaultExecutionOrder(100)]
public class NetworkPlayer : MonoBehaviour
{
    internal PlayerAvatar playerAvatar;
    
    private PlayerAvatarVisuals playerAvatarVisuals;
    private PlayerAvatarLeftArm playerLeftArm;
    private PlayerAvatarRightArm playerRightArm;

    private FlashlightController flashlight;
    private MapToolController mapTool;
    
    private Transform rigContainer;
    private Transform leftHandTarget;
    private Transform rightHandTarget;
    
    private Transform leftHandAnchor;
    private Transform rightHandAnchor;

    private Transform headlampTransform;
    
    private Vector3 leftHandPosition;
    private Vector3 rightHandPosition;

    private Quaternion leftHandRotation;
    private Quaternion rightHandRotation;

    public Transform PrimaryHand => isLeftHanded ? leftHandTarget : rightHandTarget;

    private bool isLeftHanded;
    
    private void Start()
    {
        playerAvatarVisuals = playerAvatar.playerAvatarVisuals;
        playerLeftArm = playerAvatarVisuals.GetComponent<PlayerAvatarLeftArm>();
        playerRightArm = playerAvatarVisuals.GetComponent<PlayerAvatarRightArm>();

        rigContainer = new GameObject("VR Player Rig Container")
            {
                transform =
                {
                    parent = transform, localPosition = Vector3.zero,
                    localRotation = Quaternion.identity
                }
            }
            .transform;
        leftHandTarget = new GameObject("Left Hand") { transform = { parent = rigContainer } }.transform;
        rightHandTarget = new GameObject("Right Hand") { transform = { parent = rigContainer } }.transform;

        leftHandAnchor = new GameObject("Left Hand Anchor")
                { transform = { parent = playerLeftArm.leftArmTransform, localPosition = Vector3.forward * 0.513f } }
            .transform;
        rightHandAnchor = new GameObject("Right Hand Anchor")
            {
                transform =
                {
                    // ANIM ARM R SCALE is the one that scales, not rightArmTransform
                    parent = playerRightArm.rightArmTransform.Find("ANIM ARM R SCALE"),
                    localPosition = Vector3.forward * 0.513f
                }
            }
            .transform;

        // Headlamp

        headlampTransform = new GameObject("Headlamp Anchor")
            {
                transform =
                {
                    parent = playerAvatarVisuals.attachPointTopHeadMiddle, 
                    localPosition = new Vector3(-0.21f, 0.1f, 0)
                }
            }
            .transform;
        
        // Re-parent tools and grabber

        var playerRoot = playerAvatar.transform.parent;
        
        flashlight = playerRoot.GetComponentInChildren<FlashlightController>(true);
        flashlight.transform.parent = leftHandAnchor;
        flashlight.transform.localScale = Vector3.one * flashlight.hiddenScale;
        flashlight.transform.localPosition = Vector3.zero;
        flashlight.transform.localRotation = Quaternion.identity;

        mapTool = playerRoot.GetComponentInChildren<MapToolController>(true);
        mapTool.transform.parent.parent = rightHandAnchor;
        mapTool.transform.parent.localPosition = Vector3.zero;
        mapTool.transform.parent.localRotation = Quaternion.identity;

        playerRightArm.grabberClawParent.SetParent(rightHandAnchor);
        playerRightArm.grabberClawParent.localPosition = Vector3.zero;

        playerRightArm.physGrabBeam.PhysGrabPointOrigin.SetParent(rightHandAnchor);
        playerRightArm.physGrabBeam.PhysGrabPointOrigin.localPosition = Vector3.zero;
    }

    private void Update()
    {
        transform.position = playerAvatarVisuals.transform.position;

        leftHandTarget.position =
            Vector3.Lerp(leftHandTarget.position, leftHandPosition, 15 * Time.deltaTime);
        leftHandTarget.rotation =
            Quaternion.Slerp(leftHandTarget.rotation, leftHandRotation, 15 * Time.deltaTime);

        rightHandTarget.position =
            Vector3.Lerp(rightHandTarget.position, rightHandPosition, 15 * Time.deltaTime);
        rightHandTarget.rotation =
            Quaternion.Slerp(rightHandTarget.rotation, rightHandRotation, 15 * Time.deltaTime);

        if (!playerAvatar.isTumbling)
        {
            playerRightArm.rightArmTransform.LookAt(rightHandTarget.position);
            playerLeftArm.leftArmTransform.LookAt(leftHandTarget.position);
        }

        leftHandAnchor.rotation = leftHandTarget.rotation;
        rightHandAnchor.rotation = rightHandTarget.rotation;

        // Counteract any scaling effects on the arms
        leftHandAnchor.localScale =
            new Vector3(
                leftHandAnchor.parent.localScale.x != 0 ? 1 / leftHandAnchor.parent.localScale.x : 0,
                leftHandAnchor.parent.localScale.y != 0 ? 1 / leftHandAnchor.parent.localScale.y : 0,
                leftHandAnchor.parent.localScale.z != 0 ? 1 / leftHandAnchor.parent.localScale.z : 0
            );
        rightHandAnchor.localScale =
            new Vector3(
                rightHandAnchor.parent.localScale.x != 0 ? 1 / rightHandAnchor.parent.localScale.x : 0,
                rightHandAnchor.parent.localScale.y != 0 ? 1 / rightHandAnchor.parent.localScale.y : 0,
                rightHandAnchor.parent.localScale.z != 0 ? 1 / rightHandAnchor.parent.localScale.z : 0
            );
    }

    public void HandleRigFrame(Rig rigFrame)
    {
        leftHandPosition = rigFrame.LeftPosition;
        leftHandRotation = rigFrame.LeftRotation;

        rightHandPosition = rigFrame.RightPosition;
        rightHandRotation = rigFrame.RightRotation;
    }

    public void HandleMapFrame(MapTool mapFrame)
    {
        flashlight.hideFlashlight = mapFrame.HideFlashlight;

        mapTool.transform.parent.parent = mapFrame.LeftHanded ? leftHandAnchor : rightHandAnchor;
        mapTool.transform.parent.localPosition = Vector3.zero;
        mapTool.transform.parent.localRotation = Quaternion.identity;
    }

    public void HandleHeadlamp(bool headLampEnabled)
    {
        flashlight.transform.SetParent(headLampEnabled ? headlampTransform : leftHandAnchor);
        flashlight.transform.localPosition = Vector3.zero;
        flashlight.transform.localRotation = Quaternion.identity;
    }

    public void UpdateDominantHand(bool leftHanded)
    {
        isLeftHanded = leftHanded;

        flashlight.transform.parent = isLeftHanded ? rightHandAnchor : leftHandAnchor;
        flashlight.transform.localScale = Vector3.one * flashlight.hiddenScale;
        flashlight.transform.localPosition = Vector3.zero;
        flashlight.transform.localRotation = Quaternion.identity;

        headlampTransform.transform.localPosition = new Vector3(isLeftHanded ? 0.21f : -0.21f, 0.1f, 0);

        playerRightArm.grabberClawParent.SetParent(isLeftHanded ? leftHandAnchor : rightHandAnchor);
        playerRightArm.grabberClawParent.localPosition = Vector3.zero;

        playerRightArm.physGrabBeam.PhysGrabPointOrigin.SetParent(isLeftHanded ? leftHandAnchor : rightHandAnchor);
        playerRightArm.physGrabBeam.PhysGrabPointOrigin.localPosition = Vector3.zero;
    }
}