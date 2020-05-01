using UnityEngine;
using UnityEngine.Events;

public class GameClock
{
    public int Time { get; private set; }
    public int Day { get; private set; }
    public static UnityAction<int, int> Ticked;

    public GameClock(int startTime)
    {
        Time = startTime;
        Day = 1; //TODO: HACK
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
            Day++;
            Time = 0;
        }

        Ticked?.Invoke(Time, Day);
    }
}
