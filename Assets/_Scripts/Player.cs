using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class Player : MonoBehaviour
{
    public static event UnityAction<Energy, MapTile> OnMoveSuccessEvent;
    public MapGenerator MapGenerator;
    
    private Energy Energy;
    private int EnergyConsumption;
    private MapTile CurrentTile;
    private IEnumerable<MapTile> AdjacentTiles;

    void Start()
    {
        GameManager.MissionBegin += GameStart;
    }

    public void GameStart(Mission missionDetails)
    {
        Energy = missionDetails.StartingEnergy;
        CurrentTile = MapGenerator.GetPlayerStartingTile();
        AdjacentTiles = MapGenerator.GetAdjacentTiles(CurrentTile);
        ModifyEnergyConsumption(CurrentTile.TileData);
    }

    private bool WeCanMove(MapTile tile)
    {
        return (CurrentTile != tile && AdjacentTiles.Contains(tile) && tile.TileData.TileType != TileType.BARRIER);
    }

    public void OnMove(MapTile newTile)
    {        
        CurrentTile = newTile;

        EnergyConsumption = ModifyEnergyConsumption(CurrentTile.TileData);
        Energy.Consume(EnergyConsumption);

        AdjacentTiles = MapGenerator.GetAdjacentTiles(CurrentTile);
        
        Debug.LogWarning("MOVED TO: " + CurrentTile.TileData.Id);
    }

    public void OnInteract(MapTile newTile)
    {
        if(newTile is InteractableObject)
        {
            var iTile = newTile as InteractableObject;
            if (WeCanMove(iTile.CurrentGroundTile))
            {
                OnMove(iTile.CurrentGroundTile);
                OnMoveSuccessEvent?.Invoke(Energy, iTile);
            }
            else
            {
                TileDance(iTile);
            }
        }
        else
        {
            if (WeCanMove(newTile))
            {
                OnMove(newTile);
                OnMoveSuccessEvent?.Invoke(Energy, newTile);
            }
            else
            {
                TileDance(newTile);
            }
        }
    }

    public int ModifyEnergyConsumption(TileData tile)
    {
        return 1; //tile.EnergyConsumption
    }

    public void TileDance(MapTile tile)
    {
        //Trigger cute tile animation
        Debug.Log("Tile bubbly animation!");
    }

    private void OnDisable()
    {
    }
}
