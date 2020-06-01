using System;
using EventCallbacks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

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

    public virtual void OnEnable(){
        GameClock.Ticked += Tick;
        GameManager.MissionBegin += MissionBegin;
        Player.OnMoveSuccessEvent += OnPlayerMoved;
    }

    public virtual void OnDisable(){
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

    public void OnMouseUpAsButton(){
        if (EventSystem.current.currentSelectedGameObject != null) return;
        if(!CameraControls.CameraMove && !CameraControls.CameraZoom)
            OnClickEvent?.Invoke(this);
    }

    public static InteractableObject GetInteractableObject(TileData tileData, GameObject tileGameObject, MapTile groundTile, Sprite [] sprites, int sortingOrder)
    {

        switch (tileData.TileType)
        {
            case TileType.SHELTER:
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

    public virtual void Tick(double time, int day)
    {

    }

    public virtual void MissionBegin(Mission mission)
    {

    }

    public virtual void OnPlayerMoved(Energy energy, MapTile tile)
    {

    }
}
