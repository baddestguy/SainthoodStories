using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class Player : MonoBehaviour
{
    public static event UnityAction OnMoveSuccessEvent;
    public MapGenerator MapGenerator;
    
    private int Energy; //Create class
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
    }

    public void OnMove(MapTile newTile)
    {
        Debug.Log("CHECKING: " + newTile.TileData.Id);
        //Debug.Log("CHECKING: " + newTile);
        //Debug.Log("CHECKING: " + AdjacentTiles);
        //Debug.Log("CHECKING: " + AdjacentTiles.Count());

        if (CurrentTile == newTile) return;
        if (!AdjacentTiles.Contains(newTile)) return;
        if (newTile.TileData.TileType == TileType.BARRIER) return;
        
        CurrentTile = newTile;
        Energy--; //Subtract by modifiers
        AdjacentTiles = MapGenerator.GetAdjacentTiles(CurrentTile);
        Debug.LogWarning("MOVED TO: " + CurrentTile.TileData.Id);
        OnMoveSuccessEvent?.Invoke();
    }
}
