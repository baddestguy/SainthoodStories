using System.Collections.Generic;
using UnityEngine;

public class MissionManager : MonoBehaviour
{
    public Mission CurrentMission;
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

        CurrentMission = new Mission(2, 15, 1);
    }

    public void MissionUpdate(MapTile tile)
    {
        switch (tile.TileData.TileType)
        {
            case TileType.HOUSE:
                if (!Houses.Contains(tile))
                {
                    if ((CurrentTargetHits + 1) == CurrentMission.TargetNumber)
                    {
                        tile.gameObject.SetActive(false); //TODO: TEMPORARY!
                        Debug.LogError("Mission COMPLETE! WOHOO!!");
                    }
                    else
                    {
                        Debug.Log("GOT ONE!");
                        CurrentTargetHits++;
                        Houses.Add(tile);
                        tile.gameObject.SetActive(false); //TODO: TEMPORARY!
                    }
                }
                break;
        }
    }

    private void OnEnergyConsumed(Energy energy)
    {
        if (energy.Depleted())
        {
            Debug.LogError("GAME OVER!! YOU FAILED!!");
        }
    }
}
