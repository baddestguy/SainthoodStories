using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    public ObjectivesData CurrentObjective { get { return GameDataManager.Instance.ObjectivesData[CurrentMissionId]; } }

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

    public void RestartMission()
    {
        UI.Instance.TriggerGameOver();
        SaveDataManager.Instance.LoadDaySave();
        MissionsBegin();
        GameManager.Instance.ReloadLevel();
    }

    public void EndDay()
    {
        GridCollectibleManager.Instance.ClearAll();
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

        ToolTipManager.Instance.ShowToolTip("");
        bool missionFailed = false;
        Player.LockMovement = true;
        while (UI.Instance.CrossFading || EventsManager.Instance.EventInProgress) yield return null;

        //if any unresolved building hazards exist, penalize the player
        foreach (var house in GameManager.Instance.Houses)
        {
            if (house.BuildingState == BuildingState.HAZARDOUS)
            {
                UpdateCharityPoints(-3, null);
                house.BuildingState = BuildingState.NORMAL;
            }
        }
        MissionOver = true;
        UI.Instance.EnableAllUIElements(false);
        UI.Instance.GameOver();
        UI.Instance.CrossFade(1, 1f);
        SoundManager.Instance.EndAllTracks();
        yield return new WaitForSeconds(5f);

        EndWeekSequence seq = FindObjectOfType<EndWeekSequence>();
        yield return seq.RunSequenceAsync();


        if (GameSettings.Instance.DEMO_MODE_2 && CurrentMissionId == 3)
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
        if (CurrentMissionId == GameDataManager.MAX_MISSION_ID)
        {
            FaithPoints += FaithPointsPool;
            CharityPoints += CharityPointsPool;

            if (FaithPoints < 75 || CharityPoints < 75)
            {
                if (FaithPoints < 75)
                {
                    EventsManager.Instance.AddEventToList(CustomEventType.SPIRITUALCRISIS);

                }
                if (CharityPoints < 75)
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
                    if (house.AllObjectivesComplete || (house is InteractableChurch && InventoryManager.Instance.Collectibles.Count >= 100))
                    {
                        hasAtLeastOneEnding = true;
                        EventsManager.Instance.AddEventToList(house.GetEndGameStory());
                    }
                }

                if (hasAtLeastOneEnding)
                {
                    EventsManager.Instance.AddEventToList(CustomEventType.ENDGAME_BEST);
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
        SaveDataManager.Instance.SaveGame();
        SaveDataManager.Instance.DaySave();
        MissionComplete?.Invoke(missionFailed);
    }

    public IEnumerable<SaintData> UnlockSaints()
    {
        if (GameSettings.Instance.DEMO_MODE && SaintsManager.Instance.UnlockedSaints.Count >= 3) return new List<SaintData>();

        var saintsUnlocked = new List<SaintData>();

        Debug.Log((FaithPoints + FaithPointsPool) + " / " + GameDataManager.Instance.GetNextSaintUnlockThreshold());

        if ((FaithPoints + FaithPointsPool) >= GameDataManager.Instance.GetNextSaintUnlockThreshold())
        {
            var newSaint = SaintsManager.Instance.UnlockSaint();
            if(newSaint != null)
            {
                saintsUnlocked.Add(newSaint);
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
}
