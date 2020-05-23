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
        Player.OnEnergyDepleted += GameOver;

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
            UI.Instance.ReportDisplay.text += "DAY REPORT" + "\n\n";

            EndOfDay?.Invoke();
            UI.Instance.RefreshPoints(CharityPoints, FaithPoints);
            
            if(day > CurrentMission.TotalDays)
            {
                EndMission();
            }
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

    public void EndMission()
    {
        UI.Instance.EnableEndGame(true);
        MissionComplete?.Invoke(true);
        //disable movement
        //Evaluate Mission Success/Failure
    }

    public void GameOver()
    {
        UI.Instance.EnableEndGame(true);
        MissionComplete?.Invoke(false);
    }

    private void OnDisable()
    {
        GameClock.Ticked -= OnTicked;
        Player.OnEnergyDepleted -= GameOver;
    }
}
