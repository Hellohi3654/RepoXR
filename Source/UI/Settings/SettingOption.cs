using TMPro;
using UnityEngine;

namespace RepoXR.UI.Settings;

public class SettingOption : MonoBehaviour
{
    public string settingCategory;
    public string settingName;
    public string? settingDisplayName;
    public TextMeshProUGUI settingText;

    public RectTransform rectTransform;
    
    private void Start()
    {
        settingText.text = settingDisplayName ?? Utils.ToHumanReadable(settingName);
    }

    public void FetchBoolOption()
    {
        var option = GetComponent<MenuTwoOptions>();
        option.startSettingFetch = (bool)Plugin.Config.File[settingCategory, settingName].BoxedValue;
    }

    public void UpdateBool(bool value)
    {
        Plugin.Config.File[settingCategory, settingName].BoxedValue = value;
    }

    public void UpdateInt(int value)
    {
        Plugin.Config.File[settingCategory, settingName].BoxedValue = value;
    }

    public void UpdateFloat(float value)
    {
        Plugin.Config.File[settingCategory, settingName].BoxedValue = value;
    }

    public void UpdateSlider()
    {
        var slider = GetComponent<FloatMenuSlider>();

        if (slider.isInteger)
            UpdateInt(Mathf.RoundToInt(slider.currentValue));
        else
            UpdateFloat(slider.currentValue);
    }
}