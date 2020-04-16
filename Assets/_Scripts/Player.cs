using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class Player : MonoBehaviour
{
    public static event UnityAction<Energy> OnMoveSuccessEvent;
    public MapGenerator MapGenerator;
    
    private Energy Energy;
    private int EnergyConsumption;
    private MapTile CurrentTile;
    private IEnumerable<MapTile> AdjacentTiles;

    void Start()
    {
        MapTile.OnClickEvent += OnMove;
    }

    public void GameStart(Mission missionDetails)
    {
        Energy = missionDetails.StartingEnergy;
        CurrentTile = missionDetails.StartingTile;
        AdjacentTiles = MapGenerator.GetAdjacentTiles(CurrentTile);
        ModifyEnergyConsumption(CurrentTile.TileData);
    }

    public void OnMove(MapTile newTile)
    {
        Debug.Log("CHECKING: " + newTile.TileData.Id);

        if (CurrentTile == newTile) return;
        if (!AdjacentTiles.Contains(newTile)) return;
        if (newTile.TileData.TileType == TileType.BARRIER) return;
        
        CurrentTile = newTile;

        ModifyEnergyConsumption(CurrentTile.TileData);
        Energy.Consume(EnergyConsumption); //Subtract by modifiers

        AdjacentTiles = MapGenerator.GetAdjacentTiles(CurrentTile);
        
        Debug.LogWarning("MOVED TO: " + CurrentTile.TileData.Id);
        OnMoveSuccessEvent?.Invoke(Energy);
    }

    public void ModifyEnergyConsumption(TileData tile)
    {
        EnergyConsumption = 1; //tile.EnergyConsumption
    }
}
