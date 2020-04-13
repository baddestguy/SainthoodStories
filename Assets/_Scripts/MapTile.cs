using System;
using System.Collections;
using System.Collections.Generic;
using EventCallbacks;
using UnityEngine;
using UnityEngine.Events;

public class MapTile : MonoBehaviour
{
    private SpriteRenderer SpriteRenderer;
    public TileData TileData { get; set; }

    [NonSerialized]
    public bool active;
    public static event UnityAction<MapTile> OnClickEvent;


    private void Awake(){
        SpriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    private void OnEnable(){
     //   OnTouchComplete.RegisterListener(ResetTile);
    }

    private void OnDisable(){
    //    OnTouchComplete.UnregisterListener(ResetTile);
    }

    public void Init(TileData tileData, Sprite [] sprites, int sortingOrder = 0) {
        TileData = tileData;
        if(TileData.TileId >= 0) {
            SpriteRenderer.sprite = sprites[TileData.TileId];
            SpriteRenderer.sortingOrder = sortingOrder;
        }
    }

    public void OnClick()
    {
        OnClickEvent?.Invoke(this);
    }

    public void OnRelease()
    {

    }

    public void OnMouseDown(){
        OnClickEvent?.Invoke(this);
        //OnTileEnter tileEnter = new OnTileEnter();
        //tileEnter.FireEvent();
        //gameObject.SetActive(false);
        //active = true;
    }

    public void OnMouseExit(){
        //OnTileExit tileExit = new OnTileExit();
        //tileExit.FireEvent();
    }

    private void ResetTile(OnTouchComplete touchComplete){
        Debug.Log("I reset");
        gameObject.SetActive(true);
        SpriteRenderer.color = Color.white;
        active = false;
    }
}
