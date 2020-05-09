using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MissionManager : MonoBehaviour
{
    public Mission CurrentMission;
    public static UnityAction<bool> MissionComplete;
    public static UnityAction EndOfDay;

    public Dictionary<TileType, int> HouseScores;
    public int CharityPoints { get; private set; }
    public int FaithPoints { get; private set; }

    private void Start()
    {
        GameClock.Ticked += OnTicked;

        HouseScores = new Dictionary<TileType, int>();
    }

    public void LoadAllMissions()
    {
        //Load all Missions from File!

        CurrentMission = new Mission(75, 75, 25, 0);
        CharityPoints = CurrentMission.StartingCharityPoints;
        FaithPoints = CurrentMission.StartingFaithPoints;
        UI.Instance.RefreshPoints(CharityPoints, FaithPoints);
    }

    public void MissionUpdate(MapTile tile)
    {

    }

    private void OnTicked(double time, int day)
    {
        if (GameClock.EndofDay)
        {
            UI.Instance.ReportDisplay.text += "DAY REPORT" + "\n\n";

            EndOfDay?.Invoke();
            UI.Instance.RefreshPoints(CharityPoints, FaithPoints);
        }
    }

    public void UpdateFaithPoints(int amount)
    {
        FaithPoints = Mathf.Clamp(FaithPoints + amount, 0, 100);
    }

    public void UpdateCharityPoints(int amount, InteractableHouse house)
    {
        CharityPoints = Mathf.Clamp(CharityPoints + amount, 0, 100);
        HouseScores[house.TileType] = amount;

        UI.Instance.DisplayReport(house.TileType + " : " + amount);
        Debug.Log(house.TileType + " : " + amount);
    }

    private void OnDisable()
    {
        GameClock.Ticked -= OnTicked;
    }
}
