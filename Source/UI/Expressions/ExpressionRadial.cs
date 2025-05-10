using System.Collections.Generic;
using RepoXR.Input;
using RepoXR.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace RepoXR.UI.Expressions;

public class ExpressionRadial : MonoBehaviour
{
    public static ExpressionRadial instance;
    
    public Transform handTransform;

    [SerializeField] protected AnimationCurve animationCurve;
    [SerializeField] protected Transform canvasTransform;
    [SerializeField] protected Transform previewTransform;
    [SerializeField] protected Image previewBackground;
    [SerializeField] protected ExpressionPart[] parts;

    private HapticManager.Hand currentHand = HapticManager.Hand.Both;
    
    private bool isActive;
    private float animationLerp;
    private int hoveredPart = -1;

    private List<ExpressionPart.Expression> activeExpressions = [];
    
    private void Awake()
    {
        instance = this;
        
        var playerRenderTex = PlayerExpressionsUI.instance.transform;
        playerRenderTex.SetParent(previewTransform, false);
    }

    private void OnDestroy()
    {
        instance = null!;
    }

    private void Update()
    {
        UpdateBindingHand();

        if (currentHand == HapticManager.Hand.Both)
            return;
        
        var pressed = VRInputSystem.instance.ExpressionPressed() ||
                      // Close radial menu when chat becomes active
                      (isActive && ChatManager.instance.chatState == ChatManager.ChatState.Active);
        switch (pressed)
        {
            case true when !isActive:
                ResetPosition();
                isActive = pressed;
                
                if (ChatManager.instance.chatState == ChatManager.ChatState.Active)
                    ChatManager.instance.StateSet(ChatManager.ChatState.Inactive);
            
                MenuManager.instance.MenuEffectClick(MenuManager.MenuClickEffectType.Confirm, soundOnly: true);
                break;
            case true when isActive:
                isActive = false;

                MenuManager.instance.MenuEffectClick(MenuManager.MenuClickEffectType.Action, soundOnly: true);
                break;
        }

        if (isActive)
            PlayerExpressionsUI.instance.Show();

        animationLerp += Time.deltaTime * (isActive ? 4 : -4);
        animationLerp = Mathf.Clamp01(animationLerp);

        transform.localScale = Vector3.LerpUnclamped(Vector3.zero, Vector3.one, animationCurve.Evaluate(animationLerp));

        // Hide the background if no expressions are active
        previewBackground.color = Color.Lerp(previewBackground.color,
            new Color(previewBackground.color.r, previewBackground.color.g, previewBackground.color.b,
                1f / 255 * (activeExpressions.Count > 0 ? 50 : 0)), 8 * Time.deltaTime);

        HoverLogic();
        ActivateLogic();
    }

    public static bool ExpressionActive(InputKey input)
    {
        if (instance is not { } radial)
            return false;
        
        var expression = (ExpressionPart.Expression)(input - InputKey.Expression1);
        return radial.activeExpressions.Contains(expression);
    }
    
    private void ResetPosition()
    {
        transform.position = handTransform.position;
        transform.LookAt(CameraUtils.Instance.MainCamera.transform.position);
    }

    private void UpdateBindingHand()
    {
        var chatAction = Actions.Instance["Chat"];
        var bindingIndex = Mathf.Max(chatAction.GetBindingIndex(VRInputSystem.instance.CurrentControlScheme), 0);
        var bindingPath = Actions.Instance["Chat"].bindings[bindingIndex].effectivePath;

        if (!Utils.GetControlHand(bindingPath, out var hand))
            return;

        if (currentHand != hand)
        {
            currentHand = hand;

            if (hand == HapticManager.Hand.Left)
                handTransform = VRSession.Instance.Player.Rig.leftArmTarget;
            else
                handTransform = VRSession.Instance.Player.Rig.rightArmTarget;
        }
    }
    
    private void HoverLogic()
    {
        if (!isActive)
            return;
        
        var centerToHand = handTransform.position - canvasTransform.position;
        var projected = Vector3.ProjectOnPlane(centerToHand, canvasTransform.forward);
        var angle = Vector3.SignedAngle(canvasTransform.up, projected, -canvasTransform.forward);

        if (angle < 0)
            angle += 360;

        var currentPart = (int)angle * parts.Length / 360;
        if (currentPart != hoveredPart)
            HapticManager.Impulse(currentHand, HapticManager.Type.Impulse, 0.03f);
        
        hoveredPart = currentPart;
        
        for (var i = 0; i < parts.Length; i++)
            parts[i].SetHovered(i == hoveredPart);
    }

    private void ActivateLogic()
    {
        if (!isActive || hoveredPart < 0)
            return;
        
        var action = currentHand == HapticManager.Hand.Left ? "ExpressionLeft" : "ExpressionRight";
        if (!Actions.Instance[action].WasPressedThisFrame()) 
            return;
        
        MenuManager.instance.MenuEffectClick(MenuManager.MenuClickEffectType.Tick, soundOnly: true);
        HapticManager.Impulse(currentHand, HapticManager.Type.Impulse);
        
        var part = parts[hoveredPart];
        var active = part.Toggle();

        if (active)
            activeExpressions.Add(part.expression);
        else
            activeExpressions.Remove(part.expression);
    }
}

public class ExpressionPart : MonoBehaviour
{
    public Expression expression;

    public Color defaultColor;
    public Color hoverColor;
    public Color activeColor;

    public Color textDefaultColor;
    public Color textActiveColor;

    public AnimationCurve triggerAnimation;

    private Image background;
    private TextMeshProUGUI text;

    private Vector3 currentScale;
    private bool isHovered;
    private bool isActive;

    private float animTimer = 1;

    private void Awake()
    {
        background = GetComponent<Image>();
        text = GetComponentInChildren<TextMeshProUGUI>();
    }

    private void Update()
    {
        text.color = Color.Lerp(text.color, isActive ? textActiveColor : textDefaultColor, 8 * Time.deltaTime);
        background.color = Color.Lerp(background.color, isActive ? activeColor : isHovered ? hoverColor : defaultColor,
            8 * Time.deltaTime);

        currentScale = Vector3.Lerp(currentScale, Vector3.one * (isHovered ? 1.1f : 1), 8 * Time.deltaTime);

        var targetScale = currentScale;

        if (animTimer < 1)
        {
            animTimer += Time.deltaTime * 4f;
            animTimer = Mathf.Clamp01(animTimer);

            var animValue = Vector3.one * (triggerAnimation.Evaluate(animTimer) * 0.15f);

            targetScale += isActive ? animValue : -animValue;
        }

        transform.localScale = targetScale;
    }

    public void SetHovered(bool hovered)
    {
        isHovered = hovered;
    }

    public bool Toggle()
    {
        isActive = !isActive;
        animTimer = 0;

        return isActive;
    }

    public enum Expression
    {
        Angry,
        Sad,
        Suspicious,
        EyesClosed,
        Crazy,
        Happy
    }
}