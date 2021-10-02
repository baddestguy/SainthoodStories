using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class WeatherManager : MonoBehaviour
{
    public static WeatherManager Instance { get; private set; }

    public WeatherType WeatherType;
    public bool WeatherActive;

    private bool WeatherForecastTriggered;
    private Object RainResource;
    private GameObject CurrentWeatherGO;
    private GameClock WeatherStartTime;
    private GameClock WeatherEndTime;
    private DayNightCycle DayNightCycle;

    public static UnityAction<WeatherType, GameClock, GameClock> WeatherForecastActive;

    private void Awake()
    {
        Instance = this;   
        MissionManager.MissionComplete += MissionComplete;
    }

    void Start()
    {
        GameClock.Ticked += TriggerWeatherForecast;
        GameManager.MissionBegin += MissionBegin;
        RainResource = Resources.Load("Weather/Rain");
        WeatherStartTime = new GameClock(0);
        WeatherEndTime = new GameClock(0);
    }

    private void MissionBegin(Mission mission)
    {
        WeatherStartTime = new GameClock(0);
        WeatherEndTime = new GameClock(0);
        DayNightCycle = FindObjectOfType<DayNightCycle>();
        SetWeather(GameManager.Instance.GameClock.Time);
        UI.Instance.WeatherAlert(WeatherType, WeatherStartTime, WeatherEndTime);
    }

    private void TriggerWeatherForecast(double time, int day)
    {
        if (!GameClock.DeltaTime) return;

        if (WeatherForecastTriggered)
        {
            if (CurrentWeatherGO.activeSelf)
            {
                if (GameManager.Instance.GameClock == WeatherEndTime)
                {
                    CurrentWeatherGO.GetComponent<StormyWeather>().StopStorm();
                    WeatherForecastTriggered = false;
                    SetWeather(time);
                    DayNightCycle.SetFutureSkyBox(WeatherType);
                }
            }
            else
            {
                if (GameManager.Instance.GameClock == WeatherStartTime)
                {
                    CurrentWeatherGO.SetActive(true);
                    CurrentWeatherGO.GetComponent<StormyWeather>().StartStorm();
                    WeatherType = WeatherType.RAIN;
                    WeatherForecastActive?.Invoke(WeatherType, WeatherStartTime, WeatherEndTime);
                }
            }

            return;
        }
        else
        {
            SetWeather(time);
        }

        if (GameManager.Instance.MissionManager.CurrentMission.CurrentWeek == 1 && day < 3) return;

        var wData = GetWeatherData();

        switch (GameManager.MissionDifficulty)
        {
            case MissionDifficulty.NORMAL:
                if (wData != null || !SameDayAsMission() && Random.Range(0, 100) < 1)
                {
                    WeatherActivation(wData != null ? wData.StartTime : Random.Range(4, 7), wData != null ? wData.Duration : Random.Range(2, 3));
                }
                break;

            case MissionDifficulty.HARD:
                if (wData != null || !SameDayAsMission() && Random.Range(0, 100) < 2)
                {
                    WeatherActivation(wData != null ? wData.StartTime : Random.Range(3, 5), wData != null ? wData.Duration : Random.Range(4, 5));
                }
                break;
        }
    }

    private void WeatherActivation(double futureStartTime, double futureEndTime)
    {
        GameClock clock = GameManager.Instance.GameClock;

        WeatherForecastTriggered = true;
        WeatherStartTime.SetClock(clock.Time + futureStartTime, clock.Day);
        WeatherEndTime.SetClock(WeatherStartTime.Time + futureEndTime, WeatherStartTime.Day);
        SetStormyWeather();
        Debug.LogWarning($"INCOMING STORM AT {WeatherStartTime.Time}!! Ends AT {WeatherEndTime.Time}");
    }

    private WeatherData GetWeatherData()
    {
        GameClock clock = GameManager.Instance.GameClock;
        return GameDataManager.Instance.WeatherData.Where(m => m.Week == GameManager.Instance.CurrentMission.CurrentWeek && m.Day == clock.Day && m.Time == clock.Time).FirstOrDefault();
    }

    private bool SameDayAsMission()
    {
        GameClock clock = GameManager.Instance.GameClock;
        foreach (var wData in GameDataManager.Instance.WeatherData)
        {
            if (wData.Week == GameManager.Instance.CurrentMission.CurrentWeek && wData.Day == clock.Day)
            {
                return true;
            }
        }
        return false;
    }

    public void OverrideWeatherActivation(int futureStartTime, int futureEndTime)
    {
        WeatherActivation(futureStartTime, futureEndTime);
    }

    public void OnOverride(WeatherType weatherType, int futureStartTime, int futureEndTime)
    {
        WeatherActivation(futureStartTime, futureEndTime);
    }

    private void SetWeather(double time)
    {
        if (time >= 21 || time < 6)
        {
            WeatherType = WeatherType.NIGHT;
        }
        else if (time >= 6)
        {
            WeatherType = WeatherType.DAY;
        }

        WeatherForecastActive?.Invoke(WeatherType, WeatherStartTime, WeatherEndTime);
    }

    private void SetStormyWeather()
    {
        //pick weather type depending on current environment
        CurrentWeatherGO = Instantiate(RainResource) as GameObject;
        CurrentWeatherGO.SetActive(false);
        WeatherType = WeatherType.PRERAIN;
        DayNightCycle.SetFutureSkyBox(WeatherType);
        WeatherForecastActive?.Invoke(WeatherType, WeatherStartTime, WeatherEndTime);
        SoundManager.Instance.PlayOneShotSfx("Thunder_SFX", 1f, 30);
    }

    public bool IsStormy()
    {
        return WeatherType != WeatherType.DAY && WeatherType != WeatherType.NIGHT && WeatherType != WeatherType.PRERAIN; 
    }

    public bool IsNormal()
    {
        return WeatherType == WeatherType.DAY || WeatherType == WeatherType.NIGHT;
    }

    private void MissionComplete(bool complete)
    {
        ResetWeather();
    }

    public void ResetWeather()
    {
        if(CurrentWeatherGO != null)
        {
            CurrentWeatherGO.GetComponent<StormyWeather>().StopStorm();
        }

        WeatherForecastTriggered = false;
        SetWeather(GameManager.Instance.GameClock.Time);
        DayNightCycle.SetFutureSkyBox(WeatherType);
    }

    private void OnDisable()
    {
        GameClock.Ticked -= TriggerWeatherForecast;
        GameManager.MissionBegin -= MissionBegin;
        MissionManager.MissionComplete -= MissionComplete;
    }
}
