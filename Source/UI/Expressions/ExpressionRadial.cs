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

    internal bool isActive;
    private float animationLerp;
    private int hoveredPart = -1;

    // Helper bool that prevents the chat from opening if the radial menu was closed using the chat key
    internal bool closedLastPress;

    private void Awake()
    {
        instance = this;

        // Move the preview UI to the middle of the radial menu
        var playerRenderTex = PlayerExpressionsUI.instance.transform;
        playerRenderTex.SetParent(previewTransform, false);
        
        ReloadParts();
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

        if (closedLastPress)
            closedLastPress = false;

        var pressed = VRInputSystem.instance.ExpressionPressed() ||
                      // Close radial menu when chat becomes active
                      (isActive && SemiFunc.InputDown(InputKey.Chat));
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
                closedLastPress = true;

                MenuManager.instance.MenuEffectClick(MenuManager.MenuClickEffectType.Action, soundOnly: true);
                break;
        }

        if (isActive)
            PlayerExpressionsUI.instance.Show();

        animationLerp += Time.deltaTime * (isActive ? 4 : -4);
        animationLerp = Mathf.Clamp01(animationLerp);

        transform.localScale = Vector3.LerpUnclamped(Vector3.zero, Vector3.one, animationCurve.Evaluate(animationLerp));

        // Hide the background if no expressions are active
        var playerColor = PlayerAvatar.instance.playerAvatarVisuals.color;
        previewBackground.color = Color.Lerp(previewBackground.color,
            new Color(playerColor.r, playerColor.g, playerColor.b,
                1f / 255 * (DataManager.instance.activeExpressions.Count > 0 ? 50 : 0)), 8 * Time.deltaTime);

        HoverLogic();
        ActivateLogic();
    }

    public static bool ExpressionActive(InputKey input)
    {
        var expression = (ExpressionPart.Expression)(input - InputKey.Expression1);
        return DataManager.instance.activeExpressions.Contains(expression);
    }

    private void ResetPosition()
    {
        transform.position = handTransform.position;
        transform.LookAt(CameraUtils.Instance.MainCamera.transform.position);
    }

    private void ReloadParts()
    {
        foreach (var part in parts)
            part.SetActive(DataManager.instance.activeExpressions.Contains(part.expression));
    }

    private void UpdateBindingHand()
    {
        var chatAction = Actions.Instance["Chat"];
        var bindingIndex = Mathf.Max(chatAction.GetBindingIndex(VRInputSystem.instance.CurrentControlScheme), 0);
        var bindingPath = Actions.Instance["Chat"].bindings[bindingIndex].effectivePath;

        if (!Utils.GetControlHand(bindingPath, out var hand))
            return;

        if (currentHand == hand)
            return;

        currentHand = hand;
        handTransform = hand == HapticManager.Hand.Left
            ? VRSession.Instance.Player.Rig.leftArmTarget
            : VRSession.Instance.Player.Rig.rightArmTarget;
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

        // Not while the pause menu is open
        if (MenuManager.instance.currentMenuPage)
            return;

        var action = currentHand == HapticManager.Hand.Left ? "ExpressionLeft" : "ExpressionRight";
        if (!Actions.Instance[action].WasPressedThisFrame())
            return;

        MenuManager.instance.MenuEffectClick(MenuManager.MenuClickEffectType.Tick, soundOnly: true);
        HapticManager.Impulse(currentHand, HapticManager.Type.Impulse);

        var part = parts[hoveredPart];
        var active = part.Toggle();

        if (active)
            DataManager.instance.activeExpressions.Add(part.expression);
        else
            DataManager.instance.activeExpressions.Remove(part.expression);
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
        var playerColor = PlayerAvatar.instance.playerAvatarVisuals.color;

        var targetTextColor =
            isActive ? Utils.GetTextColor(playerColor) : Utils.GetTextColor(playerColor, 0.6f, 0.85f);
        var targetBgColor = isActive ? Color.Lerp(playerColor, Color.white, 0.4f) :
            isHovered ? Color.Lerp(playerColor, Color.white, 0.2f) : Color.Lerp(playerColor, Color.black, 0.2f);
        
        targetTextColor.a = isActive ? 1 : 0.85f;
        targetBgColor.a = isActive ? 0.5f : isHovered ? 0.3f : 0.2f;

        text.color = Color.Lerp(text.color, targetTextColor, 8 * Time.deltaTime);
        background.color = Color.Lerp(background.color, targetBgColor, 8 * Time.deltaTime);

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

    public void SetActive(bool active)
    {
        isActive = active;
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