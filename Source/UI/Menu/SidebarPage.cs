using UnityEngine;

namespace RepoXR.UI.Menu;

public class SidebarPage : MonoBehaviour
{
    public void ButtonEventSettings()
    {
        MenuHelper.PageOpenOnTop(MenuHelper.RepoXRMenuPage.VRSettings);
    }
}