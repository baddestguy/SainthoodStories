using UnityEngine;
using UnityEngine.UI;

public class ActionButton : MonoBehaviour
{
    public Image ButtonImage;
    public string ButtonName;
    public bool Enabled;

    public void RefreshButton(bool enabled)
    {
        Enabled = enabled;

        if (!enabled)
        {
            Color c = ButtonImage.color;
            c.a = 0.2f;
            ButtonImage.color = c;
        }
        else
        {
            Color c = ButtonImage.color;
            c.a = 1f;
            ButtonImage.color = c;
        }
    }
}
