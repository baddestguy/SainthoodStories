using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

public class Player : MonoBehaviour
{
    public static event UnityAction<Energy, MapTile> OnMoveSuccessEvent;
    public static UnityAction StatusEffectTrigger;
    public GameMap Map;

    public Energy Energy;
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
    public List<PlayerStatusEffect> StatusEffects = new List<PlayerStatusEffect>();

    private GameObject GroundTapFX;
    private GameObject GroundMoveFX;

    public static bool OnEnergyDepleted;
    private int WeatherStatusCounter;
    private int FrozenCounter = 3;

    public GameObject FatigueFx;
    public GameObject FrozenFx;
    public ParticleSystem SnowSplash;
    public GameObject CharacterGO;

    private int SickCountdown = 3;
    private int MigraineCountdown = 6;
    private int FastingCoutndown = 6;

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

        if (StatusEffects.Any())
        {
            FatigueFx.SetActive(true);
        }
        else
        {
            FatigueFx.SetActive(false);
        }

        if (StatusEffects.Contains(PlayerStatusEffect.FROZEN))
        {
            FrozenFx.SetActive(true);
            CharacterGO.SetActive(false);
        }
        else
        {
            FrozenFx.SetActive(false);
            CharacterGO.SetActive(true);
        }
    }

    public void GameStart(Mission missionDetails)
    {
        StatusEffects = GameManager.Instance.SaveData.StatusEffects?.ToList() ?? new List<PlayerStatusEffect>();
        Energy = missionDetails.StartingEnergy;
        StartingEnergy = Energy.Amount;
        OnEnergyDepleted = false;
        LockMovement = false;
        StartCoroutine(WaitThenEnterChurch());
    }

    private InteractableHouse GetCurrentBuilding()
    {        
        if(GameManager.Instance.GameClock.Time == 6) return FindObjectOfType<InteractableChurch>();

        switch (GameManager.Instance.SaveData.CurrentHouse)
        {
            case "InteractableChurch":
                return FindObjectOfType<InteractableChurch>();
            case "InteractableHospital":
                return FindObjectOfType<InteractableHospital>();
            case "InteractableKitchen":
                return FindObjectOfType<InteractableKitchen>();
            case "InteractableOrphanage":
                return FindObjectOfType<InteractableOrphanage>();
            case "InteractableShelter":
                return FindObjectOfType<InteractableShelter>();
            case "InteractableSchool":
                return FindObjectOfType<InteractableSchool>();
            case "InteractableClothesBank":
                return FindObjectOfType<InteractableClothesBank>();
            case "InteractableMarket":
                return FindObjectOfType<InteractableMarket>();
        }

        return FindObjectOfType<InteractableChurch>();
    }

    IEnumerator WaitThenEnterChurch()
    {
        yield return new WaitForSeconds(1f);
        CurrentBuilding = GetCurrentBuilding();
        AdjacentTiles = Map.GetAdjacentTiles(CurrentBuilding.CurrentGroundTile);
        OnMove(CurrentBuilding.CurrentGroundTile);
        StartTile = CurrentBuilding;

        OnMoveSuccessEvent?.Invoke(Energy, CurrentBuilding);
        GameManager.Instance.GameClock.Ping();
        ToolTipManager.Instance.ShowToolTip("");
        GameClock.Ticked += OnTick;

        if (GameSettings.Instance.StoryToggle)
        {
            int CurrentWeek = MissionManager.Instance.CurrentMission.CurrentWeek;
            GameClock currentClock = GameManager.Instance.GameClock;
            var storyEvent = GameDataManager.Instance.StoryEventData.Select(y => y.Value).Where(s => s.Week == CurrentWeek && s.Day == currentClock.Day && s.Time == currentClock.Time).OrderBy(x => x.OrderBy);
            var filteredEvents = storyEvent.Where(e =>
            {
                if (e.Id.Contains("Tutorial") && !GameSettings.Instance.FTUE)
                {
                    return false;
                }

                return true;
            });

            if (!EventsManager.Instance.HasEvent(InteractableHouse.HouseTriggeredEvent) && InteractableHouse.HouseTriggeredEvent != CustomEventType.NONE)
            {
                EventsManager.Instance.AddEventToList(InteractableHouse.HouseTriggeredEvent);
                EventsManager.Instance.ExecuteEvents();
            }

            EventsManager.Instance.ForceTriggerStoryEvent(filteredEvents);
        }
    }

    public bool WeCanMove(MapTile tile)
    {
        if (AdjacentTiles == null) return false;

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
        if (StatusEffects.Count < 1) return;

        PopUIFX.transform.position = transform.position + new Vector3(0, 1, 0);
        PopUIFX.gameObject.SetActive(true);
        PopUIFX.Init("Ailment", 0);
    }

    private void StormyWeatherEffect()
    {
        if (!WeatherManager.Instance.IsStormy()) return;

        switch (MissionManager.Instance.CurrentMission.Season)
        {
            case Season.SUMMER:
                if (!InventoryManager.Instance.HasProvision(Provision.SHADES))
                {
                    WeatherStatusCounter++;
                    if (WeatherStatusCounter >= 3)
                    {
                        if (Random.Range(0, 100) < 50)
                        {
                            WeatherStatusCounter = 0;
                            AddRandomAilment();
                        }
                    }
                }
                break;

            case Season.FALL:
                if (!InventoryManager.Instance.HasProvision(Provision.UMBRELLA))
                {
                    WeatherStatusCounter++;
                    if (WeatherStatusCounter >= 3)
                    {
                        if (Random.Range(0, 100) < 50)
                        {
                            WeatherStatusCounter = 0;
                            AddRandomAilment();
                        }
                    }
                }
                break;

            case Season.WINTER:
                if (!InventoryManager.Instance.HasProvision(Provision.WINTER_CLOAK))
                {
                    WeatherStatusCounter++;
                    if (WeatherStatusCounter >= 3)
                    {
                        if (Random.Range(0, 100) < 50)
                        {
                            WeatherStatusCounter = 0;
                            StatusEffects.Add(PlayerStatusEffect.FROZEN);
                            Debug.LogWarning("FROZEN!");
                        }
                    }
                }
                break;
        }
    }

    private void OnMove(MapTile newTile)
    {
        StormyWeatherEffect();

        CurrentTile = newTile;

        EnergyConsumption = ModifyEnergyConsumption(CurrentTile);
    //    Energy.Consume(EnergyConsumption);

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

    public void OnInteract(MapTile newTile, bool passTime = true)
    {
        if (LockMovement || PauseMenu.Instance.active) return;

        EnergyConsumption = ModifyEnergyConsumption(newTile);
        if (newTile is InteractableObject)
        {
            var iTile = newTile as InteractableObject;
            if ((!passTime || WeCanMove(iTile.CurrentGroundTile)) && !StatusEffects.Contains(PlayerStatusEffect.FROZEN))
            {
                transform.localScale = Vector3.one;
                StartCoroutine(WaitThenDisappear(iTile, passTime));
            }
            else
            {
                TileDance(iTile);
            }
        }
        else
        {
            if (WeCanMove(newTile) && !StatusEffects.Contains(PlayerStatusEffect.FROZEN))
            {
                transform.localScale = Vector3.one;
                OnMove(newTile);
                OnMoveSuccessEvent?.Invoke(Energy, newTile);
                ApplyStatusEffect();
                if (passTime)
                    GameManager.Instance.PassTime();
                GroundMoveFX.transform.position = newTile.transform.position + new Vector3(0,0.1f);
                GroundMoveFX.SetActive(false);
                GroundMoveFX.SetActive(true);
                SoundManager.Instance.PlayOneShotSfx("Walk_SFX");
            }
            else
            {
                TileDance(newTile);
            }
        }
    }

    private IEnumerator WaitThenDisappear(InteractableObject iTile, bool passTime)
    {
        CurrentBuilding = iTile as InteractableHouse;
        OnMove(iTile.CurrentGroundTile);
        yield return new WaitForSeconds(0.3f);
        DissapearInHouse = true;
        OnMoveSuccessEvent?.Invoke(Energy, iTile);
        ApplyStatusEffect();

        if (passTime)
            GameManager.Instance.PassTime();
    }

    private void ResetPlayerOnEnergyDepletedAsync()
    {
      //  StatusEffect = PlayerStatusEffect.FATIGUED;
        UI.Instance.CrossFade(1f);

        SoundManager.Instance.EndAllTracks();
        OnEnergyDepleted = true;
        WeatherManager.Instance.ResetWeather();
        EventsManager.Instance.AddEventToList(CustomEventType.ENERGY_DEPLETED);
        GameManager.Instance.ReloadLevel();
    }

    public int ModifyEnergyConsumption(MapTile tile = null, bool tooltip = false, int amount = 1)
    {
        int energyAmount = amount;
        CustomEventData e = EventsManager.Instance.CurrentEvents.Find(i => i.Id == CustomEventType.SICK);

        if(StatusEffects.Contains(PlayerStatusEffect.FATIGUED))
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

            if (StatusEffects.Contains(PlayerStatusEffect.FROZEN))
            {
                FrozenCounter--;
                Energy.Consume(ModifyEnergyConsumption(tile));
                Debug.LogWarning("FROZEN!");
                if (FrozenCounter <= 0)
                {
                    FrozenCounter = 3;
                    StatusEffects.Remove(PlayerStatusEffect.FROZEN);
                    AddRandomAilment();
                    SnowSplash.Play();
                    Debug.LogWarning("SICK FROM ICE!");
                }
                FrozenFx.transform.DOLocalJump(Vector3.zero, 1f, 1, 0.3f);

                ApplyStatusEffect();

                GameManager.Instance.PassTime();
                return;
            }

            transform.eulerAngles = new Vector3(0, 180f, 0);
            Animator.SetTrigger("Dance");
            var soundByte = Random.Range(0, 3);
            switch (soundByte)
            {
                case 0: SoundManager.Instance.PlayOneShotSfx("Hi_SFX"); break;
                case 1: SoundManager.Instance.PlayOneShotSfx("Hey_SFX"); break;
                case 2: SoundManager.Instance.PlayOneShotSfx("Hello_SFX"); break;
            }
        }
        else
        {
            GroundTapFX.transform.position = tile.transform.position;
            GroundTapFX.SetActive(false);
            GroundTapFX.SetActive(true);
            SoundManager.Instance.PlayOneShotSfx("GrassTouch_SFX");
        }
    }

    public void ConsumeEnergy(int amount)
    {
        if(amount >= 0)
        {
            amount = ModifyEnergyConsumption(amount: amount);
        }
        Energy.Consume(amount);

        if (amount > 0 && StatusEffects.Contains(PlayerStatusEffect.VULNERABLE))
        {
            AddRandomAilment();
        }
        else if(Energy.Amount <= 0 && Random.Range(0,100) < 50 && (MissionManager.Instance.CurrentMission.CurrentWeek > 1 || GameManager.Instance.GameClock.Day >= 3))
        {
            StatusEffects.Add(PlayerStatusEffect.VULNERABLE);
        }
    }

    public void ApplyStatusEffect()
    {
        if (StatusEffects.Contains(PlayerStatusEffect.SICK))
        {
            SickCountdown--;
            if(SickCountdown == 0)
            {
                Energy.Consume(1);
                SickCountdown = 3;
            }
        }

        if (StatusEffects.Contains(PlayerStatusEffect.MIGRAINE))
        {
            MigraineCountdown--;
            if(MigraineCountdown == 0)
            {
                Energy.Consume(1000);
                MigraineCountdown = 6;
            }
        }
    }

    public void AddRandomAilment()
    {
        if (StatusEffects.Contains(PlayerStatusEffect.VULNERABLE))
        {
            StatusEffects.Add((PlayerStatusEffect)Random.Range(1, 5));
        }
        StatusEffects.Add((PlayerStatusEffect)Random.Range(1, 5));
        StatusEffectTrigger?.Invoke();
        Debug.LogWarning("ADDED AILMENT!");
    }

    public void RemoveRandomStatusEffect()
    {
        if (!StatusEffects.Any()) return;

        StatusEffectTrigger?.Invoke();
        StatusEffects.RemoveAt(Random.Range(0, StatusEffects.Count));
    }

    public bool EnergyDepleted()
    {
        return Energy.Depleted();
    }

    public bool CanUseEnergy(int consumption)
    {
        return Energy.CanUseEnergy(consumption);
    }

    public void ResetEnergy()
    {
        Energy.Consume(10000);

        Energy.Consume(-3);
    }

    public int GetEnergyAmount()
    {
        return Energy.Amount;
    }

    private void OnTick(double time, int day)
    {
        if (!GameClock.DeltaTime) return;

        var fasting = InventoryManager.Instance.GetProvision(Provision.FASTING);
        if (fasting == null) return;

        FastingCoutndown--;
        if(FastingCoutndown == 0)
        {
            ConsumeEnergy(fasting.Value);
            GameManager.Instance.MissionManager.UpdateFaithPoints(fasting.Value);
            FastingCoutndown = 6;
        }

        if (StatusEffects.Any())
        {
            SoundManager.Instance.PlayOneShotSfx("LowEnergy_SFX");
        }
    }

    private void OnDisable()
    {
        GameClock.Ticked -= OnTick;
    }
}
