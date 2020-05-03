using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MissionManager : MonoBehaviour
{
    public Mission CurrentMission;
    public static UnityAction<bool> MissionComplete;
    
    private int CurrentTargetHits;
    private List<MapTile> Houses;

    private void Start()
    {
        Energy.EnergyConsumed += OnEnergyConsumed;
        Houses = new List<MapTile>();
    }

    public void LoadAllMissions()
    {
        //Load all Missions from File!

        CurrentMission = new Mission(2, 20, 6);
    }

    public void MissionUpdate(MapTile tile)
    {
        switch (tile.TileType)
        {
            case TileType.SHELTER:
                if (!Houses.Contains(tile))
                {
                    if ((CurrentTargetHits + 1) == CurrentMission.TargetNumber)
                    {
                    //    MissionComplete?.Invoke(true);
                    }
                    else
                    {
                        CurrentTargetHits++;
                        Houses.Add(tile);
                    }
                }
                break;
        }
    }

    private void OnEnergyConsumed(Energy energy)
    {
        if (energy.Depleted())
        {
            MissionComplete?.Invoke(false);
        }
    }
}
