using UnityEngine;
using UnityEngine.UI;

public class TutorialButton : MonoBehaviour
{
    public Image ButtonImage;
    public string ButtonName;

    private void OnEnable()
    {
        RefreshTutorialButton();
    }

    public void RefreshTutorialButton()
    {
        if (GameSettings.Instance.FTUE && !TutorialManager.Instance.CheckTutorialButton(ButtonName))
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
