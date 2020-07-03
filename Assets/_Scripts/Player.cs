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
    private int StartingEnergy;
    private Dictionary<PlayerFacingDirection, MapTile> AdjacentTiles;
    private List<PlayerItem> Inventory = new List<PlayerItem>();
    private Vector3 TargetPosition;
    public static bool LockMovement;
    public Animator Animator;

    private PopUIFX PopUIFX;
    private bool DissapearInHouse;
    private PlayerStatusEffect StatusEffect = PlayerStatusEffect.NONE; //Likely going to be a List of status effects that Stack.

    void Start()
    {
        GameManager.MissionBegin += GameStart;
        TargetPosition = transform.position;
        Animator.SetBool("Idle", true);

        PopUIFX = Instantiate(Resources.Load("UI/PopUIFX") as GameObject).GetComponent<PopUIFX>();
        PopUIFX.gameObject.SetActive(false);
    }

    void Update()
    {
        if ((transform.position - TargetPosition).magnitude > 0.11)
        {
            transform.position = Vector3.Lerp(transform.position, TargetPosition, Time.deltaTime * 5);
        }
        else
        {
            Animator.SetBool("Idle", true);
            Animator.SetBool("Run", false);

            if (DissapearInHouse)
            {
                transform.localScale = Vector3.zero;
                DissapearInHouse = false;
            }
        }
    }

    public void GameStart(Mission missionDetails)
    {
        StartTile = CurrentTile;
        Energy = missionDetails.StartingEnergy;
        StartingEnergy = Energy.Amount;
        AdjacentTiles = Map.GetAdjacentTiles(CurrentTile);
        LockMovement = false;
    }

    private bool WeCanMove(MapTile tile)
    {
        return (CurrentTile != tile && AdjacentTiles.ContainsValue(tile) && tile.TileType != TileType.BARRIER);
    }

    private void FaceNewDirection(MapTile newTile)
    {
        var tile = AdjacentTiles.Where(t => t.Value == newTile);
        if (!tile.Any())
        {
            transform.eulerAngles = new Vector3(0, 140, 0); //Down
            return;
        }

        var direction = tile.First().Key;
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

    private void PopUIFXIcons()
    {
        PopUIFX.transform.position = transform.position + new Vector3(0, 1, 0);
        PopUIFX.gameObject.SetActive(true);
        PopUIFX.Init("Energy", -EnergyConsumption);
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

        PopUIFXIcons();
    }

    public void OnInteract(MapTile newTile)
    {
        if (LockMovement) return;

        EnergyConsumption = ModifyEnergyConsumption(newTile);
        if (Energy.Depleted(EnergyConsumption) && !(newTile is InteractableHouse) && WeCanMove(newTile)) //Reset if out of energy & not in a building
        {
            StartCoroutine(ResetPlayerOnEnergyDepletedAsync());
        }
        else if (newTile is InteractableObject)
        {
            var iTile = newTile as InteractableObject;
            if (WeCanMove(iTile.CurrentGroundTile))
            {
                transform.localScale = Vector3.one;
                DissapearInHouse = true;
                OnMove(iTile.CurrentGroundTile);
                OnMoveSuccessEvent?.Invoke(Energy, iTile);
                GameManager.Instance.PassTime();
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
                transform.localScale = Vector3.one;
                OnMove(newTile);
                OnMoveSuccessEvent?.Invoke(Energy, newTile);
                GameManager.Instance.PassTime();
            }
            else
            {
                TileDance(newTile);
            }
        }

        if (Energy.Depleted())
        {
            StatusEffect = PlayerStatusEffect.FATIGUED;
        }
    }

    IEnumerator ResetPlayerOnEnergyDepletedAsync()
    {
        UI.Instance.CrossFade(1f);
        yield return new WaitForSeconds(1f);

        transform.localScale = Vector3.one;
        DissapearInHouse = false;
        OnMove(StartTile);
        Energy.Consume(-StartingEnergy);
        GameManager.Instance.GameClock.Reset();
        UI.Instance.EnableCurrentUI(false);
        WeatherManager.Instance.ResetWeather();
        OnMoveSuccessEvent?.Invoke(Energy, StartTile);

        UI.Instance.CrossFade(0f);
    }

    public int ModifyEnergyConsumption(MapTile tile = null, int amount = 1)
    {
        int energyAmount = amount;
        CustomEventData e = EventsManager.Instance.CurrentEvents.Find(i => i.Id == EventType.SICK);

        if (tile != null && InventoryManager.Instance.HasProvision(Provision.SHOES))
        {
            if (Random.Range(0,100) < 30)
            {
                energyAmount--;
            }
        }
        if(StatusEffect == PlayerStatusEffect.FATIGUED)
        {
            energyAmount++;
        }
        if(tile != null && WeatherManager.Instance.IsStormy() && !InventoryManager.Instance.HasProvision(Provision.UMBRELLA))
        {
            energyAmount++;
        }     
        if(e != null)
        {
            energyAmount += (int)e.Cost;
        }

        return energyAmount; 
    }

    public void TileDance(MapTile tile)
    {
        //Trigger cute tile animation
        Debug.Log("Tile bubbly animation!");
    }

    public void ConsumeEnergy(int amount)
    {
        if(amount >= 0)
        {
            amount = ModifyEnergyConsumption(amount: amount);
        }
        Energy.Consume(amount);
    }

    public void ModifyStatusEffect(PlayerStatusEffect newStatus)
    {
        StatusEffect = newStatus;
    }

    public bool EnergyDepleted(int consumption = 0)
    {
        return Energy.Depleted(consumption);
    }

    private void OnDisable()
    {
        GameManager.MissionBegin -= GameStart;
    }
}
