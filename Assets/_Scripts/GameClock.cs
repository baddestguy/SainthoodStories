using UnityEngine;
using UnityEngine.Events;

public class GameClock
{
    public int Time { get; private set; }
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
    }
}
