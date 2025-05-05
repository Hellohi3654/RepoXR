using System.Diagnostics;
using UnityEngine;

namespace RepoXR.UI.Menu;

public class SidebarPage : MonoBehaviour
{
    public void ButtonEventSettings()
    {
        Logger.LogDebug(new StackTrace());
        MenuHelper.PageOpenOnTop(MenuHelper.RepoXRMenuPage.VRSettings);
    }
}