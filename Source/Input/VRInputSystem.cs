using System.Collections.Generic;
using Newtonsoft.Json;
using RepoXR.Assets;
using UnityEngine;
using UnityEngine.InputSystem;

namespace RepoXR.Input;

public class VRInputSystem : MonoBehaviour
{
    public static VRInputSystem Instance;

    private PlayerInput playerInput;

    public InputActionAsset Actions => playerInput.actions;
    public string CurrentControlScheme => playerInput.currentControlScheme;

    private Dictionary<string, bool> inputToggle = [];
    
    private void Awake()
    {
        Instance = this;
        
        playerInput = gameObject.AddComponent<PlayerInput>();
        playerInput.actions = AssetCollection.VRInputs;
        playerInput.defaultActionMap = "VR Actions";
        playerInput.neverAutoSwitchControlSchemes = false;
        playerInput.notificationBehavior = PlayerNotifications.InvokeCSharpEvents;
        
        playerInput.actions.LoadBindingOverridesFromJson(Plugin.Config.ControllerBindingsOverride.Value);
        playerInput.ActivateInput();

        inputToggle =
            JsonConvert.DeserializeObject<Dictionary<string, bool>>(Plugin.Config.InputToggleBindings.Value) ?? [];
    }

    public void ActivateInput()
    {
        playerInput.ActivateInput();
    }

    public void DeactivateInput()
    {
        playerInput.DeactivateInput();
    }

    public PlayerInput GetPlayerInput()
    {
        return playerInput;
    }
    
    public void InputToggleRebind(string inputAction, bool toggle)
    {
        inputToggle[inputAction] = toggle;
        
        SaveInputToggles();
    }

    public bool InputToggleGet(string action)
    {
        if (inputToggle.TryGetValue(action, out var value))
            return value;
        
        // Check for default
        foreach (var control in AssetCollection.RemappableControls.controls)
            if (control.currentInput.action.name == action)
            {
                inputToggle[action] = control.defaultToggle;

                return control.defaultToggle;
            }

        // If all else fails: default to hold
        return false;
    }

    private void SaveInputToggles()
    {
        Plugin.Config.InputToggleBindings.Value = JsonConvert.SerializeObject(inputToggle);
    }
}