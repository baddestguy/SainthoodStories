using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets._Scripts.Xbox;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class MissionManager : MonoBehaviour
{
    public static MissionManager Instance { get; private set; }
    public Mission CurrentMission;
    public static UnityAction<bool> MissionComplete;
    public static UnityAction EndOfDay;
    public static bool MissionOver;
    public List<CollectibleObjectivesData> CurrentCollectibleObjectives;
    public int CurrentCollectibleCounter = 0;
    public int CurrentCollectibleMissionId = 0;
    public int CurrentMissionId = 0;

    public Dictionary<TileType, int> HouseScores;
    public int CharityPoints { get; private set; }
    public int CharityPointsPool { get; private set; }
    public int FaithPoints { get; private set; }
    public int FaithPointsPool { get; private set; }
    public int FaithPointsPermanentlyLost;

    public bool SleptEarly = false;

    public static int TOTAL_FP_TARGET = 100;
    public static int TOTAL_CP_TARGET = 100;
    public ObjectivesData CurrentObjective { get { return CurrentMissionId <= GameDataManager.MAX_MISSION_ID ? GameDataManager.Instance.ObjectivesData[CurrentMissionId] : null; } }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        GameClock.EndDay += EndDay;

        HouseScores = new Dictionary<TileType, int>();
    }

    void OnDisable()
    {
        GameClock.EndDay -= EndDay;
    }

    public void LoadAllMissions(Mission mission)
    {
        //Load all Missions from File!

        CurrentMission = mission;
        CharityPoints = CurrentMission.StartingCharityPoints;
        CharityPointsPool = CurrentMission.CharityPointsPool;
        FaithPoints = CurrentMission.StartingFaithPoints;
        FaithPointsPool = CurrentMission.FaithPointsPool;
        UI.Instance.RefreshFP(FaithPointsPool);
        UI.Instance.RefreshCP(0, CharityPointsPool);
    }

    public void MissionsBegin()
    {
        var saveData = GameManager.Instance.SaveData;
        CurrentMissionId = saveData.CurrentMissionId == 0 ? 1 : saveData.CurrentMissionId;

        CurrentCollectibleMissionId = saveData.CurrentCollectibleMissionId == 0 ? 1 : saveData.CurrentCollectibleMissionId;
        CurrentCollectibleCounter = saveData.CurrentCollectibleCounter;
        FaithPointsPermanentlyLost = saveData.FaithPointsPermanentlyLost;

        Debug.Log("CURRENT MISSION: " + CurrentMissionId);
    }

    public void Collect(string item)
    {
        CurrentCollectibleCounter++;
        InventoryManager.Instance.AddCollectible(item);
    }

    public void EndDay(bool sleptEarly = false)
    {
        SleptEarly = sleptEarly;
        GridCollectibleManager.Instance.ClearAll();
        if (GameSettings.Instance.TUTORIAL_MODE)
        {
            GameManager.Instance.RefreshStage("InteractableHospital");

            return;
        }
        StartCoroutine(NewDayAsync());
    }

    private IEnumerator NewDayAsync()
    {
        yield return new WaitForSeconds(0.5f);
        EndOfDay?.Invoke();
        EndMission();
    }

    public void UpdateFaithPoints(int amount)
    {
        if (MissionOver) return;
        FaithPointsPool = Mathf.Clamp(FaithPointsPool + amount, -100, 5 - FaithPointsPermanentlyLost);

        if(UI.Instance != null)
        {
            UI.Instance.RefreshFP(FaithPointsPool);
        }
    }

    public void UpdateCharityPoints(int amount, InteractableHouse house)
    {
        if (MissionOver) return;
        CharityPointsPool = Mathf.Clamp(CharityPointsPool + amount, -100, 5); 
        if(UI.Instance != null)
        {
            UI.Instance.RefreshCP(amount, CharityPointsPool);
        }

        if (house == null) return;
        HouseScores[house.TileType] = amount;

        if(amount < 0 && house.BuildingState != BuildingState.RUBBLE)
        {
            UI.Instance.BuildingAlertPush(house.GetType().Name);
        }
    }

    public void EndMission()
    {
        StartCoroutine(EndMissionAsync());
    }

    private IEnumerator EndMissionAsync()
    {
        CheckAchievements();
        int fp = FaithPoints;
        int fpPool = FaithPointsPool;
        int fpTarget = GameDataManager.Instance.GetNextSaintUnlockThreshold();
        int cp = CharityPoints;
        int cpPool = CharityPointsPool;
        var newSaint = UnlockSaints();
        int oldMissionId = CurrentMissionId;

        CurrentMissionId++;
        InteractableHouse.HazardCounter = 0;
        EventsManager.Instance.ExecuteEvents();
        GameManager.Instance.Player.ResetEnergy();
        GameManager.Instance.Player.StatusEffects.Clear();
        InventoryManager.Instance.GeneratedProvisions.Clear();
        EventsManager.Instance.DailyEvent = CustomEventType.NONE;
        GameManager.Instance.GameClock.Reset();
        InventoryManager.HasChosenProvision = false;
        FaithPoints += FaithPointsPool;
        CharityPoints += CharityPointsPool;
        FaithPointsPool = 0;
        CharityPointsPool = 0;
        FaithPointsPermanentlyLost = 0;
        GameManager.Instance.ScrambleMapTiles();
        GameManager.Instance.CurrentBuilding = "InteractableChurch";

        //if any unresolved building hazards exist, penalize the player
        foreach (var house in GameManager.Instance.Houses)
        {
            if (house.BuildingState == BuildingState.HAZARDOUS)
            {
               // UpdateCharityPoints(-3, null);
                house.BuildingState = BuildingState.NORMAL;
            }
        }

        //if final mission, don't save (they can replay the last day if they quit the app at this point. And the save is getting wiped after this point anyway
        //Also, force story mode to true so they must see the ending story even if they have the toggle off
        if (oldMissionId == GameDataManager.MAX_MISSION_ID)
        {
            GameSettings.Instance.CustomEventsToggle = true;
        }
        else
        {
            SaveDataManager.Instance.SaveGame();
        }

        //Wait for all xbox saves to catch up before going into the house
        if (GameSettings.Instance.IsXboxMode)
        {
            while (!XboxUserHandler.Instance.SavedDataHandler.SaveQueue.IsEmpty || XboxUserHandler.Instance.SavedDataHandler.IsProcessingAsyncSave)
            {
                Debug.LogWarning("Waiting for all saves to complete");
                yield return new WaitForSeconds(0.3f);
            }
        }

        ToolTipManager.Instance.ShowToolTip("");
        bool missionFailed = false;
        Player.LockMovement = true;
        while (UI.Instance.CrossFading || EventsManager.Instance.EventInProgress || EventsManager.Instance.HasEventsInQueue()) yield return null;

        MissionOver = true;
        UI.Instance.EnableAllUIElements(false);
        UI.Instance.GameOver();
        UI.Instance.CrossFade(1, 1f);
        SoundManager.Instance.EndAllTracks();
        yield return new WaitForSeconds(5f);

        EndWeekSequence seq = FindAnyObjectByType<EndWeekSequence>();
        yield return seq.RunSequenceAsync(fp, fpPool, fpTarget, cp, cpPool, newSaint, oldMissionId);


        if (GameSettings.Instance.DEMO_MODE_3 && oldMissionId == 3)
        {
            EventsManager.Instance.AddEventToList(CustomEventType.ENDGAME_DEMO);
            SaveDataManager.Instance.DeleteProgress();
            SoundManager.Instance.EndAllTracks();
            EventsManager.Instance.ExecuteEvents();

            while (EventsManager.Instance.HasEventsInQueue()) yield return null;

            GameManager.Instance.LoadScene("MainMenu", LoadSceneMode.Single);
            yield break;
        }

        //If we finished the final mission
        if (oldMissionId == GameDataManager.MAX_MISSION_ID)
        {
            SteamManager.Instance.UnlockAchievement("FINISHED");
            XboxUserHandler.Instance.UnlockAchievement("12");
            if (FaithPoints < 80 || CharityPoints < 80)
            {
                if (FaithPoints < 80 && CharityPoints < 80)
                {
                    EventsManager.Instance.AddEventToList(CustomEventType.WORST_ENDING);
                }
                else if (FaithPoints < 80)
                {
                    EventsManager.Instance.AddEventToList(CustomEventType.SPIRITUALCRISIS);
                }
                else if (CharityPoints < 80)
                {
                    EventsManager.Instance.AddEventToList(CustomEventType.RIOTS);
                }

                SaveDataManager.Instance.DeleteProgress();
                SoundManager.Instance.EndAllTracks();
                EventsManager.Instance.ExecuteEvents();

                while (EventsManager.Instance.HasEventsInQueue()) yield return null;

                GameManager.Instance.LoadScene("MainMenu", LoadSceneMode.Single);
                yield break;
            }
            else
            {
                EventsManager.Instance.AddEventToList(CustomEventType.ENDGAME);
                var houses = GameManager.Instance.Houses;
                bool hasAtLeastOneEnding = false;
                foreach (var house in houses)
                {
                    if (house.AllObjectivesComplete)
                    {
                        hasAtLeastOneEnding = true;
                        EventsManager.Instance.AddEventToList(house.GetEndGameStory());
                    }
                }

                if (hasAtLeastOneEnding)
                {
                    EventsManager.Instance.AddEventToList(CustomEventType.ENDGAME_BEST);
                    SteamManager.Instance.UnlockAchievement("CANONIZED");
                    XboxUserHandler.Instance.UnlockAchievement("2");
                }
                else
                {
                    EventsManager.Instance.AddEventToList(CustomEventType.ENDGAME_NORMAL);
                }

                SaveDataManager.Instance.DeleteProgress();
                SoundManager.Instance.EndAllTracks();
                EventsManager.Instance.ExecuteEvents();

                while (EventsManager.Instance.HasEventsInQueue()) yield return null;

                GameManager.Instance.LoadScene("MainMenu", LoadSceneMode.Single);
                yield break;

            }
        }

        //Wait for all xbox saves to catch up before going into the house
        if (GameSettings.Instance.IsXboxMode)
        {
            while (!XboxUserHandler.Instance.SavedDataHandler.SaveQueue.IsEmpty || XboxUserHandler.Instance.SavedDataHandler.IsProcessingAsyncSave)
            {
                Debug.LogWarning("Waiting for all saves to complete");
                yield return new WaitForSeconds(0.3f);
            }
        }


        MissionComplete?.Invoke(missionFailed);
    }

    public IEnumerable<SaintData> UnlockSaints()
    {
        if (GameSettings.Instance.DEMO_MODE_3 && SaintsManager.Instance.UnlockedSaints.Count >= 3) return new List<SaintData>();

        var saintsUnlocked = new List<SaintData>();

        Debug.Log((FaithPoints + FaithPointsPool) + " / " + GameDataManager.Instance.GetNextSaintUnlockThreshold());

        if ((FaithPoints + FaithPointsPool) >= GameDataManager.Instance.GetNextSaintUnlockThreshold())
        {
            var newSaint = SaintsManager.Instance.UnlockSaint();
            if(newSaint != null)
            {
                saintsUnlocked.Add(newSaint);
                XboxUserHandler.Instance.UnlockAchievement("18", (SaintsManager.Instance.UnlockedSaints.Count*100 / GameDataManager.TOTAL_UNLOCKABLE_SAINTS));

                if(SaintsManager.Instance.UnlockedSaints.Count >= GameDataManager.TOTAL_UNLOCKABLE_SAINTS)
                {
                    SteamManager.Instance.UnlockAchievement("LITANY");
                }
            }
        }

        return saintsUnlocked;
    }

    public void OverrideMission(int missionId)
    {
        CurrentMissionId = missionId;
        WeatherManager.Instance.ResetWeather();
        GameManager.Instance.GameClock.Reset(missionId);
        SaveDataManager.Instance.SaveGame();
        MissionsBegin();
        GameManager.Instance.ReloadLevel();
    }

    public void OverrideHouseMission(HouseType houseType, int missionId)
    {
        var houseName = houseType.ToString();
        InteractableHouse house = null;

        switch (houseName)
        {
            case "InteractableChurch":
                house = FindAnyObjectByType<InteractableChurch>();
                break;
            case "InteractableHospital":
                house = FindAnyObjectByType<InteractableHospital>();
                break;
            case "InteractableKitchen":
                house = FindAnyObjectByType<InteractableKitchen>();
                break;
            case "InteractableOrphanage":
                house = FindAnyObjectByType<InteractableOrphanage>();
                break;
            case "InteractableShelter":
                house = FindAnyObjectByType<InteractableShelter>();
                break;
            case "InteractableSchool":
                house = FindAnyObjectByType<InteractableSchool>();
                break;
            case "InteractableClothesBank":
                house = FindAnyObjectByType<InteractableClothesBank>();
                break;
        }

        if(house != null )
        {
            house.CurrentMissionId = missionId;
            WeatherManager.Instance.ResetWeather();
            GameManager.Instance.GameClock.Reset(missionId);
            SaveDataManager.Instance.SaveGame();
            MissionsBegin();
            GameManager.Instance.ReloadLevel();
        }
    }

    public void OverideCP(int cp)
    {
        UpdateCharityPoints(cp, null);
    }

    public void OverideFP(int fp)
    {
        UpdateFaithPoints(fp);
    }

    public bool FirstWeek()
    {
        return CurrentMission.CurrentWeek == 1;
    }

    public void CheckAchievements()
    {
        XboxUserHandler.Instance.UnlockAchievement("1");
        SteamManager.Instance.UnlockAchievement("GETTING_STARTED");
        if(CurrentMissionId == 16) {
            XboxUserHandler.Instance.UnlockAchievement("3");
            SteamManager.Instance.UnlockAchievement("MIDWAY");
        }
    }
}
