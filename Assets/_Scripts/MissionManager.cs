using System;
using System.Collections;
using System.Collections.Generic;
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
        GameClock.EndDay += NewDay;

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

    private void NewDay()
    {
        if (!GameClock.DeltaTime) return;

        SoundManager.Instance.PlayOneShotSfx("StartGame_SFX", 1f, 10);
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
        FaithPoints = Mathf.Clamp(FaithPoints + amount, -100, 5);
        UI.Instance.RefreshFP(FaithPoints);

        //if (FaithPoints <= 0)
        //{
        //    EndMission();
        //    return;
        //}
    }

    public void UpdateCharityPoints(int amount, InteractableHouse house)
    {
        if (MissionOver) return;
        CharityPoints = Mathf.Clamp(CharityPoints + amount, -100, 5);
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

        if (FaithPoints <= 0 || CharityPoints <= 0)
        {
            missionFailed = true;
            //instant game over
            if (CharityPoints <= 0)
            {
                EventsManager.Instance.AddEventToList(CustomEventType.RIOTS);
            }
            if (FaithPoints <= 0)
            {
                EventsManager.Instance.AddEventToList(CustomEventType.SPIRITUALCRISIS);
            }
            SaveDataManager.Instance.DeleteProgress();
            SoundManager.Instance.EndAllTracks();
            EventsManager.Instance.ExecuteEvents();

            while (EventsManager.Instance.HasEventsInQueue()) yield return null;

            GameManager.Instance.LoadScene("MainMenu", LoadSceneMode.Single);
            yield break;
            //Game Over, Restart Week!
        }
        else
        {
            EndWeekSequence seq = FindObjectOfType<EndWeekSequence>();
            yield return seq.RunSequenceAsync();
            if (GameManager.Instance.GameClock.EndofWeek())
            {
                if(CurrentMission.Season == Season.WINTER)
                {
                    //BONUS: GIVE THE CHOICE TO ASCEND TO HEAVEN OR STAY AND CONTINUE TO HELP!

                    EventsManager.Instance.AddEventToList(CustomEventType.ENDGAME);
                    EventsManager.Instance.ExecuteEvents();
                    SaveDataManager.Instance.DeleteProgress();
                    while (EventsManager.Instance.HasEventsInQueue()) yield return null;

                    GameManager.Instance.LoadScene("MainMenu", LoadSceneMode.Single);
                    yield break;
                }
                CurrentMission.CurrentWeek++;
                GameManager.Instance.GameClock.EndTheWeek();
            }
        }

        EventsManager.Instance.ExecuteEvents();
        GameManager.Instance.Player.ResetEnergy();
        GameManager.Instance.Player.StatusEffects.Clear();
        InventoryManager.Instance.GeneratedProvisions.Clear();
        EventsManager.Instance.DailyEvent = CustomEventType.NONE;
        SaveDataManager.Instance.SaveGame();
        MissionComplete?.Invoke(missionFailed);
    }

    public IEnumerable<SaintData> UnlockSaints()
    {
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
        GameClock.EndDay -= NewDay;
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
