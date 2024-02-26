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
    public List<ObjectivesData> CompletedObjectives;
    public List<ObjectivesData> CurrentObjectives;
    public List<CollectibleObjectivesData> CurrentCollectibleObjectives;
    public int CurrentCollectibleCounter = 0;
    public int CurrentCollectibleMissionId = 0;
    public int CurrentMissionId = 0;

    public Dictionary<TileType, int> HouseScores;
    public int CharityPoints { get; private set; }
    public int FaithPoints { get; private set; }
    public int FaithPointsPool { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
     //   GameClock.EndDay += NewDay;

        HouseScores = new Dictionary<TileType, int>();
    }

    public void LoadAllMissions(Mission mission)
    {
        //Load all Missions from File!

        CurrentMission = mission;
        CharityPoints = CurrentMission.StartingCharityPoints;
        FaithPoints = CurrentMission.StartingFaithPoints;
        FaithPointsPool = CurrentMission.FaithPointsPool;
        UI.Instance.RefreshFP(FaithPoints);
        UI.Instance.RefreshCP(0, CharityPoints);
    }

    public void MissionsBegin()
    {
        var saveData = GameManager.Instance.SaveData;
        CompletedObjectives = saveData.CompletedObjectives?.ToList();
        for (int i = 1; i < GameDataManager.Instance.ObjectivesData.Count; i++)
        {
            var comp = CompletedObjectives?.Where(x => x.Id == i) ?? Enumerable.Empty<ObjectivesData>();
            if(CompletedObjectives == null || comp.Count() < GameDataManager.Instance.ObjectivesData[i].Count+1)
            {
                CurrentMissionId = i;
                CurrentObjectives = GameDataManager.Instance.GetObjectivesData(i).ToList();
                foreach(var c in comp)
                {
                    var done = CurrentObjectives.Remove(c);
                }
                if (!CurrentObjectives.Any())
                {
                    //TODO: SHOW POPUP TO END DAY
                    CurrentObjectives.Add(new ObjectivesData() { Id = CurrentMissionId, Event = BuildingEventType.RETURN, House = "InteractableChurch" });
                }

                break;
            }
        }

        CurrentCollectibleMissionId = saveData.CurrentCollectibleMissionId == 0 ? 1 : saveData.CurrentCollectibleMissionId;
        CurrentCollectibleCounter = saveData.CurrentCollectibleCounter;

        //if (!GameClock.DeltaTime) return;

        //SoundManager.Instance.PlayOneShotSfx("StartGame_SFX", 1f, 10);
        //StartCoroutine(NewDayAsync());
    }

    public void CompleteObjective(ObjectivesData obj)
    {
        if (obj == null) return;

        CurrentObjectives.Remove(obj);

        if (CompletedObjectives == null)
        {
            CompletedObjectives = new List<ObjectivesData>() { obj };
        }
        else
        {
            CompletedObjectives.Add(obj);
        }

        if (!CurrentObjectives.Any())
        {
            //TODO: SHOW POPUP TO END DAY
            CurrentObjectives.Add(new ObjectivesData() { Id = CurrentMissionId, Event = BuildingEventType.RETURN, House = "InteractableChurch" });
        }
    }

    public void Collect(string item)
    {
        CurrentCollectibleCounter++;
        InventoryManager.Instance.AddCollectible(item);
    }

    public void EndDay()
    {
        CurrentObjectives.Clear();
        StartCoroutine(NewDayAsync());
    }

    private IEnumerator NewDayAsync()
    {
        yield return new WaitForSeconds(0.5f);
        EndOfDay?.Invoke();
        Instance.EndMission();
    }

    public void UpdateFaithPoints(int amount)
    {
        if (MissionOver) return;
        FaithPoints = Mathf.Clamp(FaithPoints + amount, -100, 15);

        if(UI.Instance != null)
        {
            UI.Instance.RefreshFP(FaithPoints);
        }

        //if (FaithPoints <= 0)
        //{
        //    EndMission();
        //    return;
        //}
    }

    public void UpdateCharityPoints(int amount, InteractableHouse house)
    {
        if (MissionOver) return;
        CharityPoints = Mathf.Clamp(CharityPoints + amount, -100, 100);
        UI.Instance.RefreshCP(amount, CharityPoints);

        //if (CharityPoints <= 0)
        //{
        //    EndMission();
        //    return;
        //}

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
        MissionOver = true;
        UI.Instance.EnableAllUIElements(false);
        UI.Instance.GameOver();
        while (UI.Instance.CrossFading || EventsManager.Instance.EventInProgress) yield return null;
        UI.Instance.CrossFade(1, 1f);
        SoundManager.Instance.EndAllTracks();
        yield return new WaitForSeconds(5f);

        //if (FaithPoints <= 0 || CharityPoints <= 0)
        //{
        //    missionFailed = true;
        //    //instant game over
        //    if (CharityPoints <= 0)
        //    {
        //        EventsManager.Instance.AddEventToList(CustomEventType.RIOTS);
        //    }
        //    if (FaithPoints <= 0)
        //    {
        //        EventsManager.Instance.AddEventToList(CustomEventType.SPIRITUALCRISIS);
        //    }
        //    SaveDataManager.Instance.DeleteProgress();
        //    SoundManager.Instance.EndAllTracks();
        //    EventsManager.Instance.ExecuteEvents();

        //    while (EventsManager.Instance.HasEventsInQueue()) yield return null;

        //    GameManager.Instance.LoadScene("MainMenu", LoadSceneMode.Single);
        //    yield break;
        //    //Game Over, Restart Week!
        //}
        //else
        {
            EndWeekSequence seq = FindObjectOfType<EndWeekSequence>();
            yield return seq.RunSequenceAsync();
            //if (MissionOver) //TODO: Use a different condition to switch seasons
            //{
            //    if(CurrentMission.Season == Season.WINTER)
            //    {
            //        //BONUS: GIVE THE CHOICE TO ASCEND TO HEAVEN OR STAY AND CONTINUE TO HELP!

            //        EventsManager.Instance.AddEventToList(CustomEventType.ENDGAME);
            //        EventsManager.Instance.ExecuteEvents();
            //        SaveDataManager.Instance.DeleteProgress();
            //        while (EventsManager.Instance.HasEventsInQueue()) yield return null;

            //        GameManager.Instance.LoadScene("MainMenu", LoadSceneMode.Single);
            //        yield break;
            //    }
            //    CurrentMission.CurrentWeek++;
            //    GameManager.Instance.GameClock.EndTheWeek();
            //}
        }

        if (GameSettings.Instance.DEMO_MODE){
            if (CurrentMission.CurrentWeek == 1)
            {
                CurrentMission.CurrentWeek++;
                GameManager.Instance.GameClock.EndTheWeek();
                CharityPoints = 3;
                FaithPoints = 3;
            }
            else if (GameManager.Instance.GameClock.Day > 3)
            {
                EventsManager.Instance.AddEventToList(CustomEventType.ENDGAME);

                SaveDataManager.Instance.DeleteProgress();
                SoundManager.Instance.EndAllTracks();
                EventsManager.Instance.ExecuteEvents();

                while (EventsManager.Instance.HasEventsInQueue()) yield return null;

                GameManager.Instance.LoadScene("MainMenu", LoadSceneMode.Single);
                yield break;
            }
        }

        InteractableHouse.HazardCounter = 0;
        EventsManager.Instance.ExecuteEvents();
        GameManager.Instance.Player.ResetEnergy();
        GameManager.Instance.Player.StatusEffects.Clear();
        InventoryManager.Instance.GeneratedProvisions.Clear();
        EventsManager.Instance.DailyEvent = CustomEventType.NONE;
        GameManager.Instance.GameClock.Reset();
        InventoryManager.HasChosenProvision = false;
        SaveDataManager.Instance.SaveGame();
        MissionComplete?.Invoke(missionFailed);
    }

    public IEnumerable<SaintData> UnlockSaints()
    {
        if (GameSettings.Instance.DEMO_MODE && SaintsManager.Instance.UnlockedSaints.Count >= 3) return new List<SaintData>();

        var saintsUnlocked = new List<SaintData>();
        FaithPointsPool += FaithPoints;

        Debug.Log(FaithPointsPool + " / " + GameDataManager.Instance.Constants["SAINTS_UNLOCK_THRESHOLD_1"].IntValue);

        if (FaithPointsPool >= GameDataManager.Instance.Constants["SAINTS_UNLOCK_THRESHOLD_1"].IntValue)
        {
            saintsUnlocked.Add(SaintsManager.Instance.UnlockSaint());
            FaithPointsPool = 0;
        }
        //if (FaithPoints >= GameDataManager.Instance.Constants["SAINTS_UNLOCK_THRESHOLD_2"].IntValue)
        //{
        //    saintsUnlocked.Add(SaintsManager.Instance.UnlockSaint());
        //}
        //if (FaithPoints >= GameDataManager.Instance.Constants["SAINTS_UNLOCK_THRESHOLD_3"].IntValue)
        //{
        //    saintsUnlocked.Add(SaintsManager.Instance.UnlockSaint());
        //}

        return saintsUnlocked;
    }

    private void OnDisable()
    {
    }

    public void OverrideMission(int missionId)
    {
        if(CompletedObjectives != null)
        {
            CompletedObjectives.Clear();
        }

        for (int i = 1; i < missionId; i++)
        {
            for(int j = 0; j < GameDataManager.Instance.ObjectivesData[i].Count; j++)
            {
                CompleteObjective(GameDataManager.Instance.ObjectivesData[i][j]);
            }
            CurrentMissionId = i;
            CurrentObjectives = GameDataManager.Instance.GetObjectivesData(i).ToList();
            CompleteObjective(new ObjectivesData() { Id = CurrentMissionId, Event = BuildingEventType.RETURN, House = "InteractableChurch" });
        }

        foreach(var house in GameManager.Instance.Houses)
        {
            house.OverrideState(CurrentMissionId);
        }

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
