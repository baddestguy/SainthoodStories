using System.Collections.Generic;
using UnityEngine;

public class InteractableObject : MapTile
{
    public MapTile CurrentGroundTile;
    private IEnumerable<MapTile> AdjacentTiles;

    public void Init(MapTile groundTile, TileData tileData, Sprite[] sprites, int sortingOrder = 0)
    {
        CurrentGroundTile = groundTile;
        base.Init(tileData, sprites, sortingOrder);
    }
}
