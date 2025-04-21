using System.Collections;
using System.Linq;
using RepoXR.Input;
using RepoXR.Player.Camera;
using Unity.XR.CoreUtils;
using UnityEngine;

namespace RepoXR.Player;

public class FirstPersonVRRig : MonoBehaviour
{
    private static readonly int AlbedoColor = Shader.PropertyToID("_AlbedoColor");
    private static readonly int HurtColor = Shader.PropertyToID("_ColorOverlayAmount");
    private static readonly int HurtAmount = Shader.PropertyToID("_ColorOverlayAmount");
    
    public MeshRenderer[] meshes;

    public Transform head;
    public Transform leftArm;
    public Transform rightArm;
    public Transform leftArmTarget;
    public Transform rightArmTarget;
    public Transform leftHandAnchor;
    public Transform rightHandAnchor;
    public Transform leftHandTip;
    public Transform rightHandTip;
    
    public Transform inventory;
    public Transform map;

    public Collider leftHandCollider;
    public Collider rightHandCollider;
    public Collider mapPickupCollider;

    public VRInventory inventoryController;

    public Vector3 headOffset;
    
    private Transform leftArmMesh;
    private Transform rightArmMesh;

    private PlayerAvatar playerAvatar;
    private PlayerAvatarVisuals playerAvatarVisuals;
    private PlayerAvatarRightArm playerAvatarRightArm;
    
    // Map tool stuff

    private MapToolController mapTool;
    private bool mapHeldLeftHand;
    private bool mapHeld;
    
    private void Awake()
    {
        leftArmMesh = leftArm.GetComponentInChildren<MeshRenderer>().transform;
        rightArmMesh = rightArm.GetComponentInChildren<MeshRenderer>().transform;
    }

    private IEnumerator Start()
    {
        playerAvatar = PlayerController.instance.playerAvatarScript;
        playerAvatarVisuals = playerAvatar.playerAvatarVisuals;
        playerAvatarRightArm = playerAvatarVisuals.GetComponentInChildren<PlayerAvatarRightArm>(true);
        
        // Parent claw to right hand
        playerAvatarRightArm.grabberClawParent.SetParent(rightHandTip);
        playerAvatarRightArm.grabberClawParent.localPosition = Vector3.zero;
        playerAvatarRightArm.grabberClawParent.gameObject.SetLayerRecursively(6);

        // Everything else is only available after the first frame
        yield return null;
        
        // Parent flashlight to left hand
        var flashlight = FlashlightController.Instance;
        
        flashlight.transform.parent = leftHandTip;
        flashlight.transform.localPosition = Vector3.zero;
        flashlight.transform.localRotation = Quaternion.identity;
        
        // Map tool
        mapTool = FindObjectsOfType<MapToolController>().First(tool => tool.PlayerAvatar.isLocal);
    }

    private void LateUpdate()
    {
        transform.position = head.position + headOffset;
        
        // TODO: Maybe use Player rotation? This might be just fine though.
        transform.rotation = Quaternion.Lerp(transform.rotation,
            Quaternion.Euler(transform.eulerAngles.x, head.eulerAngles.y, transform.eulerAngles.z),
            10 * Time.deltaTime);
        
        UpdateArms();
        UpdateClaw();
        MapToolLogic();
    }

    private void UpdateArms()
    {
        leftArm.localPosition = new Vector3(leftArm.localPosition.x, leftArm.localPosition.y, 0);
        rightArm.localPosition = new Vector3(rightArm.localPosition.x, rightArm.localPosition.y, 0);
            
        leftArm.LookAt(leftArmTarget.position);
        rightArm.LookAt(rightArmTarget.position);

        // I KNOW THAT THIS IS NOT *THE* WAY TO DO THIS, THEY DON'T CALL IT *INVERSE* KINEMATICS FOR NOTHING, AND I AM DOING QUITE THE OPPOSITE
        var maxDistanceLeft = leftHandAnchor.localPosition.z;
        var maxDistanceRight = rightHandAnchor.localPosition.z;

        if (Vector3.Distance(leftArm.position, leftArmTarget.position) is var leftDistance &&
            leftDistance < maxDistanceLeft)
        {
            leftArm.localPosition += Vector3.back * (maxDistanceLeft - leftDistance);
            leftArm.LookAt(leftArmTarget.position);
        }

        if (Vector3.Distance(rightArm.position, rightArmTarget.position) is var rightDistance &&
            rightDistance < maxDistanceRight)
        {
            rightArm.localPosition += Vector3.back * (maxDistanceRight - rightDistance);
            rightArm.LookAt(rightArmTarget.position);
        }
        
        leftArmMesh.localEulerAngles = Vector3.up * 90;
        rightArmMesh.localEulerAngles = Vector3.down * 90;
        
        leftArmMesh.Rotate(Vector3.right, leftArmTarget.localEulerAngles.z);
        rightArmMesh.Rotate(Vector3.right, rightArmTarget.localEulerAngles.z);

        leftHandTip.rotation = leftArmTarget.rotation;
        rightHandTip.rotation = rightArmTarget.rotation;
    }

