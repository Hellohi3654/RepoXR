using System;
using RepoXR.Assets;
using RepoXR.Input;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

namespace RepoXR.UI;

public class XRRayInteractorManager : MonoBehaviour
{
    public static XRRayInteractorManager? Instance { get; private set; }

    private XRRayInteractor leftController;
    private XRRayInteractor rightController;
    private LineRenderer leftRenderer;
    private LineRenderer rightRenderer;

    private ActiveController activeController;
    
    private void Awake()
    {
        Instance = this;
        
        leftController = CreateInteractorController("Left");
        rightController = CreateInteractorController("Right");
        
        leftRenderer = leftController.GetComponent<LineRenderer>();
        rightRenderer = rightController.GetComponent<LineRenderer>();
        
        leftController.GetComponent<ActionBasedController>().uiPressAction.action.performed += LeftControllerPressed;
        rightController.GetComponent<ActionBasedController>().uiPressAction.action.performed += RightControllerPressed;
        
        UpdateActiveController(ActiveController.Right);
    }

    private void OnDestroy()
    {
        Instance = null;
        
        leftController.GetComponent<ActionBasedController>().uiPressAction.action.performed -= LeftControllerPressed;
        rightController.GetComponent<ActionBasedController>().uiPressAction.action.performed -= RightControllerPressed;
    }

    public void SetVisible(bool visible)
    {
        leftController.GetComponent<XRInteractorLineVisual>().enabled = visible;
        rightController.GetComponent<XRInteractorLineVisual>().enabled = visible;
    }

    public void SetLineSortingOrder(int sortingOrder)
    {
        leftRenderer.sortingOrder = sortingOrder;
        rightRenderer.sortingOrder = sortingOrder;
    }
    
    private void LeftControllerPressed(InputAction.CallbackContext obj)
    {
        UpdateActiveController(ActiveController.Left);
    }

    private void RightControllerPressed(InputAction.CallbackContext obj)
    {
        UpdateActiveController(ActiveController.Right);
    }

    public Vector2 GetUIHitPosition(RectTransform? rect)
    {
        if (!GetActiveInteractor().TryGetCurrentUIRaycastResult(out var result))
            return Vector2.one * -1000;

        var canvas = rect == null
            ? result.gameObject.GetComponentInParent<Canvas>().transform
            : rect.GetComponentInParent<Canvas>().transform;

        var local = canvas.InverseTransformPoint(result.worldPosition);
        return new Vector2(local.x, local.y);
    }

    public bool GetTriggerDown()
    {
        return GetActiveInteractor().GetComponent<ActionBasedController>().uiPressAction.action.WasPressedThisFrame();
    }

    public bool GetTriggerButton()
    {
        return GetActiveInteractor().GetComponent<ActionBasedController>().uiPressAction.action.IsPressed();
    }

    public float GetUIScrollX()
    {
        return GetActiveInteractor().GetComponent<ActionBasedController>().uiScrollAction.action.ReadValue<Vector2>().x;
    }

    public float GetUIScrollY()
    {
        return GetActiveInteractor().GetComponent<ActionBasedController>().uiScrollAction.action.ReadValue<Vector2>().y;
    }

    private void UpdateActiveController(ActiveController newValue)
    {
        activeController = newValue;
        
        var oldInteractor = newValue == ActiveController.Left ? rightController : leftController;
        var newInteractor = newValue == ActiveController.Left ? leftController : rightController;

        var oldVisual = oldInteractor.GetComponent<XRInteractorLineVisual>();
        var newVisual = newInteractor.GetComponent<XRInteractorLineVisual>();

        oldVisual.lineLength = 1;
        newVisual.lineLength = 20;

        oldVisual.invalidColorGradient = oldVisual.validColorGradient = new Gradient
        {
            mode = GradientMode.Blend,
            alphaKeys =
            [
                new GradientAlphaKey(0.05f, 0),
                new GradientAlphaKey(0.05f, 0.8f),
                new GradientAlphaKey(0f, 1),
            ],
            colorKeys =
            [
                new GradientColorKey(Color.white, 0),
                new GradientColorKey(Color.white, 1)
            ]
        };
        
        newVisual.invalidColorGradient = new Gradient
        {
            mode = GradientMode.Blend,
            alphaKeys =
            [
                new GradientAlphaKey(0.2f, 0),
                new GradientAlphaKey(0.2f, 1),
            ],
            colorKeys =
            [
                new GradientColorKey(Color.white, 0),
                new GradientColorKey(Color.white, 1)
            ]
        };
        
        newVisual.validColorGradient = new Gradient
        {
            mode = GradientMode.Blend,
            alphaKeys =
            [
                new GradientAlphaKey(1, 0),
                new GradientAlphaKey(1, 1),
            ],
            colorKeys =
            [
                new GradientColorKey(Color.white, 0),
                new GradientColorKey(Color.white, 1)
            ]
        };
    }

