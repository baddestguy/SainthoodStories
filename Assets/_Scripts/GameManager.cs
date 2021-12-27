using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;


public enum SceneID
{
    Spring,
    Summer,
    Fall,
    Winter,
    BootLoader,
    MainMenu,
    Credits,
    NormalLevelAjust,
    WeekDaysUI,
    PauseMenu,
    SaintsShowcase_Day
}

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
    private Scene activeScene;
    public SceneID CurrentSceneID;
    public SceneID PreviousSceneID;
    public bool InGameSession;

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
        LoadScene("MainMenu", LoadSceneMode.Single);
    }


    private void Update()
    {
        if (InGameSession)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                PauseMenu.Instance.Activate();
            }
        }
    }

    public void LoadScene(string sceneName, LoadSceneMode mode = LoadSceneMode.Additive)
    {
        Scene scene = SceneManager.GetSceneByName(sceneName);
        if (!scene.isLoaded)
        {
            SceneManager.LoadScene(sceneName, mode);
        }
    }

    public void FadeAndLoadScene(string sceneName)
    {
        StartCoroutine(WaitAndLoadScene(sceneName));
    }

    public void UnloadScene(string sceneName)
    {
        Scene scene = SceneManager.GetSceneByName(sceneName);
        if (scene.isLoaded)
        {
            SceneManager.UnloadSceneAsync(scene);
        }
    }

    private void OnLevelLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
        bool loadWeekDaysScene = true;
        if (loadSceneMode == LoadSceneMode.Single)
        {
            activeScene = scene;
        }

        if (scene.name.Contains("Level"))
        {
            PreviousSceneID = CurrentSceneID;
            CurrentSceneID = CurrentMission.SeasonSceneId;

            MissionManager.MissionOver = false;
            Player = FindObjectOfType<Player>();
            Map = FindObjectOfType<GameMap>();
            MissionManager.LoadAllMissions(CurrentMission);
            GameClock = new GameClock(SaveData.Time, SaveData.Day);

            if(PreviousSceneID == SceneID.SaintsShowcase_Day)
            {
                UI.Instance.ShowWeekBeginText("");
            }
            else
            {
                if (Player.OnEnergyDepleted)
                    UI.Instance.ShowWeekBeginText(LocalizationManager.Instance.GetText("WeekIntroEnergyDepleted"));
                else
                    UI.Instance.ShowWeekBeginText($"{LocalizationManager.Instance.GetText(CurrentMission.SeasonLevel.Replace("Level", "_Splash"))}");
            }

            Player.GameStart(CurrentMission);
            MissionBegin?.Invoke(CurrentMission);
            UI.Instance.InitTimeEnergy(GameClock, MissionManager.CurrentMission.StartingEnergy);
            PlayAmbience(GameClock.Time, GameClock.Day);
            TreasuryManager.Instance.Money = SaveData.Money;
            SaintsManager.Instance.LoadSaints(SaveData.Saints);
            InventoryManager.Instance.LoadInventory(SaveData);
            LoadScene("PauseMenu", LoadSceneMode.Additive);
            SoundManager.Instance.PlayMusic("Convent_Music", "Field_Music");
        }
        else if (scene.name.Contains("MainMenu"))
        {
            PreviousSceneID = CurrentSceneID;
            CurrentSceneID = SceneID.MainMenu;
            SaveDataManager.Instance.LoadGame((data, newGame) => {
                TutorialManager.Instance.CurrentTutorialStep = data.TutorialSteps;
                if (data.TutorialSteps >= 20) GameSettings.Instance.FTUE = false;
            },false, true);
            InGameSession = false;
        //    SoundManager.Instance.PlayAmbience();
            SoundManager.Instance.PlayMusic("MainMenu_Music");

        }
        else if (scene.name.Contains(SceneID.SaintsShowcase_Day.ToString()))
        {
            LoadScene("PauseMenu", LoadSceneMode.Additive);
            loadWeekDaysScene = false;
            PreviousSceneID = CurrentSceneID;
            CurrentSceneID = SceneID.SaintsShowcase_Day;
        }else if (scene.name.Contains(SceneID.PauseMenu.ToString()))
        {
            loadWeekDaysScene = false;
        }

        if (loadWeekDaysScene)
        {
            LoadScene("WeekDaysUI");
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
            SoundManager.Instance.PlayAmbience();
        }
        else if (GameClock.Time >= 6)
        {
            SoundManager.Instance.PlayAmbience();
        }
    }

    public void SetMissionParameters(MissionDifficulty missionDifficulty, bool newGame = false, bool showUI = true)
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
                
                SaveDataManager.Instance.LoadGame((data, aNewGame) => {
                    
                    if (aNewGame)
                    {
                        SaveDataManager.Instance.DeleteSave();
                        TutorialManager.Instance.CurrentTutorialStep = 0;
                        GameSettings.Instance.FTUE = true;
                    }

                    SaveData = data;
                    CurrentMission = new Mission(SaveData.FP, SaveData.CP, SaveData.Energy, SaveData.Time, SaveData.Day, SaveData.Week);
                    SoundManager.Instance.PlayOneShotSfx("StartGame_SFX", 1f, 10);

                    StartCoroutine(WaitAndLoadScene(CurrentMission.SeasonLevel));
                }, newGame, false, !activeScene.name.Contains("MainMenu"), showUI: showUI);
                break;
        }

        MissionDifficulty = missionDifficulty;
    }



    public void ReloadLevel()
    {

        SaveDataManager.Instance.LoadGame((data, newGame) => {
            CurrentMission = new Mission(data.FP, data.CP, data.Energy, data.Time, 7, data.Week);
            StartCoroutine(WaitAndLoadScene(CurrentMission.SeasonLevel));
        },false, true);

    }

    private IEnumerator WaitAndLoadScene(string sceneName)
    {
        UI.Instance.CrossFade(1f);
        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnLevelLoaded;
        MapTile.OnClickEvent -= OnTap;
        Player.OnMoveSuccessEvent -= OnPlayerMoved;
        GameClock.Ticked -= PlayAmbience;
    }
}
