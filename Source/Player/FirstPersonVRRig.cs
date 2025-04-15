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

    public Vector3 headOffset;
    
    [SerializeField] protected LineRenderer leftHandLine;
    [SerializeField] protected LineRenderer rightHandLine;
    
    private Transform leftArmMesh;
    private Transform rightArmMesh;

    private PlayerAvatar playerAvatar;
    private PlayerAvatarVisuals playerAvatarVisuals;
    private PlayerAvatarRightArm playerAvatarRightArm;
    
    private void Awake()
    {
        leftArmMesh = leftArm.GetComponentInChildren<MeshRenderer>().transform;
        rightArmMesh = rightArm.GetComponentInChildren<MeshRenderer>().transform;
    }

    private void Start()
    {
        playerAvatar = PlayerController.instance.playerAvatarScript;
        playerAvatarVisuals = playerAvatar.playerAvatarVisuals;
        playerAvatarRightArm = playerAvatarVisuals.GetComponentInChildren<PlayerAvatarRightArm>(true);
        
        playerAvatarRightArm.grabberClawParent.SetParent(rightHandTip);
        playerAvatarRightArm.grabberClawParent.localPosition = Vector3.zero;
        playerAvatarRightArm.grabberClawParent.gameObject.SetLayerRecursively(6);
    }

    private void LateUpdate()
    {
        transform.position = head.position + headOffset;
        
        // TODO: Maybe use Player rotation? This might be just fine though.
        // TODO: Will need to disable (or at least hide) the rig when player dies and in spectator
        transform.rotation = Quaternion.Lerp(transform.rotation,
            Quaternion.Euler(transform.eulerAngles.x, head.eulerAngles.y, transform.eulerAngles.z),
            10 * Time.deltaTime);
        
        UpdateArms();
        UpdateClaw();
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
        
        leftHandLine.SetPositions([leftHandTip.position, leftHandTip.position + leftHandTip.forward * 5]);
        rightHandLine.SetPositions([rightHandTip.position, rightHandTip.position + rightHandTip.forward * 5]);
    }

    private void UpdateClaw()
    {
        playerAvatarRightArm.deltaTime = playerAvatarVisuals.deltaTime;
        playerAvatarRightArm.GrabberLogic();
    }

    public void SetVisible(bool visible)
    {
        foreach (var mesh in meshes)
            mesh.enabled = visible;

        leftHandLine.enabled = visible;
        rightHandLine.enabled = visible;
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