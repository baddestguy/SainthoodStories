using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class TutorialButton : MonoBehaviour
{
    public Image ButtonImage;
    public string ButtonName;

    private void OnEnable()
    {
        RefreshTutorialButton();
        Wiggle();
    }

    public void RefreshTutorialButton()
    {
        if (GameSettings.Instance.FTUE)
        {
            if (!TutorialManager.Instance.CheckTutorialButton(ButtonName))
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

    private void Wiggle()
    {
        if (!GameSettings.Instance.FTUE) return;
        if (TutorialManager.Instance.CheckTutorialButton(ButtonName))
        {
            transform.DOComplete();
            transform.DOPunchScale(transform.localScale * 0.5f, 0.5f, elasticity: 0f);
        }
    }
}
