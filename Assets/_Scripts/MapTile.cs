using System;
using EventCallbacks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MapTile : MonoBehaviour
{
    public TileType TileType;
    private SpriteRenderer SpriteRenderer;
    public TileData TileData { get; set; }

    [NonSerialized]
    public bool active;
    public static event UnityAction<MapTile> OnClickEvent;

    [SerializeField]
    private Image GridTile;

    public GamepadCursor GamepadCursor;

    private void Awake(){
        SpriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    public virtual void OnEnable(){
        GameClock.Ticked += Tick;
        GameManager.MissionBegin += MissionBegin;
        Player.OnMoveSuccessEvent += OnPlayerMoved;
        TooltipMouseOver.OnHover += HoverExit;
        GamepadCursor = FindObjectOfType<GamepadCursor>();
    }

    public virtual void OnDisable(){
        GameClock.Ticked -= Tick;
        GameManager.MissionBegin -= MissionBegin;
        Player.OnMoveSuccessEvent -= OnPlayerMoved;
        TooltipMouseOver.OnHover -= HoverExit;
    }

    public void Init(TileData tileData, Sprite [] sprites, int sortingOrder = 0) {
        TileData = tileData;
        if(TileData.TileSpriteId >= 0) {
            SpriteRenderer.sprite = sprites[TileData.TileSpriteId];
            SpriteRenderer.sortingOrder = sortingOrder;
        }
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

    public virtual void OnMouseOver()
    {
        if (GamepadCursor != null && GamepadCursor.PlayerInput.currentControlScheme == "Gamepad") return;

        Hover();
    }

    public virtual void OnMouseExit()
    {
        HoverExit();
    }

    public virtual void Hover()
    {
        if (TooltipMouseOver.IsHovering) return;
        if (EventsManager.Instance.EventInProgress) return;
        if (GridTile == null || !CameraControls.ZoomComplete) return;

        Color c;

        if(GameManager.Instance.Player.WeCanMove(this))
        {
            c = Color.green;
            c.a = 0.1f;
            GridTile.color = c;
            GridTile.gameObject.SetActive(true);
            ToolTipManager.Instance.ShowToolTip("Tooltip_MoveHere", GameDataManager.Instance.GetToolTip(TooltipStatId.MOVE, energyModifier: -GameManager.Instance.Player.ModifyEnergyConsumption(this, true)));
        }
        else if(!InteractableHouse.HouseUIActive)
        {
            c = Color.white;
            c.a = 0.1f;
            GridTile.color = c;
            GridTile.gameObject.SetActive(true);
        }
    }

    public virtual void HoverExit()
    {
        if (TooltipMouseOver.IsHovering) return;
        if (GridTile == null) return;
        GridTile.gameObject.SetActive(false);
        ToolTipManager.Instance.ShowToolTip("");
    }

    public virtual void Click()
    {
        //Make a check if a ui was click..
        if (UI.Instance.WasUiHit) return;

        if (EventSystem.current.currentSelectedGameObject != null) return;
        if (!CameraControls.CameraMove && !CameraControls.CameraZoom)
            OnClickEvent?.Invoke(this);
    }

    private void OnMouseUpAsButton()
    {
        if (InteractableHouse.InsideHouse) return;

        Click();
    }
}