    private void UpdateClaw()
    {
        playerAvatarRightArm.deltaTime = playerAvatarVisuals.deltaTime;
        playerAvatarRightArm.GrabberLogic();
    }

    // TODO: Move to VRMapTool.cs
    // TODO: Wait actually maybe not
    private void MapToolLogic()
    {
        if (!mapTool)
            return;

        mapTool.transform.parent.localPosition =
            Vector3.Lerp(mapTool.transform.parent.localPosition, Vector3.zero, 5 * Time.deltaTime);
        mapTool.transform.parent.localRotation = Quaternion.Slerp(mapTool.transform.parent.localRotation,
            Quaternion.identity, 5 * Time.deltaTime);

        // If the map tool was disabled for any reason, reparent back to hotbar
        if (!mapTool.Active && mapHeld)
        {
            mapHeld = false;
            mapHeldLeftHand = false;
            mapTool.transform.parent.parent = map;
            playerAvatar.physGrabber.enabled = true;
        }

        mapHeld = mapTool.Active;

        // Check for states that don't allow the map to be used
        if (playerAvatar.isDisabled || playerAvatar.isTumbling || VRCameraAim.instance.IsActive || SemiFunc.MenuLevel())
        {
            mapTool.Active = false;
            return;
        }

        // Right hand pickup logic
        if (!mapTool.Active && Actions.Instance["MapGrabRight"].WasPressedThisFrame() &&
            Utils.Collide(rightHandCollider, mapPickupCollider) && !PlayerController.instance.sprinting)
            if (mapTool.HideLerp >= 1)
            {
                mapTool.transform.parent.parent = rightHandTip;
                mapTool.Active = true;
                
                // Prevent picking up items while the map is opened
                playerAvatar.physGrabber.ReleaseObject();
                playerAvatar.physGrabber.enabled = false;
            }

        // Left hand touch logic (before picking up)
        if (!mapTool.Active && Utils.Collide(leftHandCollider, mapPickupCollider) &&
            !PlayerController.instance.sprinting)
            FlashlightController.Instance.hideFlashlight = true;
        else if (!mapTool.Active)
            FlashlightController.Instance.hideFlashlight = false;

        // Left hand pickup logic
        if (!mapTool.Active && Actions.Instance["MapGrabLeft"].WasPressedThisFrame() &&
            Utils.Collide(leftHandCollider, mapPickupCollider) && !PlayerController.instance.sprinting)
            if (mapTool.HideLerp >= 1)
            {
                mapTool.transform.parent.parent = leftHandTip;
                mapHeldLeftHand = true;
                FlashlightController.Instance.hideFlashlight = true;
                mapTool.Active = true;
            }
        
        // Disable map when sprinting
        if (PlayerController.instance.sprinting)
            mapTool.Active = false;

        // Right hand "let-go" logic
        if (mapTool.Active && !Actions.Instance["MapGrabRight"].IsPressed() && !mapHeldLeftHand && mapTool.HideLerp <= 0)
            mapTool.Active = false;

        // Left hand "let-go" logic
        if (mapTool.Active && !Actions.Instance["MapGrabLeft"].IsPressed() && mapHeldLeftHand && mapTool.HideLerp <= 0)
            mapTool.Active = false;
    }
    
    public void SetVisible(bool visible)
    {
        foreach (var mesh in meshes)
            mesh.enabled = visible;

        map.gameObject.SetActive(visible);
        inventory.gameObject.SetActive(visible);
    }
    
    public void SetColor(Color color)
    {
        foreach (var mesh in meshes)
            mesh.sharedMaterial.SetColor(AlbedoColor, color);
    }

    public void SetHurtColor(Color color)
    {
        foreach (var mesh in meshes)
            mesh.sharedMaterial.SetColor(HurtColor, color);
    }

    public void SetHurtAmount(float amount)
    {
        foreach (var mesh in meshes)
            mesh.sharedMaterial.SetFloat(HurtAmount, amount);
    }
}