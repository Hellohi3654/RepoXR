using System.Collections;
using RepoXR.Managers;
using UnityEngine;

namespace RepoXR.UI;

public class Crosshair : MonoBehaviour
{
    private const int LayerMask = 1 << 0 | 1 << 9 | 1 << 10 | 1 << 16;
    
    private Transform handTransform;

    private IEnumerator Start()
    {
        yield return null;

        if (VRSession.Instance is not { } session)
            yield break;

        handTransform = session.Player.MainHand;
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

        transform.position = hit.point;
        transform.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal) * Quaternion.Euler(270, 270, 0);
        transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, 0);
    }
}