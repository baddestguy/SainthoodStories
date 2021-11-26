using DG.Tweening;
using UnityEngine;

public class ButtonWiggle : MonoBehaviour
{
    public bool WiggleOnAwake;
    public bool ShouldWiggle = true;
    public float WiggleDelay = 0f;

    private void OnEnable()
    {
        if(WiggleOnAwake)
            Wiggle();
    }

    public void Wiggle(float delay = 0f)
    {
        if (!ShouldWiggle) return;

        transform.DOPunchScale(transform.localScale * 0.5f, 0.5f, elasticity: 0f).SetDelay(delay);
    }
}
