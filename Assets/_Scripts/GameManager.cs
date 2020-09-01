using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public static UnityAction<Mission> MissionBegin;

    public MissionManager MissionManager;
    [HideInInspector]
    public MapGenerator MapGenerator;
    [HideInInspector]
    public GameMap Map;
    public Player Player;
    public GameClock GameClock;
    public static MissionDifficulty MissionDifficulty;

    public Mission CurrentMission;
    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        SceneManager.sceneLoaded += OnLevelLoaded;
        MapTile.OnClickEvent += OnTap;
        Player.OnMoveSuccessEvent += OnPlayerMoved;
        GameClock.Ticked += PlayAmbience;
        SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
    }

    private void OnLevelLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
        if (scene.name.Contains("Level"))
        {
            MissionManager.MissionOver = false;
            Instantiate(Resources.Load("UI/UI") as GameObject);
            Player = FindObjectOfType<Player>();
            MissionManager.LoadAllMissions(CurrentMission);
            GameClock = new GameClock(MissionManager.CurrentMission.StartingClock);
            Player.GameStart(MissionManager.CurrentMission);
            MissionBegin?.Invoke(MissionManager.CurrentMission);
            UI.Instance.InitTimeEnergy(GameClock, MissionManager.CurrentMission.StartingEnergy);
            UI.Instance.ShowWeekBeginText();
            PlayAmbience(GameClock.Time, GameClock.Day);
        }
    }

    private void OnMapGenerated()
    {
    }

    private void OnTap(MapTile tile)
    {
        Player.OnInteract(tile);
    }

    private void OnPlayerMoved(Energy energy, MapTile tile)
    {
        //Trigger Status effects if any
    }

    public void PassTime()
    {
        GameClock.Tick();
    }

    public void PlayAmbience(double time, int day)
    {
        if (WeatherManager.Instance.IsStormy()) return;

        if (GameClock.Time >= 21 || GameClock.Time < 6)
        {
            SoundManager.Instance.PlayAmbience("SummerNightAmbience");
        }
        else if (GameClock.Time >= 6)
        {
            SoundManager.Instance.PlayAmbience("SummerDayAmbience");
        }
    }

    public void SetMissionParameters(MissionDifficulty missionDifficulty)
    {
        switch (missionDifficulty)
        {
            //case MissionDifficulty.EASY: 
            //    CurrentMission = new Mission(75, 75, 30, 5.5, 1); 
            //    SceneManager.LoadScene("NormalLevel", LoadSceneMode.Single);
            //    break;
            //case MissionDifficulty.NORMAL: 
            //    CurrentMission = new Mission(50, 50, 20, 5.5, 7); 
            //    SceneManager.LoadScene("NormalLevel", LoadSceneMode.Single);
            //    break;
            case MissionDifficulty.HARD:
                CurrentMission = new Mission(30, 30, 20, 5.5, 7, 1);
              //  CurrentMission = new Mission(90, 90, 20, 22.5, 1, 1); //Test Mission
                TreasuryManager.Instance.DonateMoney(100);
                SoundManager.Instance.PlayOneShotSfx("StartGame", 1f, 10);
                StartCoroutine(WaitAndLoadScene());
                break;
        }

        MissionDifficulty = missionDifficulty;
    }

    private IEnumerator WaitAndLoadScene()
    {
        UI.Instance.CrossFade(1f);
        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene("NormalLevel", LoadSceneMode.Single);
    }

    public void SaveGame()
    {

    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnLevelLoaded;
        MapTile.OnClickEvent -= OnTap;
        Player.OnMoveSuccessEvent -= OnPlayerMoved;
        GameClock.Ticked -= PlayAmbience;
    }
}
