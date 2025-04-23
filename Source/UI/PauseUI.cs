using UnityEngine;

namespace RepoXR.UI;

public class PauseUI : MonoBehaviour
{
    private Vector3 targetPos;
    private Quaternion targetRot;

    private Transform camera;
    private XRRayInteractorManager interactor;
    
    private void Awake()
    {
        camera = Camera.main!.transform;
        interactor = camera.transform.parent.gameObject.AddComponent<XRRayInteractorManager>();
        interactor.SetVisible(false);
    }

    private void Update()
    {
        transform.localPosition = Vector3.Lerp(transform.localPosition, targetPos, 8 * Time.deltaTime);
        transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRot, 8 * Time.deltaTime);
    }

    public void Show()
    {
        ResetPosition(true);
        
        interactor.SetVisible(true);
    }

    public void Hide()
    {
        interactor.SetVisible(false);
    }

    public void ResetPosition(bool instant = false)
    {
        var fwd = (camera.localRotation * Vector3.forward).normalized;
        fwd.y = 0;
        fwd.Normalize();

        var pos = camera.transform.localPosition + fwd * 5 + Vector3.up * 0.15f;
        var cameraPos = new Vector3(camera.localPosition.x, pos.y, camera.localPosition.z);

        targetPos = pos;
        targetRot = Quaternion.LookRotation(-(cameraPos - pos).normalized);

        if (instant)
        {
            transform.localPosition = targetPos;
            transform.localRotation = targetRot;
        }
    }
}