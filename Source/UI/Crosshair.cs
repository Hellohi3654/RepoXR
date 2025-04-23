using System.Collections;
using RepoXR.Managers;
using UnityEngine;

namespace RepoXR.UI;

public class Crosshair : MonoBehaviour
{
    private const int LayerMask = 1 << 0 | 1 << 9 | 1 << 10 | 1 << 16 | 1 << 20 | 1 << 23;
    
    private Transform handTransform;
    private Transform camera;
    
    private IEnumerator Start()
    {
        yield return null;

        if (VRSession.Instance is not { } session)
            yield break;

        handTransform = session.Player.MainHand;
        camera = Camera.main!.transform;
    }

    private void Update()
    {
        if (!handTransform)
            return;

        if (!Physics.Raycast(new Ray(handTransform.position, handTransform.forward), out var hit, 10, LayerMask))
        {
            transform.position = Vector3.down * 3000;
            return;
        }

        var upness = Mathf.Abs(Mathf.Max(0, Vector3.Dot(hit.normal, Vector3.up) - 0.5f)) / 0.5f;
        var toCamera = camera.position - hit.point;
        var projectedToCamera = Vector3.ProjectOnPlane(toCamera, hit.normal).normalized;
        var forward = Quaternion.AngleAxis(90, hit.normal) * projectedToCamera;
        var calculatedRotation = Quaternion.LookRotation(forward, hit.normal);
        var finalRotation =
            Quaternion.Lerp(Quaternion.Euler(0, calculatedRotation.eulerAngles.y, calculatedRotation.eulerAngles.z),
                calculatedRotation, upness);

        transform.SetPositionAndRotation(hit.point, finalRotation);
    }
}