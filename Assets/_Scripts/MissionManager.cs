using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

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

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        GameClock.StartNewDay += NewDay;

        HouseScores = new Dictionary<TileType, int>();
    }

    public void LoadAllMissions(Mission mission)
    {
        //Load all Missions from File!

        CurrentMission = mission;
        CharityPoints = CurrentMission.StartingCharityPoints;
        FaithPoints = CurrentMission.StartingFaithPoints;
        UI.Instance.RefreshFP(FaithPoints);
        UI.Instance.RefreshCP(0, CharityPoints);
    }

    private void NewDay()
    {
        if (!GameClock.DeltaTime) return;

        EndOfDay?.Invoke();
            
        if(GameManager.Instance.GameClock.EndofWeek())
        {
            EndMission();
        }
    }

    public void UpdateFaithPoints(int amount)
    {
        if (MissionOver) return;
        FaithPoints = Mathf.Clamp(FaithPoints + amount, 0, 100);
        UI.Instance.RefreshFP(FaithPoints);

        if (FaithPoints <= 0)
        {
            EndMission();
            return;
        }
    }

    public void UpdateCharityPoints(int amount, InteractableHouse house)
    {
        if (MissionOver) return;
        CharityPoints = Mathf.Clamp(CharityPoints + amount, 0, 100);
        UI.Instance.RefreshCP(amount, CharityPoints);

        if (CharityPoints <= 0)
        {
            EndMission();
            return;
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
        bool missionFailed = false;
        Player.LockMovement = true;
        MissionOver = true;
        UI.Instance.CrossFade(1, 1f);
        SoundManager.Instance.EndAllTracks();
        yield return new WaitForSeconds(5f);

        if (FaithPoints < 75 || CharityPoints < 75)
        {
            missionFailed = true;
            //instant game over
            if (CharityPoints <= 0)
            {
                EventsManager.Instance.AddEventToList(CustomEventType.RIOTS);
            }
            else if (FaithPoints <= 0)
            {
                EventsManager.Instance.AddEventToList(CustomEventType.SPIRITUALCRISIS);
            }
            else
            {   //end of week game over
                if (CharityPoints < 75)
                {
                    EventsManager.Instance.AddEventToList(CustomEventType.RIOTS);
                }
                if (FaithPoints < 75)
                {
                    EventsManager.Instance.AddEventToList(CustomEventType.SPIRITUALCRISIS);
                }
            }

            //Game Over, Restart Week!
        }
        else
        {
            EndWeekSequence seq = FindObjectOfType<EndWeekSequence>();
            yield return seq.RunSequenceAsync();
            CurrentMission.CurrentWeek++;
        }

        EventsManager.Instance.ExecuteEvents();
        EventsManager.Instance.CurrentEvents.Clear();
        FaithPoints = 15;
        CharityPoints = 15;
        GameManager.Instance.Player.ResetEnergy();
        GameManager.Instance.GameClock.SetClock(6, 1);
        InventoryManager.Instance.ClearProvisions();
        SaveDataManager.Instance.SaveGame();
        MissionComplete?.Invoke(missionFailed);
    }

    public IEnumerable<SaintData> UnlockSaints()
    {
        var saintsUnlocked = new List<SaintData>();

        if (FaithPoints >= GameDataManager.Instance.Constants["SAINTS_UNLOCK_THRESHOLD_1"].IntValue)
        {
            saintsUnlocked.Add(SaintsManager.Instance.UnlockSaint());
        }
        if (FaithPoints >= GameDataManager.Instance.Constants["SAINTS_UNLOCK_THRESHOLD_2"].IntValue)
        {
            saintsUnlocked.Add(SaintsManager.Instance.UnlockSaint());
        }
        if (FaithPoints >= GameDataManager.Instance.Constants["SAINTS_UNLOCK_THRESHOLD_3"].IntValue)
        {
            saintsUnlocked.Add(SaintsManager.Instance.UnlockSaint());
        }

        return saintsUnlocked;
    }

    private void OnDisable()
    {
        GameClock.StartNewDay -= NewDay;
    }

    public void OverideCP(int cp)
    {
        UpdateCharityPoints(cp, null);
    }

    public void OverideFP(int fp)
    {
        UpdateFaithPoints(fp);
    }
}
