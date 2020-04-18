using System.Collections.Generic;
using UnityEngine;

public class InteractableEnemy : InteractableObject
{
    private IEnumerable<MapTile> AdjacentTiles;
    private MapGenerator MapGenerator;

    public override void Init(MapTile groundTile, TileData tileData, Sprite[] sprites, int sortingOrder = 0)
    {
        MapGenerator = GameManager.Instance.MapGenerator;
        AdjacentTiles = MapGenerator.GetAdjacentTiles(CurrentGroundTile);
        base.Init(groundTile, tileData, sprites, sortingOrder);
    }
}
