using UnityEngine;
using UnityEngine.Events;

public class GameClock
{
    private int Time; 
    public static UnityAction<int> Ticked;

    public GameClock(int startTime)
    {
        Time = startTime;
    }

    public void SetTime(int startTime)
    {
        Time = startTime;
    }

    public void Tick()
    {
        Time++;
        if(Time > 23)
        {
            Time = 0;
        }

        Ticked?.Invoke(Time);
        Debug.Log("TIME: " + Time);
    }
}
