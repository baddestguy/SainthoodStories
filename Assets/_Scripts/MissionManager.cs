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
        UI.Instance.RefreshPoints(CharityPoints, FaithPoints);
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
        FaithPoints = Mathf.Clamp(FaithPoints + amount, 0, 100);
        UI.Instance.RefreshPoints(CharityPoints, FaithPoints);
    }

    public void UpdateCharityPoints(int amount, InteractableHouse house)
    {
        CharityPoints = Mathf.Clamp(CharityPoints + amount, 0, 100);
        UI.Instance.RefreshPoints(CharityPoints, FaithPoints);

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
        Player.LockMovement = true;
        MissionOver = true;

        UI.Instance.CrossFade(1, 1f);
        SoundManager.Instance.EndAllTracks();
        yield return new WaitForSeconds(5f);

        if (FaithPoints < 30 || CharityPoints < 30)
        {
            if (FaithPoints < 30)
            {
                EventsManager.Instance.AddEventToList(CustomEventType.SPIRITUALCRISIS);
            }
            if (CharityPoints < 30)
            {
                EventsManager.Instance.AddEventToList(CustomEventType.RIOTS);
            }

            //Game Over, Restart Week!
        }
        else
        {
            if (FaithPoints >= 75)
            {
                EventsManager.Instance.AddEventToList(CustomEventType.ICON);
                SaintsManager.Instance.UnlockSaint();
            }
            else
            {
                EventsManager.Instance.AddEventToList(CustomEventType.TRYHARDER_FAITH);
            }
            if (CharityPoints >= 75)
            {
                EventsManager.Instance.AddEventToList(CustomEventType.DONATION);
                TreasuryManager.Instance.DonateMoney(Random.Range(200, 250));
            }
            else
            {
                EventsManager.Instance.AddEventToList(CustomEventType.TRYHARDER_CHARITY);
                EventsManager.Instance.AddEventToList(CustomEventType.DONATION);
                TreasuryManager.Instance.DonateMoney(Random.Range(75, 100));
            }
            CurrentMission.CurrentWeek++;
        }

        EventsManager.Instance.ExecuteEvents();
        FaithPoints = 15;
        CharityPoints = 15;
        GameManager.Instance.Player.ResetEnergy();
        GameManager.Instance.GameClock.SetClock(6, 1);
        InventoryManager.Instance.ClearProvisions();
        SaveDataManager.Instance.SaveGame();
        MissionComplete?.Invoke(true);
    }

    private void OnDisable()
    {
        GameClock.StartNewDay -= NewDay;
    }
}
