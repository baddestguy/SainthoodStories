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

    public static UnityAction<GameClock, GameClock> WeatherForecastActive;

    private void Awake()
    {
        Instance = this;   
        MissionManager.MissionComplete += MissionComplete;
    }

    void Start()
    {
        SceneManager.sceneLoaded += OnLevelLoaded;
        GameClock.Ticked += TriggerWeatherForecast;
        RainResource = Resources.Load("Weather/Rain");
        WeatherStartTime = new GameClock(0);
        WeatherEndTime = new GameClock(0);
    }

    private void OnLevelLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
        WeatherType = WeatherType.NONE;
        DayNightCycle = FindObjectOfType<DayNightCycle>();
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
                    WeatherType = WeatherType.NONE;
                    DayNightCycle.SetFutureSkyBox(WeatherType);
                    WeatherForecastActive?.Invoke(WeatherStartTime, WeatherEndTime);
                }
            }
            else
            {
                if (GameManager.Instance.GameClock == WeatherStartTime)
                {
                    CurrentWeatherGO.SetActive(true);
                    CurrentWeatherGO.GetComponent<StormyWeather>().StartStorm();
                    WeatherType = WeatherType.RAIN;
                    WeatherForecastActive?.Invoke(WeatherStartTime, WeatherEndTime);
                }
            }

            return;
        } 

        switch (GameManager.MissionDifficulty)
        {
            case MissionDifficulty.NORMAL:
                if (Random.Range(0, 100) < 1)
                {
                    WeatherForecastTriggered = true;
                    WeatherStartTime.SetClock(time + Random.Range(4, 7), day);
                    WeatherEndTime.SetClock(WeatherStartTime.Time + Random.Range(2, 3), WeatherStartTime.Day);
                    SetWeather();
                    UI.Instance.DisplayMessage($"INCOMING STORM IN {WeatherStartTime} hours!!");
                    Debug.LogWarning($"INCOMING STORM AT {WeatherStartTime.Time}!! Ends AT {WeatherEndTime.Time}");
                }

                break;
            case MissionDifficulty.HARD:
                if (Random.Range(0, 100) < 2)
                {
                    WeatherForecastTriggered = true;
                    WeatherStartTime.SetClock(time + Random.Range(3, 5), day);
                    WeatherEndTime.SetClock(WeatherStartTime.Time + Random.Range(4, 5), WeatherStartTime.Day);
                    SetWeather();
                    UI.Instance.DisplayMessage($"INCOMING STORM IN {WeatherStartTime} hours!!");
                    Debug.LogWarning($"INCOMING STORM AT {WeatherStartTime.Time}!! Ends AT {WeatherEndTime.Time}");
                }

                break;
        }
    }

    private void SetWeather()
    {
        //pick weather type depending on current environment
        CurrentWeatherGO = Instantiate(RainResource) as GameObject;
        CurrentWeatherGO.SetActive(false);
        WeatherForecastActive?.Invoke(WeatherStartTime, WeatherEndTime);
        WeatherType = WeatherType.PRESTORM;
        DayNightCycle.SetFutureSkyBox(WeatherType);
    }

    public bool IsStormy()
    {
        return WeatherType != WeatherType.NONE && WeatherType != WeatherType.PRESTORM; 
    }

    private void MissionComplete(bool complete)
    {
        WeatherType = WeatherType.NONE;
        WeatherForecastTriggered = false;
    }

    private void OnDisable()
    {
        GameClock.Ticked -= TriggerWeatherForecast;
        MissionManager.MissionComplete -= MissionComplete;
        SceneManager.sceneLoaded -= OnLevelLoaded;
    }
}
