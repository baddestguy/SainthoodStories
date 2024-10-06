using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Assets._Scripts.Extensions;
using Assets._Scripts.Xbox;
using Rewired;
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
    SaintsShowcase_Day,
    WorldMap
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
    public int PlayerEnergy;
    public GameClock GameClock;
    public static MissionDifficulty MissionDifficulty;
    public SaveObject SaveData;

    public Mission CurrentMission;
    public InteractableHouse CurrentHouse;
    public string CurrentBuilding;
    private Scene activeScene;
    public SceneID CurrentSceneID;
    public SceneID PreviousSceneID;
    public bool InGameSession;
    public bool SceneLoaded;

    public int RunAttempts;
    public int[] MaptileIndexes;// = new int[14] {0, 4, 7, 9, 13, 16, 23, 40, 47, 50, 53, 56, 60, 63};

    public InteractableHouse[] Houses;
    public Dictionary<string, BuildingState> HouseStates = new Dictionary<string, BuildingState>();
    public List<string> WorldCollectibles = new List<string>();

    public AsyncOperation LoadingOperation;

    [HideInInspector]
    public bool PlayerHasLoggedIn { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        MaptileIndexes = new int[14] { 0, 4, 7, 9, 13, 16, 23, 40, 47, 50, 53, 56, 60, 63 };
        SceneManager.sceneLoaded += OnLevelLoaded;
        MapTile.OnClickEvent += OnMapTileTap;
        Player.OnMoveSuccessEvent += OnPlayerMoved;
        GameClock.Ticked += OnTick;
    }

    public void PlayerLoginSuccess()
    {
        StartCoroutine(CompletePlayerLoginProcess());
    }

    private IEnumerator CompletePlayerLoginProcess()
    {
        while (SoundManager.Instance == null)
        {
            //Wait for SoundManager if not initialized...
            yield return null;
        }

        GameSettings.Instance.BeginLoad();
        SoundManager.Instance.PlayOneShotSfx("StartGame_SFX", 1f, 10);

        LoadScene("MainMenu", LoadSceneMode.Single);
        yield return null;

        PlayerHasLoggedIn = true;
    }

    private void Update()
    {
    }

    public void ExitHouse()
    {
        GetBuildingStates();
        PlayerEnergy = Player.Energy.Amount;
        //LoadScene("WorldMap", LoadSceneMode.Single);
    }

    public void LoadScene(string sceneName, LoadSceneMode mode = LoadSceneMode.Additive)
    {
        SceneLoaded = false;
        Scene scene = SceneManager.GetSceneByName(sceneName);
        if (!scene.isLoaded)
        {
            StartCoroutine(WaitAndLoadScene(sceneName));
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
        Time.timeScale = 1f;
        if (loadSceneMode == LoadSceneMode.Single)
        {
            activeScene = scene;
        }

        if (scene.IsGameLevel())
        {
            PreviousSceneID = CurrentSceneID;
            CurrentSceneID = CurrentMission.SeasonSceneId;
            Instantiate(Resources.Load("UI/UI"));
            EventsManager.Instance.LoadTriggeredMissionEvents(SaveData.MissionEvents);
            MissionManager.MissionOver = false;
            Player = FindObjectOfType<Player>();
            Map = FindObjectOfType<GameMap>();
            MissionManager.LoadAllMissions(CurrentMission);
            GameClock = new GameClock(SaveData.Time, SaveData.Day);

            if (PreviousSceneID == SceneID.SaintsShowcase_Day || PreviousSceneID == SceneID.WorldMap)
            {
                UI.Instance.ShowWeekBeginText("");
            }
            else
            {
                UI.Instance.ShowDayBeginText("");
            }

            if (SaveData.Maptiles == null)
            {
                ScrambleMapTiles();
            }
            else
            {
                MaptileIndexes = SaveData.Maptiles;
            }

            InteractableHouse.HouseTriggeredEvent = SaveData.HouseTriggeredEvent;
            Player.GameStart(CurrentMission);
            MissionBegin?.Invoke(CurrentMission);
            UI.Instance.InitTimeEnergy(GameClock, MissionManager.CurrentMission.StartingEnergy);
            PlayAmbience(GameClock.Time, GameClock.Day);
            TreasuryManager.Instance.Money = SaveData.Money;
            InventoryManager.Instance.LoadInventory(SaveData);
            Houses = FindObjectsOfType<InteractableHouse>();
            if (GameClock.Time == 5)
            {
                GameClock.StartNewDay?.Invoke();
            }

            var obj = MissionManager.Instance.CurrentObjective;
            if (obj != null && (obj.WeatherId == (int)WeatherId.RAIN || obj.WeatherId == (int)WeatherId.BLIZZARD))
            {
                WeatherManager.Instance.OverrideWeatherActivation(0, 1);
            }

            if (obj != null && obj.DailyEvent != CustomEventType.NONE)
            {
                var security = InventoryManager.Instance.GetProvision(Provision.SECURITY_GUARDS);
                if (security != null && obj.DailyEvent == CustomEventType.VANDALISM)
                {
                    obj.DailyEvent = CustomEventType.VANDALISM_STOPPED;
                }
                EventsManager.Instance.DailyEvent = obj.DailyEvent;
                EventsManager.Instance.AddEventToList(obj.DailyEvent);
                EventsManager.Instance.TriggeredMissionEvents.Add(obj.DailyEvent);
            }

            GridCollectibleManager.Instance.ClearAll();
            if (GameSettings.Instance.TUTORIAL_MODE)
            {
                Player.Energy.OnOveride(4);
            }
        }
        else if (scene.IsMenu())
        {
            GamepadCursor.CursorSpeed = 2000f;

            PreviousSceneID = CurrentSceneID;
            CurrentSceneID = SceneID.MainMenu;
            SaveDataManager.Instance.LoadGame((data, newGame) =>
            {
                //CAREFUL! GAMEDATAMANAGER HAS NOT BEEN LOADED YET!
                SaveData = data;
                RunAttempts = data.RunAttempts;

                TutorialManager.Instance.CurrentTutorialStep = data.TutorialSteps;
                if (data.TutorialSteps >= 15) GameSettings.Instance.FTUE = false;
                if (data.RunAttempts > 0)
                {
                    TutorialManager.Instance.SkipTutorial = true;
                }

                UI.Instance.DisplayRunAttempts();

                if (data.Maptiles == null)
                {
                    UI.Instance.DisableMainMenuContinueBtn();
                }

                SaintsManager.Instance.LoadSaints(SaveData.Saints);

                //    EventsManager.Instance.CurrentEvents.Add(data.CurrentDailyEvent);
            }, false, true);
            InGameSession = false;
            SoundManager.Instance.PlayAmbience("SummerDay_Ambience");
            SoundManager.Instance.PlayMusic("MainMenu_Music", loopDelay: 70);
            GameSettings.Instance.TUTORIAL_MODE = false;
            TutorialManager.Instance.Steps.Clear();
        }
        else if (scene.IsSaintShowcase())
        {
            PreviousSceneID = CurrentSceneID;
            CurrentSceneID = SceneID.SaintsShowcase_Day;
        }
        else if (scene.IsPauseMenu())
        {
        }
        else if (scene.IsWorldMap())
        {
            PreviousSceneID = CurrentSceneID;
            CurrentSceneID = SceneID.WorldMap;
            var obj = MissionManager.Instance.CurrentObjective;
            if (obj != null)
            {
                WeatherManager.Instance.ChangeWeather(obj.WeatherId);
            }
            else
            {
                int[] randomWeather = new int[] { 6, 10, 22, 24 };
                WeatherManager.Instance.ChangeWeather(randomWeather[Random.Range(0, randomWeather.Length - 1)]);
            }
        }

        SceneLoaded = true;
    }

    public void GetBuildingStates()
    {
        foreach (var h in Houses)
        {
            HouseStates[h.GetType().Name] = h.BuildingState;
        }
    }

    public void OnMapTileTap(MapTile tile)
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

    private void OnTick(double time, int day)
    {
        PlayAmbience(time, day);
        if (GameClock.DeltaTime && time % 2 == 0)
        {
            foreach (var h in Houses)
            {
                if (h.MyObjective == null) continue;
                if (h.MyObjective.Event > BuildingEventType.URGENT)
                {
                    MissionManager.Instance.UpdateCharityPoints(-1, null);
                }
            }
        }

        if (time >= 19 || time < 6)
        {
            SoundManager.Instance.StartPlaylist(false);
        }
        else
        {
            SoundManager.Instance.StartPlaylist();
        }
    }

    public void PlayAmbience(double time, int day)
    {
        if (MissionManager.MissionOver) return;
        if (WeatherManager.Instance.IsStormy()) return;

        if (GameClock.Time >= 19 || GameClock.Time < 6)
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
            case MissionDifficulty.HARD:

                SaveDataManager.Instance.LoadGame((data, aNewGame) =>
                {

                    if (aNewGame)
                    {
                        RunAttempts++;
                        SaveDataManager.Instance.DeleteProgress();
                        TutorialManager.Instance.CurrentTutorialStep = data.TutorialSteps;
                    }
                    SaveData = data;
                    Debug.Log("Run Attempts: " + RunAttempts);
                    CurrentMission = new Mission(SaveData.FP, SaveData.FPPool, SaveData.CP, SaveData.CPPool, SaveData.Energy, SaveData.Time, SaveData.Day, SaveData.Week);
                    SoundManager.Instance.PlayOneShotSfx("StartGame_SFX", 1f, 10);

                    MissionManager.MissionsBegin();
                    if (GameSettings.Instance.FTUE)
                    {
                        StartCoroutine(WaitAndLoadScene("TutorialLevel"));
                    }
                    else
                    {
                        if (GameSettings.Instance.DEMO_MODE)
                        {
                            CurrentMission.OverrideSeason(Season.FALL);
                        }

                        StartCoroutine(WaitAndLoadScene(CurrentMission.SeasonLevel));
                    }

                    UI.Instance.DisplayRunAttempts();
                }, newGame, false, !activeScene.name.Contains("MainMenu"), showUI: showUI);
                break;
        }

        MissionDifficulty = missionDifficulty;
    }

    public MapTile GetNextRandomMapTile(string house)
    {
        switch (house)
        {
            case "InteractableHospital": return Player.Map.MapTiles[MaptileIndexes[0]];
            case "InteractableOrphanage": return Player.Map.MapTiles[MaptileIndexes[1]];
            case "InteractableSchool": return Player.Map.MapTiles[MaptileIndexes[2]];
            case "InteractableShelter": return Player.Map.MapTiles[MaptileIndexes[3]];
            case "InteractableKitchen": return Player.Map.MapTiles[MaptileIndexes[4]];
            case "InteractableMarket": return Player.Map.MapTiles[MaptileIndexes[5]];
                //case "InteractableChurch": 
                //    var 

        }

        return null;
    }

    public void ScrambleMapTiles()
    {
        MaptileIndexes.Shuffle();
    }

    public void ReloadLevel()
    {
        SaveDataManager.Instance.LoadGame((data, newGame) =>
        {
            CurrentMission = new Mission(data.FP, data.FPPool, data.CP, data.CPPool, data.Energy, data.Time, 7, data.Week);
            StartCoroutine(WaitAndLoadScene(CurrentMission.SeasonLevel));
            SaveData = data;
        }, false, true);

    }

    public bool HasPlayerEnergy()
    {
        return PlayerEnergy > 0 || GameSettings.Instance.InfiniteBoost;
    }

    public void UpdatePlayerEnergyFromWorld(int amount)
    {
        PlayerEnergy = Mathf.Clamp(PlayerEnergy + amount, 0, 100);
    }

    private IEnumerator WaitAndLoadScene(string sceneName)
    {
        if (UI.Instance != null)
        {
            UI.Instance.CrossFade(1f);
        }
        yield return new WaitForSeconds(1f);

        SceneLoaded = false;
        LoadingOperation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);

        if (UI.Instance != null) UI.Instance.LoadingScreenEnable(true);
        LoadingScreen loadingScreen = null;
        if (UI.Instance != null) loadingScreen = UI.Instance.LoadingScreen.GetComponent<LoadingScreen>();
        while (!LoadingOperation.isDone)
        {
            float progressValue = Mathf.Clamp01(LoadingOperation.progress / 0.9f);

            if (loadingScreen != null)
                loadingScreen.LoadingBarFill.fillAmount = progressValue;

            yield return null;
        }
        if (UI.Instance != null) UI.Instance.LoadingScreenEnable(false);
    }

    public void ClearData()
    {
        InteractableHouse.DeadlineCounter = 0;
        InteractableHouse.InsideHouse = false;
        InteractableHouse.HouseUIActive = false;
        InteractableHouse.HazardCounter = 0;
        InteractableHouse.HouseTriggeredEvent = CustomEventType.NONE;
        InteractableHouse.InsideHouse = false;
        EventsManager.Instance.ClearData();
        TutorialManager.Instance.ClearData();
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnLevelLoaded;
        MapTile.OnClickEvent -= OnMapTileTap;
        Player.OnMoveSuccessEvent -= OnPlayerMoved;
        GameClock.Ticked -= OnTick;
    }
}
