using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class Player : MonoBehaviour
{
    public static event UnityAction<Energy, MapTile> OnMoveSuccessEvent;
    public GameMap Map;
    
    private Energy Energy;
    private int EnergyConsumption;
    [SerializeField]
    private MapTile CurrentTile;
    private IEnumerable<MapTile> AdjacentTiles;
    private List<PlayerItem> Inventory = new List<PlayerItem>();

    private Vector3 TargetPosition;

    void Start()
    {
        GameManager.MissionBegin += GameStart;
        TargetPosition = transform.position;
    }

    void Update()
    {
        if((transform.position - TargetPosition).magnitude > 0.01)
        {
            transform.position = Vector3.Lerp(transform.position, TargetPosition, Time.deltaTime*5);
        }
    }

    public void GameStart(Mission missionDetails)
    {
        Energy = missionDetails.StartingEnergy;
        AdjacentTiles = Map.GetAdjacentTiles(CurrentTile);
        ModifyEnergyConsumption(CurrentTile);
    }

    private bool WeCanMove(MapTile tile)
    {
        return (CurrentTile != tile && AdjacentTiles.Contains(tile) && tile.TileType != TileType.BARRIER);
    }

    private void OnMove(MapTile newTile)
    {        
        CurrentTile = newTile;

        EnergyConsumption = ModifyEnergyConsumption(CurrentTile);
        Energy.Consume(EnergyConsumption);

        AdjacentTiles = Map.GetAdjacentTiles(CurrentTile);

        TargetPosition = CurrentTile.transform.position;        
    }

    public void OnInteract(MapTile newTile)
    {
        if(newTile is InteractableObject)
        {
            var iTile = newTile as InteractableObject;
            if (WeCanMove(iTile.CurrentGroundTile))
            {
                OnMove(iTile.CurrentGroundTile);
                OnMoveSuccessEvent?.Invoke(Energy, iTile);
            }
            else
            {
                TileDance(iTile);
            }
        }
        else
        {
            if (WeCanMove(newTile))
            {
                OnMove(newTile);
                OnMoveSuccessEvent?.Invoke(Energy, newTile);
            }
            else
            {
                TileDance(newTile);
            }
        }
    }

    public int ModifyEnergyConsumption(MapTile tile)
    {
        return 1; //tile.EnergyConsumption
    }

    public void TileDance(MapTile tile)
    {
        //Trigger cute tile animation
        Debug.Log("Tile bubbly animation!");
    }

    public void AddToInventory(PlayerItem item)
    {
        if (Inventory.Any())
        {
            UI.Instance.DisplayMessage("INVENTORY FULL!");
            return;
        }
        Inventory.Add(item);
    }

    public PlayerItem GetItem(ItemType itemType)
    {
        int index = Inventory.FindIndex(i => i.Item == itemType);
        if (index < 0) return null;

        PlayerItem item = Inventory[index];
        Inventory.RemoveAt(index);

        return item;
    }

    public void ConsumeEnergy(int amount)
    {
        Energy.Consume(amount);
    }

    private void OnDisable()
    {
        GameManager.MissionBegin -= GameStart;
    }
}
