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
    public SaveObject SaveData;

    public Mission CurrentMission;
    public InteractableHouse CurrentHouse;

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
            Player = FindObjectOfType<Player>();
            Map = FindObjectOfType<GameMap>();
            MissionManager.LoadAllMissions(CurrentMission);
            GameClock = new GameClock(SaveData.Time, SaveData.Day);
            Player.GameStart(CurrentMission);
            MissionBegin?.Invoke(CurrentMission);
            UI.Instance.InitTimeEnergy(GameClock, MissionManager.CurrentMission.StartingEnergy);
            UI.Instance.ShowWeekBeginText();
            PlayAmbience(GameClock.Time, GameClock.Day);
            TreasuryManager.Instance.Money = SaveData.Money;
            SaintsManager.Instance.LoadSaints(SaveData.Saints);
            InventoryManager.Instance.LoadInventory(SaveData);
        }
        else if (scene.name.Contains("MainMenu"))
        {
            SaveData = SaveDataManager.Instance.LoadGame();
            TutorialManager.Instance.CurrentTutorialStep = SaveData.TutorialSteps;
            if (SaveData.TutorialSteps >= 20) GameSettings.Instance.FTUE = false;
        }
    }

    private void OnTap(MapTile tile)
    {
        if (Player == null) return;

        ToolTipManager.Instance.ShowToolTip("");
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
        if (MissionManager.MissionOver) return;
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

    public void SetMissionParameters(MissionDifficulty missionDifficulty, bool newGame = false)
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
                if (newGame)
                {
                    SaveDataManager.Instance.DeleteSave();
                    TutorialManager.Instance.CurrentTutorialStep = 0;
                    GameSettings.Instance.FTUE = true;
                }
                SaveData = SaveDataManager.Instance.LoadGame();
                CurrentMission = new Mission(SaveData.FP, SaveData.CP, SaveData.Energy, SaveData.Time, 7, SaveData.Week);
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

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnLevelLoaded;
        MapTile.OnClickEvent -= OnTap;
        Player.OnMoveSuccessEvent -= OnPlayerMoved;
        GameClock.Ticked -= PlayAmbience;
    }
}
