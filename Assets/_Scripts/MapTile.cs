using System;
using EventCallbacks;
using UnityEngine;
using UnityEngine.Events;

public class MapTile : MonoBehaviour
{
    public TileType TileType;
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
        GameClock.Ticked += Tick;
        GameManager.MissionBegin += MissionBegin;
        Player.OnMoveSuccessEvent += OnPlayerMoved;
    }

    private void OnDisable(){
        //    OnTouchComplete.UnregisterListener(ResetTile);
        GameClock.Ticked -= Tick;
        GameManager.MissionBegin -= MissionBegin;
        Player.OnMoveSuccessEvent -= OnPlayerMoved;
    }

    public void Init(TileData tileData, Sprite [] sprites, int sortingOrder = 0) {
        TileData = tileData;
        if(TileData.TileSpriteId >= 0) {
            SpriteRenderer.sprite = sprites[TileData.TileSpriteId];
            SpriteRenderer.sortingOrder = sortingOrder;
        }
    }

    public void OnRelease()
    {

    }

    public void OnMouseUp(){
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

    public static InteractableObject GetInteractableObject(TileData tileData, GameObject tileGameObject, MapTile groundTile, Sprite [] sprites, int sortingOrder)
    {

        switch (tileData.TileType)
        {
            case TileType.HOUSE:
                var house = tileGameObject.AddComponent<InteractableHouse>();
                house.Init(UnityEngine.Random.Range(15, 19), groundTile, tileData, sprites, sortingOrder); //TODO: HACK! Read from file!
                return house;

            case TileType.BANDIT:
                var enemy = tileGameObject.AddComponent<InteractableEnemy>();
                enemy.Init(groundTile, tileData, sprites, sortingOrder);
                return enemy;
        }

        var interac = tileGameObject.AddComponent<InteractableObject>();
        interac.Init(tileData, sprites, sortingOrder);
        return interac;
    }

    public virtual void Tick(int time)
    {

    }

    public virtual void MissionBegin(Mission mission)
    {

    }

    public virtual void OnPlayerMoved(Energy energy, MapTile tile)
    {

    }
}
