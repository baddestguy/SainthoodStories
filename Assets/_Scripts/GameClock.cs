using UnityEngine;
using UnityEngine.Events;

public class GameClock
{
    public double Time { get; private set; }
    public int Day { get; private set; }

    public static bool EndofDay;

    public static UnityAction<double, int> Ticked;
    public static UnityAction ExecuteEvents;
    public static UnityAction EndDay;
    public static UnityAction StartNewDay;

    public static bool DeltaTime { get; private set; }

    public GameClock(double startTime, int day = 1)
    {
        Time = startTime;
        Day = day;
    }

    public string TimeDisplay()
    {
        if (Time - (int)Time == 0) return $"{(int)Time}:00";
        if (Time - (int)Time == 0.25) return $"{(int)Time}:15";
        if (Time - (int)Time == 0.5) return $"{(int)Time}:30";
        if (Time - (int)Time == 0.75) return $"{(int)Time}:45";

        return "Error";
    }

    public void SetClock(double time, int day)
    {
        Time = time;
        Day = day;

        if (Time > 23)
        {
            //Day++;
            Time = 23;
        }
    }

    public void AddTime(double deltaTime)
    {
        Time += deltaTime;
        if (Time > 23)
        {
            //Day++;
            Time = 23;
        }
    }

    public void AddTicks(double ticks)
    {
        for(int i = 0; i < ticks; i++)
        {
            Time += 0.25;
            if (Time > 23.75)
            {
                Day++;
                Time = Time - 24;
            }
        }
    }

    public void Tick()
    {
        Time += 0.25;
        if(Time > 23.75)
        {
            Day++;
            if (Day > 7) Day = 1;
            Time = 0;
            EndofDay = true;
        }
        else
        {
            EndofDay = false;
        }

        DeltaTime = true;
        Ticked?.Invoke(Time, Day);
        if (EndofDay)
        {
            EndDay?.Invoke();
        }
        ExecuteEvents?.Invoke();
        SaveDataManager.Instance.SaveGame();
        DeltaTime = false;

    //    Debug.Log(Day + " : " + Time);
    }

    public void Ping()
    {
        Ticked?.Invoke(Time, Day);
        ExecuteEvents?.Invoke();
    }

    public void EndTheWeek()
    {
        Day = 1;
        Time = 6;
    }

    public void Reset()
    {
        if(Time > 5)
        {
            Day++;
            if (Day > 7) Day = 1;
        }
        Time = 5;
    }

    public bool DuringTheDay()
    {
        return Time >= 6 && Time < 21;
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

    public void OnOveride(int DayOverride, double TimeOverride)
    {
     //   DeltaTime = false;
        SetClock(TimeOverride, DayOverride);
        Ping();
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