using System.Linq;
using UnityEngine;
using UnityEngine.Events;

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
        InteractableHouse.OnEnterHouse += OnEnterHouse;
        WeatherStartTime = new GameClock(0);
        WeatherEndTime = new GameClock(0);
    }

    private void MissionBegin(Mission mission)
    {
        RainResource = Resources.Load("Weather/Rain");
        WeatherStartTime = new GameClock(0);
        WeatherEndTime = new GameClock(0);
        DayNightCycle = FindObjectOfType<DayNightCycle>();
        BroadcastWeather();
        UI.Instance.WeatherAlert(WeatherType, WeatherStartTime, WeatherEndTime);
    }

    private void TriggerWeatherForecast(double time, int day)
    {
        if (!GameClock.DeltaTime) return;

        if (WeatherForecastTriggered)
        {
            if (GameManager.Instance.GameClock == WeatherEndTime)
            {
                if(CurrentWeatherGO != null && CurrentWeatherGO.GetComponent<StormyWeather>() != null)
                {
                    CurrentWeatherGO.GetComponent<StormyWeather>().StopStorm();
                }
                WeatherForecastTriggered = false;
                WeatherType = WeatherType.NONE;
                BroadcastWeather();
                DayNightCycle.SetFutureSkyBox(WeatherType);
                SoundManager.Instance.PlayWeatherAmbience(false);
            }
            else if (GameManager.Instance.GameClock == WeatherStartTime)
            {
                SetWeatherType();
                if(CurrentWeatherGO != null)
                {
                    CurrentWeatherGO?.SetActive(true);
                    CurrentWeatherGO?.GetComponent<StormyWeather>()?.StartStorm();
                }
                SoundManager.Instance.PlayWeatherAmbience(true);
                WeatherForecastActive?.Invoke(WeatherType, WeatherStartTime, WeatherEndTime);
            }
            return;
        }
        else
        {
            BroadcastWeather();
        }

        //HACK: Don't trigger weather before tutorials
        if (GameManager.Instance.MissionManager.CurrentMission.CurrentWeek == 1 && day < 3) return;
        if (GameManager.Instance.MissionManager.CurrentMission.CurrentWeek == 2 && day < 2) return;
        if (GameManager.Instance.MissionManager.CurrentMission.CurrentWeek == 3 && day < 2) return;

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
                    if (MissionManager.Instance.CurrentMission.Season == Season.SUMMER && !GameManager.Instance.GameClock.DuringTheDay()) break;
                    
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

    public void BroadcastWeather()
    {
        WeatherForecastActive?.Invoke(WeatherType, WeatherStartTime, WeatherEndTime);
    }

    private void SetStormyWeather()
    {
        SetPreStormWeather();
        WeatherForecastActive?.Invoke(WeatherType, WeatherStartTime, WeatherEndTime);
    }

    private void SetPreStormWeather()
    {
        switch (MissionManager.Instance.CurrentMission.Season)
        {
            case Season.SPRING:
            case Season.FALL:
            case Season.WINTER:
                SoundManager.Instance.PlayOneShotSfx("Thunder_SFX", 1f, 30);
                WeatherType = WeatherType.PRESTORM;
                break;
            case Season.SUMMER:
                WeatherType = WeatherType.PREHEAT;
                break;
        }
        DayNightCycle.SetFutureSkyBox(WeatherType);
    }

    private void SetWeatherType()
    {
        switch (MissionManager.Instance.CurrentMission.Season)
        {
            case Season.SPRING:
                WeatherType = WeatherType.HAIL;
                break;
            case Season.SUMMER:
                WeatherType = WeatherType.HEATWAVE;
                break;
            case Season.FALL:
                CurrentWeatherGO = Instantiate(RainResource) as GameObject;
                WeatherType = WeatherType.RAIN;
                break;
            case Season.WINTER:
                WeatherType = WeatherType.SNOW;
                break;
        }
        DayNightCycle.SetFutureSkyBox(WeatherType);
    }

    public bool IsStormy()
    {
        return WeatherType != WeatherType.NONE && WeatherType != WeatherType.PRESTORM && WeatherType != WeatherType.PREHEAT; 
    }

    public bool IsNormal()
    {
        return WeatherType == WeatherType.NONE;
    }

    private void MissionComplete(bool complete)
    {
        ResetWeather();
    }

    public void OnEnterHouse(bool inHouse)
    {
        if (IsStormy())
        {
            SoundManager.Instance.PlayWeatherAmbience(true);
        }
    }

    public void ResetWeather()
    {
        if(CurrentWeatherGO != null)
        {
            CurrentWeatherGO.GetComponent<StormyWeather>().StopStorm();
            Destroy(CurrentWeatherGO);
            CurrentWeatherGO = null;
        }
        WeatherType = WeatherType.NONE;
        WeatherForecastTriggered = false;
        BroadcastWeather();
        DayNightCycle.SetFutureSkyBox(WeatherType);
    }

    private void OnDisable()
    {
        InteractableHouse.OnEnterHouse -= OnEnterHouse;
        GameClock.Ticked -= TriggerWeatherForecast;
        GameManager.MissionBegin -= MissionBegin;
        MissionManager.MissionComplete -= MissionComplete;
    }
}
