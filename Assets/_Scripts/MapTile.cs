using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapTile : MonoBehaviour
{
    [SerializeField]
    private SpriteRenderer SpriteRenderer = null;
    private TileData TileData { get; set; }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void LoadSprite(TileData tileData, Sprite [] sprites, int sortingOrder = 0) {
        TileData = tileData;
        if(TileData.TileId >= 0) {
            SpriteRenderer.sprite = sprites[TileData.TileId];
            SpriteRenderer.sortingOrder = sortingOrder;
        }
    }
}
