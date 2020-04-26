using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMap : MonoBehaviour
{
    public int Columns;
    public int Rows;
    public List<MapTile> MapTiles = new List<MapTile>();
    public List<InteractableObject> Interactables = new List<InteractableObject>();

    public IEnumerable<MapTile> GetAdjacentTiles(MapTile mapTile)
    {
        var currentPosition = -1;
        var list = new List<MapTile>();

        for (int i = 0; i < MapTiles.Count; i++)
        {
            if (mapTile.GetInstanceID() == MapTiles[i].GetInstanceID())
            {
                currentPosition = i;
                var tile = (i - Columns) < 0 ? null : MapTiles[i - Columns];
                if (tile != null && tile.TileType != TileType.BARRIER)
                {
                    list.Add(tile);
                }
                tile = (i - 1) < 0 ? null : MapTiles[i - 1];
                if (tile != null && tile.TileType != TileType.BARRIER)
                {
                    list.Add(tile);
                }
                tile = (i + 1) >= MapTiles.Count ? null : MapTiles[i + 1];
                if (tile != null && tile.TileType != TileType.BARRIER)
                {
                    list.Add(tile);
                }
                tile = (i + Columns) >= MapTiles.Count ? null : MapTiles[i + Columns];
                if (tile != null && tile.TileType != TileType.BARRIER)
                {
                    list.Add(tile);
                }
                return list;
            }
        }
        return null;
    }
}
