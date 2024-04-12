using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMap : MonoBehaviour
{
    public int Columns;
    public int Rows;
    public GameObject MapTilesParent;
    public List<MapTile> MapTiles = new List<MapTile>();
    public List<InteractableObject> Interactables = new List<InteractableObject>();

    private void Start()
    {
    //    MapTiles.Clear();
    //    var tiles = MapTilesParent.GetComponentsInChildren<MapTile>();
    }

    public Dictionary<PlayerFacingDirection, MapTile> GetAdjacentTiles(MapTile mapTile)
    {
        var list = new Dictionary<PlayerFacingDirection, MapTile>();

        for (int i = 0; i < MapTiles.Count; i++)
        {
            if (mapTile.GetInstanceID() == MapTiles[i].GetInstanceID())
            {
                var tile = (i - Columns) < 0 ? null : MapTiles[i - Columns];
                if (tile != null && tile.TileType != TileType.BARRIER)
                {
                    list.Add(PlayerFacingDirection.UP, tile);
                }
                tile = (i - 1) < 0 || (i) % Rows == 0 ? null : MapTiles[i - 1];
                if (tile != null && tile.TileType != TileType.BARRIER)
                {
                    list.Add(PlayerFacingDirection.LEFT, tile);
                }
                tile = (i + 1) >= MapTiles.Count || (i + 1) % Rows == 0 ? null : MapTiles[i + 1];
                if (tile != null && tile.TileType != TileType.BARRIER)
                {
                    list.Add(PlayerFacingDirection.RIGHT, tile);
                }
                tile = (i + Columns) >= MapTiles.Count ? null : MapTiles[i + Columns];
                if (tile != null && tile.TileType != TileType.BARRIER)
                {
                    list.Add(PlayerFacingDirection.DOWN, tile);
                }
                return list;
            }
        }
        return null;
    }
}
