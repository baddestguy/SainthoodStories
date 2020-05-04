using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MissionManager : MonoBehaviour
{
    public Mission CurrentMission;
    public static UnityAction<bool> MissionComplete;
    public static UnityAction EndOfDay;

    public Dictionary<TileType, int> HouseScores;
    public int TownPoints { get; private set; }
    public int FaithPoints { get; private set; }

    private void Start()
    {
        Energy.EnergyConsumed += OnEnergyConsumed;
        GameClock.Ticked += OnTicked;

        HouseScores = new Dictionary<TileType, int>();
    }

    public void LoadAllMissions()
    {
        //Load all Missions from File!

        CurrentMission = new Mission(75, 75, 20, 0);
        TownPoints = CurrentMission.StartingTownPoints;
        FaithPoints = CurrentMission.StartingFaithPoints;
        UI.Instance.RefreshPoints(TownPoints, FaithPoints);
    }

    public void MissionUpdate(MapTile tile)
    {

    }

    private void OnTicked(int time, int day)
    {
        if (GameClock.EndofDay)
        {
            UI.Instance.ReportDisplay.text += "DAY REPORT" + "\n\n";

            EndOfDay?.Invoke();
            UI.Instance.RefreshPoints(TownPoints, FaithPoints);
        }
    }

    public void UpdateFaithPoints(int amount)
    {
        FaithPoints = Mathf.Clamp(FaithPoints + amount, 0, 100);
    }

    public void UpdateTownPoints(int amount, InteractableHouse house)
    {
        TownPoints = Mathf.Clamp(TownPoints + amount, 0, 100);
        HouseScores[house.TileType] = amount;

        UI.Instance.DisplayReport(house.TileType + " : " + amount);
        Debug.Log(house.TileType + " : " + amount);
    }

    private void OnEnergyConsumed(Energy energy)
    {
        if (energy.Depleted())
        {
            MissionComplete?.Invoke(false);
            //Reset Player back to Church.
            //Fast forward to next day at 5am.
            //Reset Energy to 20.
        }
    }

    private void OnDisable()
    {
        Energy.EnergyConsumed -= OnEnergyConsumed;
        GameClock.Ticked -= OnTicked;
    }
}
