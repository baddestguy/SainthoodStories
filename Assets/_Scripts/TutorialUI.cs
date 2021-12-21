using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class TutorialUI : MonoBehaviour
{
    public GameObject MyUI;
    public int Threshold;
    public double Time;
    public double Day;

    public bool SlideRight;
    private bool Animated;
    public bool ShouldAnimate;

    
    private Vector3 startPoint;
    private Vector3 oldScale;

    public void Start()
    {
        oldScale = MyUI.transform.localScale;
        startPoint = MyUI.transform.localPosition;
        if (GameSettings.Instance.FTUE)
        {
            MyUI.transform.localScale = Vector3.zero;
        }
    }

    public void OnEnable()
    {
        StartCoroutine(Refresh());
    }

    private IEnumerator Refresh()
    {
        yield return null;
        if (GameSettings.Instance.FTUE)
        {
            if(Threshold > -1)
            {
                if(TutorialManager.Instance.CurrentTutorialStep >= Threshold)
                {
                    MyUI.transform.localScale = oldScale;
                    if (ShouldAnimate && !Animated)
                    {
                        if (SlideRight)
                        {
                            MyUI.transform.position -= new Vector3(300, 0, 0);
                            MyUI.transform.DOLocalMoveX(startPoint.x, 1f).SetEase(Ease.InBack);
                        }
                        else
                        {
                            MyUI.transform.position += new Vector3(300, 0, 0);
                            MyUI.transform.DOLocalMoveX(startPoint.x, 1f).SetEase(Ease.InBack);
                        }
                    }
                    Animated = true;
                }
                else
                {
                    MyUI.transform.localScale = Vector3.zero;
                }
            }
            else
            {
                GameClock c = GameManager.Instance.GameClock;
                if (c.Day >= Day && c.Time >= Time)
                {
                    MyUI.transform.localScale = oldScale;
                    if (ShouldAnimate && !Animated)
                    {
                        if (SlideRight)
                        {
                            MyUI.transform.position -= new Vector3(300, 0, 0);
                            //MyUI.transform.DOLocalMoveX(-810f, 1f).SetEase(Ease.InBack);
                            MyUI.transform.DOLocalMoveX(startPoint.x, 1f).SetEase(Ease.InBack);
                        }
                        else
                        {
                            MyUI.transform.position += new Vector3(300, 0, 0);
                            //MyUI.transform.DOLocalMoveX(788.6f, 1f).SetEase(Ease.InBack);
                            MyUI.transform.DOLocalMoveX(startPoint.x, 1f).SetEase(Ease.InBack);
                        }
                    }
                    Animated = true;
                }
                else
                {
                    MyUI.transform.localScale = Vector3.zero;
                }
            }
        }
        else
        {
            if (Threshold < 0)
            {
                GameClock c = GameManager.Instance.GameClock;
                GameClock myClock = new GameClock(Time, (int)Day);
                if (c > myClock)
                {
                    MyUI.transform.localScale = oldScale;
                    if (ShouldAnimate && !Animated)
                    {
                        if (SlideRight)
                        {
                            MyUI.transform.position -= new Vector3(300, 0, 0);
                            MyUI.transform.DOLocalMoveX(startPoint.x, 1f).SetEase(Ease.InBack);
                        }
                        else
                        {
                            MyUI.transform.position += new Vector3(300, 0, 0);
                            MyUI.transform.DOLocalMoveX(startPoint.x, 1f).SetEase(Ease.InBack);
                        }
                    }
                    Animated = true;
                }
                else
                {
                //    MyUI.transform.localScale = Vector3.zero;
                }
            }
            else
            {
                MyUI.transform.localScale = oldScale;
                if (ShouldAnimate && !Animated)
                {
                    if (SlideRight)
                    {
                        MyUI.transform.position -= new Vector3(300, 0, 0);
                        MyUI.transform.DOLocalMoveX(startPoint.x, 1f).SetEase(Ease.InBack);
                    }
                    else
                    {
                        MyUI.transform.position += new Vector3(300, 0, 0);
                        MyUI.transform.DOLocalMoveX(startPoint.x, 1f).SetEase(Ease.InBack);
                    }
                }
                Animated = true;
            }
        }
    }

    private void OnDisable()
    {
    }
}
