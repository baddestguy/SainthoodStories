using UnityEngine;
using UnityEngine.Events;

public class GameClock
{
    public int Time { get; private set; }
    public int Day { get; private set; }
    public static UnityAction<int, int> Ticked;

    public GameClock(int startTime, int day = 1)
    {
        Time = startTime;
        Day = day;
    }

    public void SetTime(int startTime)
    {
        Time = startTime;
    }

    public void AddTime(int deltaTime)
    {
        Time += deltaTime;
        if (Time > 23)
        {
            Day++;
            Time = Time - 24;
        }
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

    public static bool operator >(GameClock gameClock1, GameClock gameClock2)
    {
        if (gameClock1.Day > gameClock2.Day) return true;
        if (gameClock1.Day < gameClock2.Day) return false;

        if (gameClock1.Time > gameClock2.Time) return true;
        if (gameClock1.Time < gameClock2.Time) return false;

        return false;
    }

    public static bool operator <(GameClock gameClock1, GameClock gameClock2)
    {
        if (gameClock1.Day < gameClock2.Day) return true;
        if (gameClock1.Day > gameClock2.Day) return false;

        if (gameClock1.Time < gameClock2.Time) return true;
        if (gameClock1.Time > gameClock2.Time) return false;

        return false;
    }

    public static bool operator >=(GameClock gameClock1, GameClock gameClock2)
    {
        if (gameClock1.Day > gameClock2.Day) return true;
        if (gameClock1.Day < gameClock2.Day) return false;

        if (gameClock1.Time > gameClock2.Time) return true;
        if (gameClock1.Time < gameClock2.Time) return false;

        return true;
    }

    public static bool operator <=(GameClock gameClock1, GameClock gameClock2)
    {
        if (gameClock1.Day < gameClock2.Day) return true;
        if (gameClock1.Day > gameClock2.Day) return false;

        if (gameClock1.Time < gameClock2.Time) return true;
        if (gameClock1.Time > gameClock2.Time) return false;

        return true;
    }
}
