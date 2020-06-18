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

        switch (GameManager.MissionDifficulty)
        {
            case MissionDifficulty.NORMAL:
                if (Random.Range(0, 100) < 1)
                {
                    WeatherForecastTriggered = true;
                    WeatherStartTime.SetClock(time + Random.Range(4, 7), day);
                    WeatherEndTime.SetClock(WeatherStartTime.Time + Random.Range(2, 3), WeatherStartTime.Day);
                    SetStormyWeather();
                    UI.Instance.DisplayMessage($"INCOMING STORM IN {WeatherStartTime} hours!!");
                    Debug.LogWarning($"INCOMING STORM AT {WeatherStartTime.Time}!! Ends AT {WeatherEndTime.Time}");
                }

                break;
            case MissionDifficulty.HARD:
                if (Random.Range(0, 100) < 20)
                {
                    WeatherForecastTriggered = true;
                    WeatherStartTime.SetClock(time + Random.Range(3, 5), day);
                    WeatherEndTime.SetClock(WeatherStartTime.Time + Random.Range(4, 5), WeatherStartTime.Day);
                    SetStormyWeather();
                    UI.Instance.DisplayMessage($"INCOMING STORM IN {WeatherStartTime} hours!!");
                    Debug.LogWarning($"INCOMING STORM AT {WeatherStartTime.Time}!! Ends AT {WeatherEndTime.Time}");
                }

                break;
        }
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
        WeatherType = WeatherType.DAY;
        WeatherForecastTriggered = false;
    }

    private void OnDisable()
    {
        GameClock.Ticked -= TriggerWeatherForecast;
        GameManager.MissionBegin -= MissionBegin;
        MissionManager.MissionComplete -= MissionComplete;
    }
}
