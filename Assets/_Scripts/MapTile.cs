using System;
using System.Collections;
using System.Collections.Generic;
using EventCallbacks;
using UnityEngine;

public class MapTile : MonoBehaviour
{
    private SpriteRenderer SpriteRenderer;
    private TileData TileData { get; set; }

    [NonSerialized]
    public bool active;
    private void Awake(){
        SpriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    private void OnEnable(){
        OnTouchComplete.RegisterListener(ResetTile);
    }

    private void OnDisable(){
        OnTouchComplete.UnregisterListener(ResetTile);
    }

    public void LoadSprite(TileData tileData, Sprite [] sprites, int sortingOrder = 0) {
        TileData = tileData;
        if(TileData.TileId >= 0) {
            SpriteRenderer.sprite = sprites[TileData.TileId];
            SpriteRenderer.sortingOrder = sortingOrder;
        }
    }

    public void OnMouseEnter(){
        OnTileEnter tileEnter = new OnTileEnter();
        tileEnter.FireEvent();
        gameObject.SetActive(false);
        active = true;
    }

    public void OnMouseExit(){
        OnTileExit tileExit = new OnTileExit();
        tileExit.FireEvent();
    }

    private void ResetTile(OnTouchComplete touchComplete){
        Debug.Log("I reset");
        gameObject.SetActive(true);
        SpriteRenderer.color = Color.white;
        active = false;
    }
}
