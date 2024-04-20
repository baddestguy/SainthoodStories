using System;
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
        transform.DOComplete();
        transform.DOPunchScale(transform.localScale * 0.5f, 0.5f, elasticity: 0f).SetDelay(WiggleDelay);
    }

    public void Wiggle(float delay = 0f)
    {
        transform.DOComplete();
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

    #region XboxSupport

    private Vector3? _preHoverScale;


    /// <summary>
    /// Enlarge an action button to visibly show a user that it is the current action button that will be triggered by the controller
    /// </summary>
    public void HandleControllerHover()
    {
        transform.DOComplete();
        _preHoverScale ??= transform.localScale;
        transform.DOScale(_preHoverScale.Value * 1.5f, 0.5f);
    }
    /// <summary>
    /// Reset an action button to visibly show a user that it is no longer the current action button that will be triggered by the controller
    /// </summary>
    public void HandleControllerExit()
    {
        transform.DOComplete();
        transform.DOScale(_preHoverScale!.Value, 0.5f);
        _preHoverScale = null;
    }

    public bool HasCriticalCircle => !ButtonName.Equals("WORLD", StringComparison.InvariantCultureIgnoreCase) &&
                                     !ButtonName.Equals("ENTER", StringComparison.InvariantCultureIgnoreCase) &&
                                     !ButtonName.Equals("EXIT", StringComparison.InvariantCultureIgnoreCase);

    #endregion
}
