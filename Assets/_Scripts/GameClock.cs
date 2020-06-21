using UnityEngine;
using UnityEngine.Events;

public class GameClock
{
    public double Time { get; private set; }
    public int Day { get; private set; }

    public static bool EndofDay;

    public static UnityAction<double, int> Ticked;
    public static UnityAction ExecuteEvents;

    public GameClock(double startTime, int day = 1)
    {
        Time = startTime;
        Day = day;
    }

    public void SetClock(double time, int day)
    {
        Time = time;
        Day = day;

        if (Time > 23.5)
        {
            Day++;
            Time = Time - 24;
        }
    }

    public void AddTime(int deltaTime)
    {
        Time += deltaTime;
        if (Time > 23.5)
        {
            Day++;
            Time = Time - 24;
        }
    }

    public void Tick()
    {
        Time += 0.5;
        if(Time > 23.5)
        {
            Day++;
            Time = 0;
            EndofDay = true;
        }
        else
        {
            EndofDay = false;
        }

        Ticked?.Invoke(Time, Day);
        ExecuteEvents?.Invoke();
    }

    public void Reset()
    {
        double timeDiff = 23.5 - Time;
        Time += timeDiff;
        Tick();
    }

    public static bool operator >(GameClock gameClock1, GameClock gameClock2)
    {
        if (gameClock1 == null || gameClock2 == null) return false;
   
        if (gameClock1.Day > gameClock2.Day) return true;
        if (gameClock1.Day < gameClock2.Day) return false;

        if (gameClock1.Time > gameClock2.Time) return true;
        if (gameClock1.Time < gameClock2.Time) return false;

        return false;
    }

    public static bool operator <(GameClock gameClock1, GameClock gameClock2)
    {
        if (gameClock1 == null || gameClock2 == null) return false;
   
        if (gameClock1.Day < gameClock2.Day) return true;
        if (gameClock1.Day > gameClock2.Day) return false;

        if (gameClock1.Time < gameClock2.Time) return true;
        if (gameClock1.Time > gameClock2.Time) return false;

        return false;
    }

    public static bool operator >=(GameClock gameClock1, GameClock gameClock2)
    {
        if (gameClock1 == null || gameClock2 == null) return false;
  
        if (gameClock1.Day > gameClock2.Day) return true;
        if (gameClock1.Day < gameClock2.Day) return false;

        if (gameClock1.Time > gameClock2.Time) return true;
        if (gameClock1.Time < gameClock2.Time) return false;

        return true;
    }

    public static bool operator <=(GameClock gameClock1, GameClock gameClock2)
    {
        if (gameClock1 == null || gameClock2 == null) return false;
   
        if (gameClock1.Day < gameClock2.Day) return true;
        if (gameClock1.Day > gameClock2.Day) return false;

        if (gameClock1.Time < gameClock2.Time) return true;
        if (gameClock1.Time > gameClock2.Time) return false;

        return true;
    }

    public static bool operator ==(GameClock gameClock1, GameClock gameClock2)
    {
        if (gameClock1?.Day == null && gameClock2?.Day == null) return true;
        if (gameClock1?.Day == null || gameClock2?.Day == null) return false;

        if (gameClock1.Day == gameClock2.Day && gameClock1.Time == gameClock2.Time) return true;

        return false;
    }


    public static bool operator !=(GameClock gameClock1, GameClock gameClock2)
    {
        if (gameClock1 == null && gameClock2 == null) return false;
        if (gameClock1 == null || gameClock2 == null) return true;

        if (gameClock1.Day != gameClock2.Day || gameClock1.Time != gameClock2.Time) return true;

        return false;
    }

    public override bool Equals(object o)
    {
        var gameClock2 = o as GameClock;
        if (gameClock2 == null) return false;

        return Day == gameClock2.Day && Time == gameClock2.Time;
    }

    public override int GetHashCode()
    {
        unchecked
        {
            const int HashingBase = (int)2166136261;
            const int HashingMultiplier = 16777619;

            int hash = HashingBase;
            hash = (hash * HashingMultiplier) ^ (!Object.ReferenceEquals(null, Day) ? Day.GetHashCode() : 0);
            hash = (hash * HashingMultiplier) ^ (!Object.ReferenceEquals(null, Time) ? Time.GetHashCode() : 0);
            return hash;
        }
    }
}

public static class GameClockExtensions
{
    public static double TimeDifference(this GameClock gameClock1, GameClock gameClock2)
    {
        double time;
        if (gameClock2.Day > gameClock1.Day)
        {
            time = gameClock2.Time + ((gameClock2.Day - gameClock1.Day) * 24);
        }
        else
        {
            time = gameClock2.Time;
        }
        return time - gameClock1.Time;
    }
}