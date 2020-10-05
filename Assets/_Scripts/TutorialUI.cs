using UnityEngine;

public class TutorialUI : MonoBehaviour
{
    public GameObject MyUI;
    public int Threshold;
    public double Time;
    public double Day;

    private Vector3 oldScale;

    public void Start()
    {
        oldScale = MyUI.transform.localScale;
        if (GameSettings.Instance.FTUE)
        {
            EventsManager.EventDialogTriggered += RefreshUI;
            MyUI.transform.localScale = Vector3.zero;
        }
    }

    public void RefreshUI(bool started)
    {
        if (started) return;

        if (GameSettings.Instance.FTUE)
        {
            if(Threshold > -1)
            {
                if(TutorialManager.Instance.CurrentTutorialStep >= Threshold)
                {
                    MyUI.transform.localScale = oldScale;
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
                }
                else
                {
                    MyUI.transform.localScale = Vector3.zero;
                }
            }
            else
            {
                MyUI.transform.localScale = oldScale;
            }
        }
    }

    private void OnDisable()
    {
        EventsManager.EventDialogTriggered -= RefreshUI;
    }
}
