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
    public static UnityAction StartNewDay;
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
        GameClock.Ticked += OnTicked;

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

    private void OnTicked(double time, int day)
    {
        if (GameClock.EndofDay)
        {
         //   UI.Instance.ReportDisplay.text += "DAY REPORT" + "\n\n";

            EndOfDay?.Invoke();
            
            if(day > CurrentMission.TotalDays)
            {
                EndMission();
            }
            else
            {
                StartNewDay?.Invoke();
                InventoryManager.Instance.GenerateProvisionsForNewDay();
            }
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

        if(amount < 0)
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
        yield return new WaitForSeconds(5f);

        if (FaithPoints < 30 || CharityPoints < 30)
        {
            if (FaithPoints < 30)
            {
                EventsManager.Instance.AddEventToList(EventType.SPIRITUALCRISIS);
            }
            if (CharityPoints < 30)
            {
                EventsManager.Instance.AddEventToList(EventType.RIOTS);
            }
        }
        else
        {
            if (FaithPoints > 75)
            {
                EventsManager.Instance.AddEventToList(EventType.ICON);
            }
            if (CharityPoints > 75)
            {
                EventsManager.Instance.AddEventToList(EventType.DONATION);
            }
        }
        EventsManager.Instance.ExecuteEvents();
        MissionComplete?.Invoke(true);
    }

    private void OnDisable()
    {
        GameClock.Ticked -= OnTicked;
    }
}
