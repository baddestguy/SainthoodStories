using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class ActionButton : MonoBehaviour
{
    public Image ButtonImage;
    public string ButtonName;
    public bool Enabled;
    public float Timer;

    public bool ShouldWiggle = true;
    public float WiggleDelay = 1f;

    private void OnEnable()
    {
        StartCoroutine(WaitThenWiggleAsync());
    }

    IEnumerator WaitThenWiggleAsync()
    {
        yield return null;
        if (!ShouldWiggle || !Enabled || GameSettings.Instance.FTUE) yield break;
        transform.DOPunchScale(transform.localScale*0.5f, 0.5f, elasticity: 0f).SetDelay(WiggleDelay);
    }

    public void Wiggle(float delay = 0f)
    {
        transform.DOPunchScale(transform.localScale * 0.5f, 0.5f, elasticity: 0f).SetDelay(delay);
    }

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

    public void SetTimer(float timer)
    {
        Timer = timer;
    }
}
