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
    public InteractableHouse CurrentBuilding;
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

    private GameObject GroundTapFX;
    private GameObject GroundMoveFX;
    void Start()
    {
        TargetPosition = transform.position;

        PopUIFX = Instantiate(Resources.Load("UI/PopUIFX") as GameObject).GetComponent<PopUIFX>();
        PopUIFX.gameObject.SetActive(false);

        GroundTapFX = Instantiate(Resources.Load("Environment/GroundLeavesFx") as GameObject);
        GroundMoveFX = Instantiate(Resources.Load("Environment/GroundMoveFx") as GameObject);
    }

    void Update()
    {
        if ((transform.position - TargetPosition).magnitude > 0.11)
        {
            transform.position = Vector3.Lerp(transform.position, TargetPosition, Time.deltaTime * 5);

            if (DissapearInHouse)
            {
                transform.localScale = Vector3.Lerp(transform.localScale, Vector3.zero, Time.deltaTime * 5);
            }
        }
        else
        {
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
        StartCoroutine(WaitThenEnterChurch());
    }

    IEnumerator WaitThenEnterChurch()
    {
        yield return new WaitForSeconds(1f);
        OnMoveSuccessEvent?.Invoke(Energy, CurrentBuilding);
        GameManager.Instance.GameClock.Ping();
    }

    public bool WeCanMove(MapTile tile)
    {
        if (GameSettings.Instance.FTUE)
        {
            return TutorialManager.Instance.GetTutorialMapTiles().Contains(tile) && CurrentTile != tile && AdjacentTiles.ContainsValue(tile) && tile.TileType != TileType.BARRIER;
        }

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

        PopUIFXIcons();

        if (GameSettings.Instance.FTUE)
        {
            TutorialManager.Instance.RemoveTileFromGroup();
        }
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
                CurrentBuilding = iTile as InteractableHouse;
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
                GroundMoveFX.transform.position = newTile.transform.position + new Vector3(0,0.1f);
                GroundMoveFX.SetActive(false);
                GroundMoveFX.SetActive(true);
                SoundManager.Instance.PlayOneShotSfx("Walk", 0.25f);
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

        transform.localScale = Vector3.zero;
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
        CustomEventData e = EventsManager.Instance.CurrentEvents.Find(i => i.Id == CustomEventType.SICK);

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
        if(tile is InteractableHouse)
        {
            (tile as InteractableHouse).HouseJump();
        }
        else if(tile == CurrentTile)
        {
            transform.eulerAngles = new Vector3(0, 180f, 0);
            Animator.SetTrigger("Dance");
            var soundByte = Random.Range(0, 3);
            switch (soundByte)
            {
                case 0: SoundManager.Instance.PlayOneShotSfx("Hi"); break;
                case 1: SoundManager.Instance.PlayOneShotSfx("Hey"); break;
                case 2: SoundManager.Instance.PlayOneShotSfx("Hello"); break;
            }
        }
        else
        {
            GroundTapFX.transform.position = tile.transform.position;
            GroundTapFX.SetActive(false);
            GroundTapFX.SetActive(true);
            SoundManager.Instance.PlayOneShotSfx("GrassTouch");
        }
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

    public void ResetEnergy()
    {
        Energy.Consume(10000);
        Energy.Consume(-StartingEnergy);
    }

    public int GetEnergyAmount()
    {
        return Energy.Amount;
    }

    private void OnDisable()
    {
    }
}
