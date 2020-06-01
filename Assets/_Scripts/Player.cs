using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class Player : MonoBehaviour
{
    public static event UnityAction<Energy, MapTile> OnMoveSuccessEvent;
    public static event UnityAction OnEnergyDepleted;
    public GameMap Map;
    
    private Energy Energy;
    private int EnergyConsumption;
    [SerializeField]
    private MapTile CurrentTile;
    private MapTile StartTile;
    private Dictionary<PlayerFacingDirection, MapTile> AdjacentTiles;
    private List<PlayerItem> Inventory = new List<PlayerItem>();
    private Vector3 TargetPosition;
    private bool LockMovement;
    public Animator Animator;

    void Start()
    {
        GameManager.MissionBegin += GameStart;
        TargetPosition = transform.position;
        Animator.SetBool("Idle", true);
    }

    void Update()
    {
        if((transform.position - TargetPosition).magnitude > 0.11)
        {
            transform.position = Vector3.Lerp(transform.position, TargetPosition, Time.deltaTime*5);
        }
        else
        {
            Animator.SetBool("Idle", true);
            Animator.SetBool("Run", false);
        }
    }

    public void GameStart(Mission missionDetails)
    {
        StartTile = CurrentTile;
        Energy = missionDetails.StartingEnergy;
        AdjacentTiles = Map.GetAdjacentTiles(CurrentTile);
        ModifyEnergyConsumption(CurrentTile);
    }

    private bool WeCanMove(MapTile tile)
    {
        return (CurrentTile != tile && AdjacentTiles.ContainsValue(tile) && tile.TileType != TileType.BARRIER);
    }

    private void FaceNewDirection(MapTile newTile)
    {
        var direction = AdjacentTiles.Where(t => t.Value == newTile).First().Key;
        switch (direction)
        {
            case PlayerFacingDirection.UP:
                transform.eulerAngles = new Vector3(0, 310, 0);
                break;
            case PlayerFacingDirection.DOWN:
                transform.eulerAngles = new Vector3(0, 140, 0);
                break;
            case PlayerFacingDirection.LEFT:
                transform.eulerAngles = new Vector3(0, 210, 0);
                break;
            case PlayerFacingDirection.RIGHT:
                transform.eulerAngles = new Vector3(0, 30, 0);
                break;
        }
    }

    private void OnMove(MapTile newTile)
    {
        CurrentTile = newTile;

        EnergyConsumption = ModifyEnergyConsumption(CurrentTile);
        Energy.Consume(EnergyConsumption);

        FaceNewDirection(CurrentTile);
        AdjacentTiles = Map.GetAdjacentTiles(CurrentTile);

        TargetPosition = CurrentTile.transform.position;

        Animator.SetBool("Run", true);
        Animator.SetBool("Idle", false);
    }

    public void OnInteract(MapTile newTile)
    {
        if (LockMovement) return;

        if (newTile is InteractableObject)
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

        if (Energy.Depleted() && !(newTile is InteractableHouse)) //Reset if out of energy & not in a building
        {
            GameManager.Instance.GameClock.Reset();
            OnEnergyDepleted?.Invoke();
            LockMovement = true;
        }

    }

    public int ModifyEnergyConsumption(MapTile tile)
    {
        int energyAmount = 1;

        if(WeatherManager.Instance.WeatherType != WeatherType.NONE)
        {
            energyAmount++;
        }
        return energyAmount; 
    }

    public void TileDance(MapTile tile)
    {
        //Trigger cute tile animation
        Debug.Log("Tile bubbly animation!");
    }

    public void AddToInventory(PlayerItem item)
    {
        if (Inventory.Count == 2)
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

    public bool EnergyDepleted()
    {
        return Energy.Depleted();
    }

    private void OnDisable()
    {
        GameManager.MissionBegin -= GameStart;
    }
}