    private XRRayInteractor GetActiveInteractor()
    {
        return activeController switch
        {
            ActiveController.Left => leftController,
            ActiveController.Right => rightController,
            _ => throw new ArgumentOutOfRangeException()
        };
    }
    
    private XRRayInteractor CreateInteractorController(string hand)
    {
        var go = new GameObject($"{hand} Controller")
        {
            transform =
            {
                parent = transform
            },
            layer = 5
        };

        var controller = go.AddComponent<ActionBasedController>();
        var interactor = go.AddComponent<XRRayInteractor>();
        var visual = go.AddComponent<XRInteractorLineVisual>();
        var renderer = go.GetComponent<LineRenderer>();

        visual.lineBendRatio = 1;
        visual.invalidColorGradient = new Gradient
        {
            mode = GradientMode.Blend,
            alphaKeys =
            [
                new GradientAlphaKey(0.1f, 0),
                new GradientAlphaKey(0.1f, 1),
            ],
            colorKeys =
            [
                new GradientColorKey(Color.white, 0),
                new GradientColorKey(Color.white, 1)
            ]
        };
        visual.enabled = true;
        visual.lineLength = 20;
        renderer.sortingOrder = 4;

        renderer.material = AssetCollection.DefaultLine;
        
        AddActionBasedControllerBinds(controller, hand);
        
        return interactor;
    }

    private static void AddActionBasedControllerBinds(ActionBasedController controller, string hand)
    {
        controller.enableInputTracking = true;
        controller.positionAction =
            new InputActionProperty(hand == "Left"
                ? Actions.Instance.LeftHandPosition
                : Actions.Instance.RightHandPosition);
        controller.rotationAction =
            new InputActionProperty(hand == "Left"
                ? Actions.Instance.LeftHandRotation
                : Actions.Instance.RightHandRotation);
        controller.trackingStateAction = new InputActionProperty(hand == "Left"
            ? Actions.Instance.LeftHandTrackingState
            : Actions.Instance.RightHandTrackingState);

        controller.enableInputActions = true;
        controller.selectAction = new InputActionProperty(AssetCollection.DefaultXRActions.FindAction($"{hand}/Select"));
        controller.selectActionValue =
            new InputActionProperty(AssetCollection.DefaultXRActions.FindAction($"{hand}/Select Value"));
        controller.activateAction =
            new InputActionProperty(AssetCollection.DefaultXRActions.FindAction($"{hand}/Activate"));
        controller.activateActionValue =
            new InputActionProperty(AssetCollection.DefaultXRActions.FindAction($"{hand}/Activate Value"));
        controller.uiPressAction = new InputActionProperty(AssetCollection.DefaultXRActions.FindAction($"{hand}/UI Press"));
        controller.uiPressActionValue =
            new InputActionProperty(AssetCollection.DefaultXRActions.FindAction($"{hand}/UI Press Value"));
        controller.uiScrollAction =
            new InputActionProperty(AssetCollection.DefaultXRActions.FindAction($"{hand}/UI Scroll"));
        controller.rotateAnchorAction =
            new InputActionProperty(AssetCollection.DefaultXRActions.FindAction($"{hand}/Rotate Anchor"));
        controller.translateAnchorAction =
            new InputActionProperty(AssetCollection.DefaultXRActions.FindAction($"{hand}/Translate Anchor"));
        controller.scaleToggleAction =
            new InputActionProperty(AssetCollection.DefaultXRActions.FindAction($"{hand}/Scale Toggle"));
        controller.scaleDeltaAction =
            new InputActionProperty(AssetCollection.DefaultXRActions.FindAction($"{hand}/Scale Delta"));
    }

    private enum ActiveController
    {
        Left,
        Right
    }
}