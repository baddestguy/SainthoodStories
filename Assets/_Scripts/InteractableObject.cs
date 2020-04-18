using System.Collections.Generic;
using UnityEngine;

public class InteractableObject : MapTile
{
    public MapTile CurrentGroundTile;
    

    public virtual void Init(MapTile groundTile, TileData tileData, Sprite[] sprites, int sortingOrder = 0)
    {
        CurrentGroundTile = groundTile;
        Init(tileData, sprites, sortingOrder);
    }

}
