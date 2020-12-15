using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

public class InteractableHouse : InteractableObject
{
    public GameClock DeadlineTime;
    public int EnergyConsumption;
    public int OpenTime;
    public int ClosingTime;
    public int ItemDeliveryPoints;
    public int VolunteerPoints;
    public int DeadlineDeliveryBonus;
    public bool DeadlineSet;
    public int RequiredItems;    

    public MissionDifficulty MissionDifficulty;
    public static int DeadlineCounter;

    protected int CurrentCharityPoints;
    protected int CurrentFaithPoints;
    protected const int MeditationPoints = 1;
    public int NeglectedPoints;
    protected int NeglectedMultiplier = 1;

    protected PopIcon PopIcon;
    protected BuildingInformationPopup InfoPopup;
    protected BuildingInformationPopup RubbleInfoPopup;
    protected PopUI PopUI;
    protected string PopUILocation = "";
    protected string OriginalPopUILocation = "";

    public UnityEvent ButtonCallback;
    private bool CameraLockOnMe;
    private PopUIFX PopUIFX;
    public static bool HouseUIActive;

    public BuildingState BuildingState;
    protected int BuildPoints = 0;
    public GameObject RubbleGo;
    public GameObject BuildingGo;

    public int RelationshipPoints;
    private int VolunteerCountdown = 0;
    public int EventsTriggered;

    public static UnityAction<bool> OnEnterHouse;
    public BuildingActivityState BuildingActivityState = BuildingActivityState.NONE;
    protected IEnumerable<BuildingMissionData> MyMissions;

    public static UnityAction<float> OnActionProgress;

    protected virtual void Start()
    {
        UI.Meditate += Meditated;
        MissionManager.EndOfDay += ReportScores;
        MissionManager.EndOfDay += EndofDay;
        EventsManager.EventDialogTriggered += OnEventDialogTriggered;
        EventsManager.EventExecuted += OnEventExecuted;

        LoadData();
        Initialize();
    }

    public void Initialize()
    {
        if (BuildingState == BuildingState.NORMAL)
        {
            PopIcon = Instantiate(Resources.Load<GameObject>("UI/PopIcon")).GetComponent<PopIcon>();
        }
        else if (BuildingState == BuildingState.RUBBLE)
        {
            PopIcon = Instantiate(Resources.Load<GameObject>("UI/PopConstructIcon")).GetComponent<PopIcon>();
            OriginalPopUILocation = PopUILocation;
            PopUILocation = "UI/ConstructUI";
        }

        InfoPopup = Instantiate(Resources.Load<GameObject>("UI/BuildingInfoPopup")).GetComponent<BuildingInformationPopup>();
        RubbleInfoPopup = Instantiate(Resources.Load<GameObject>("UI/RubbleInfoPopup")).GetComponent<BuildingInformationPopup>();
        
        RubbleGo.SetActive(BuildingState == BuildingState.RUBBLE);
        BuildingGo.SetActive(BuildingState == BuildingState.NORMAL);

        PopIcon.transform.SetParent(transform);
        PopIcon.transform.localPosition = new Vector3(0, 1, 0);
        PopIcon.gameObject.SetActive(false);

        InfoPopup.transform.SetParent(transform);
        InfoPopup.transform.localPosition = new Vector3(0, 2, 0);
        InfoPopup.gameObject.SetActive(false);

        RubbleInfoPopup.transform.SetParent(transform);
        RubbleInfoPopup.transform.localPosition = new Vector3(0, 2, 0);
        RubbleInfoPopup.gameObject.SetActive(false);

        if (!string.IsNullOrEmpty(PopUILocation))
        {
            PopUI = Instantiate(Resources.Load<GameObject>(PopUILocation)).GetComponent<PopUI>();
            PopUI.transform.SetParent(transform);
            PopUI.transform.localPosition = new Vector3(0, 1, 0);
            PopUI.gameObject.SetActive(false);
        }

        if(PopUIFX == null)
        {
            PopUIFX = Instantiate(Resources.Load("UI/PopUIFX") as GameObject).GetComponent<PopUIFX>();
            PopUIFX.gameObject.SetActive(false);
        }

        MyMissions = GameDataManager.Instance.GetBuildingMissionData(GetType().Name);
    }

    public void Init(int deadline, MapTile groundTile, TileData tileData, Sprite[] sprites, int sortingOrder = 0)
    {
        Init(groundTile, tileData, sprites, sortingOrder);
    }

    public override void Tick(double time, int day)
    {
        if (DeadlineTime.Time != -1)
        {
            PopMyIcon();
            Debug.LogWarning($"{name}: Deadline: {DeadlineTime.Time} : DAY {DeadlineTime.Day} : {RequiredItems} Items!!");
        }

        if(GameClock.DeltaTime)
        {
            SetDeadlineTime(time, day);
        }

        if ((DeadlineTime.Time != -1) && (time >= DeadlineTime.Time && day >= DeadlineTime.Day))
        {
            Debug.LogError($"{name}: TIME UP!");
            NeglectedMultiplier++;
            DeadlineCounter--;
            DeadlineTime.SetClock(-1, day);
            DeadlineDeliveryBonus = 1;
            DeadlineSet = false;
            RequiredItems = 0;
            PopIcon.gameObject.SetActive(false);
            UI.Instance.SideNotificationPop(GetType().Name);
            UpdateCharityPoints(-2, 0);
        }
        PopUI.Init(PopUICallback, GetType().Name, RequiredItems, DeadlineTime, this);

        switch (BuildingActivityState)
        {
            case BuildingActivityState.VOLUNTEERING:
                VolunteerCountdown++;
                OnActionProgress?.Invoke(VolunteerCountdown / 4f);
                if (VolunteerCountdown >= 4)
                {
                    BuildRelationship(ThankYouType.VOLUNTEER);
                    Player player = GameManager.Instance.Player;
                    var moddedEnergy = player.ModifyEnergyConsumption(amount: EnergyConsumption);
                    UpdateCharityPoints(VolunteerPoints, moddedEnergy);
                    VolunteerCountdown = 0;
                }
                break;
        }

        BuildingActivityState = BuildingActivityState.NONE;

        if (CanBuild() && !HouseUIActive)
        {
            PopIcon.gameObject.SetActive(true);
            PopIcon.Init("Rubble", 0, new GameClock(-1));
        }
    }

    protected virtual BuildingMissionData GetBuildingMission(BuildingEventType bEvent)
    {
        GameClock clock = GameManager.Instance.GameClock;
        return MyMissions.Where(m => m.Week == GameManager.Instance.CurrentMission.CurrentWeek && m.Day == clock.Day && m.Time == clock.Time && m.Event == bEvent).FirstOrDefault();
    }

    protected virtual bool SameDayAsMission()
    {
        GameClock clock = GameManager.Instance.GameClock;
        foreach(var mission in MyMissions)
        {
            if(mission.Week == GameManager.Instance.CurrentMission.CurrentWeek && mission.Day == clock.Day)
            {
                return true;
            }
        }
        return false;
    }

    public virtual void SetDeadlineTime(double time, int day)
    {
        if (BuildingState != BuildingState.NORMAL) return;
        if (time < 6 || time >= 18) return;
        if ((DeadlineTime.Time != -1)) return;

        switch (MissionDifficulty)
        {
            case MissionDifficulty.EASY:
                if (DeadlineCounter < 1)
                {
                    var mission = GetBuildingMission(BuildingEventType.DELIVER_ITEM);
                    if (mission != null || (!SameDayAsMission() && Random.Range(0, 100) < 1))
                    {
                        DeadlineCounter++;
                        DeadlineTime.SetClock(time + (mission != null ? mission.DeadlineHours : RandomFutureTimeByDifficulty()), day);
                        RequiredItems = mission != null ? mission.RequiredItems : 1;
                        DeadlineDeliveryBonus = 4;
                        DeadlineSet = true;
                        PopMyIcon();
                        Debug.LogWarning($"{name}: DEADLINE SET FOR {DeadlineTime.Time} : DAY  {DeadlineTime.Day} : {RequiredItems} Items!");
                    }
                }
                break;

            case MissionDifficulty.NORMAL:
                if (DeadlineCounter < 2)
                {
                    var mission = GetBuildingMission(BuildingEventType.DELIVER_ITEM);
                    if (mission != null || (!SameDayAsMission() && Random.Range(0, 100) < 1))
                    {
                        DeadlineCounter++;
                        DeadlineTime.SetClock(time + (mission != null ? mission.DeadlineHours : RandomFutureTimeByDifficulty()), day);
                        RequiredItems = mission != null ? mission.RequiredItems : Random.Range(1,3);
                        DeadlineDeliveryBonus = 3;
                        DeadlineSet = true;
                        PopMyIcon();
                        Debug.LogWarning($"{name}: DEADLINE SET FOR {DeadlineTime.Time} : DAY  {DeadlineTime.Day} : {RequiredItems} Items!");
                    }
                }
                break;

            case MissionDifficulty.HARD:
                if (DeadlineCounter < 3)
                {
                    var mission = GetBuildingMission(BuildingEventType.DELIVER_ITEM);
                    if (mission != null || (!SameDayAsMission() && Random.Range(0, 100) < 3))
                    {
                        DeadlineCounter++;
                        DeadlineTime.SetClock(time + (mission != null ? mission.DeadlineHours : RandomFutureTimeByDifficulty()), day);
                        RequiredItems = mission != null ? mission.RequiredItems : Random.Range(1,3);
                        DeadlineDeliveryBonus = 2;
                        DeadlineSet = true;
                        PopMyIcon();
                        Debug.LogWarning($"{name}: DEADLINE SET FOR {DeadlineTime.Time} : DAY  {DeadlineTime.Day} : {RequiredItems} Items!");
                    }
                }
                break;
        }
    }

    public double RandomFutureTimeByDifficulty()
    {
        switch (MissionDifficulty)
        {
            case MissionDifficulty.EASY: return Random.Range(6, 9);
            case MissionDifficulty.NORMAL: return Random.Range(6, 9);
            case MissionDifficulty.HARD: return Random.Range(6, 9);
        }

        return -1;
    }

    public virtual void EndofDay()
    {
        EventsTriggered = 0;
    }

    public override void MissionBegin(Mission mission)
    {
        MissionDifficulty = GameManager.MissionDifficulty;
        //Load data?
        DeadlineTime = new GameClock(-1);
        DeadlineDeliveryBonus = 1;
    }

    public virtual void Meditated(InteractableHouse house)
    {
        if (house != this) return;

        GameClock clock = GameManager.Instance.GameClock;
        Player player = GameManager.Instance.Player;

        player.ConsumeEnergy(-1);
        UI.Instance.DisplayMessage("MEDITATED!!");
        UpdateFaithPoints(MeditationPoints, 1);
        clock.Tick();
    }

    public virtual void DeliverItem(InteractableHouse house)
    {
        if (house != this) return;

        RequiredItems--;
        PopMyIcon();
        PopUI.Init(PopUICallback, GetType().Name, RequiredItems, DeadlineTime, this);
        if (RequiredItems <= 0)
        {
            DeadlineCounter = Mathf.Max(0, DeadlineCounter - 1);
            DeadlineTime.SetClock(-1, DeadlineTime.Day);
            DeadlineDeliveryBonus = 1;
            DeadlineSet = false;
            RequiredItems = 0;
            PopIcon.gameObject.SetActive(false);
            UI.Instance.SideNotificationPop(GetType().Name);

            BuildRelationship(ThankYouType.ITEM);
            GameClock.ExecuteEvents?.Invoke();
        }
    }

    public void ThankYouMessage(ThankYouType thanks)
    {
        switch (thanks)
        {
            case ThankYouType.ITEM: 
                ItemDeliveryThanks(); 
                break;

            case ThankYouType.BABY:
                EventsManager.Instance.AddEventToList(CustomEventType.THANKYOU_BABY);
                break;

            case ThankYouType.TEACH:
                EventsManager.Instance.AddEventToList(CustomEventType.THANKYOU_TEACH);
                break;

            case ThankYouType.VOLUNTEER:
                VolunteerThanks();
                break;
        }
    }

    public virtual void ItemDeliveryThanks()
    {
    }

    public virtual void VolunteerThanks()
    {
    }

    public virtual void MoneyThanks()
    {
        EventsManager.Instance.AddEventToList(CustomEventType.THANKYOU_MONEY);
    }

    protected void PopUIFXIcons(string icon, int amount)
    {
        PopUIFX.transform.position = transform.position + new Vector3(0, 0, 0);
        PopUIFX.gameObject.SetActive(true);
        PopUIFX.Init(icon, amount, 0);
    }

    protected IEnumerator PopUIFXIconsAsync(Stack<Tuple<string, int>> stack)
    {
        while(stack.Count > 0)
        {
            var item = stack.Pop();
            PopUIFXIcons(item.Item1, item.Item2);
            yield return new WaitForSeconds(0.5f);
        }
    }

    public virtual void Build()
    {
        Player player = GameManager.Instance.Player;
        if (player.EnergyDepleted() || !CanBuild()) return;

        BuildPoints++;
        OnActionProgress?.Invoke(BuildPoints / 4f);

        UI.Instance.DisplayMessage("BUILDING!");
        if(BuildPoints >= 4)
        {
            //Play Cool Construction Vfx and Animation!
            BuildingState = BuildingState.NORMAL;
            PopUILocation = OriginalPopUILocation;
            Destroy(PopUI.gameObject);
            Initialize();
            PopUI.gameObject.SetActive(true);
            SoundManager.Instance.PlayOneShotSfx("Success", 1f, 5f);
            SoundManager.Instance.PlayHouseAmbience("Construction", false, 0.3f);
            SoundManager.Instance.PlayOneShotSfx("Cheer",1f,5f);
            if (DuringOpenHours())
            {
                SoundManager.Instance.PlayHouseAmbience(GetType().Name, true, 0.3f);
            }

            BuildingCompleteDialog();
            var moddedEnergy = player.ModifyEnergyConsumption(amount: EnergyConsumption);
            UpdateCharityPoints(VolunteerPoints*2, moddedEnergy);
            SaveDataManager.Instance.SaveGame();
        }
        else
        {
            SoundManager.Instance.PlayOneShotSfx("Build", 1f, 5f);
        }
        player.ConsumeEnergy(EnergyConsumption);
        GameClock clock = GameManager.Instance.GameClock;
        clock.Tick();
    }

    public virtual void BuildingCompleteDialog()
    {
        switch (GetType().Name)
        {
            case "InteractableOrphanage":
                EventsManager.Instance.AddEventToList(CustomEventType.ORPHANAGE_COMPLETE);
                break;
            case "InteractableKitchen":
                EventsManager.Instance.AddEventToList(CustomEventType.KITCHEN_COMPLETE);
                break;
            case "InteractableShelter":
                EventsManager.Instance.AddEventToList(CustomEventType.SHELTER_COMPLETE);
                break;
            case "InteractableSchool":
                EventsManager.Instance.AddEventToList(CustomEventType.SCHOOL_COMPLETE);
                break;
            case "InteractableClothesBank":
                EventsManager.Instance.AddEventToList(CustomEventType.CLOTHES_COMPLETE);
                break;
        }
    }

    public virtual void PopUICallback(string button)
    {
        switch (button)
        {
            case "BUILD":
                Build();
                break;
        }
    }

    public virtual void VolunteerWork(InteractableHouse house)
    {
    }

    public virtual void UpdateCharityPoints(int amount, int energy)
    {
        CustomEventData e = EventsManager.Instance.CurrentEvents.Find(i => i.Id == CustomEventType.HIGH_MORALE || i.Id == CustomEventType.LOW_MORALE);
        int charityMultiplier = 1;
        if (e != null)
        {
            if (Random.Range(0, 100) < 20)
            {
                charityMultiplier += e.Id == CustomEventType.HIGH_MORALE ? (int)e.Gain : -charityMultiplier;
            }
        }

        CurrentCharityPoints += amount * charityMultiplier;
        Stack<Tuple<string, int>> stack = new Stack<Tuple<string, int>>();
        stack.Push(new Tuple<string, int>("CPHappy", amount * charityMultiplier));
        if (energy != 0) stack.Push(new Tuple<string, int>("Energy", -energy));
        StartCoroutine(PopUIFXIconsAsync(stack));

        GameManager.Instance.MissionManager.UpdateCharityPoints(amount * charityMultiplier, null);
        UI.Instance.BuildingAlertPop(GetType().Name);
    }

    public virtual void UpdateFaithPoints(int amount, int energy)
    {
        CustomEventData e = EventsManager.Instance.CurrentEvents.Find(i => i.Id == CustomEventType.HIGH_SPIRIT || i.Id == CustomEventType.LOW_SPIRIT);

        int faithMultiplier = InventoryManager.Instance.HasProvision(Provision.ROSARY) ? 2 : 1;
        if(e != null)
        {
            if(Random.Range(0,100) < 20)
            {
                faithMultiplier += e.Id == CustomEventType.HIGH_SPIRIT ? (int)e.Gain : -faithMultiplier;
            }
        }
        CurrentFaithPoints += amount * faithMultiplier;
        Stack<Tuple<string, int>> stack = new Stack<Tuple<string, int>>();
        stack.Push(new Tuple<string, int>("InteractableChurch", amount * faithMultiplier));
        if (energy != 0) stack.Push(new Tuple<string, int>("Energy", energy));
        StartCoroutine(PopUIFXIconsAsync(stack));
  
        GameManager.Instance.MissionManager.UpdateFaithPoints(amount * faithMultiplier);
        Debug.LogWarning("FAITH: " + CurrentFaithPoints);
    }

    public virtual void ReportScores()
    {
        GameManager.Instance.MissionManager.UpdateCharityPoints(0, this);

        if (CurrentCharityPoints <= 0)
        {
            NeglectedMultiplier++;
        }
        else
        {
            NeglectedMultiplier = 1;
        }

        CurrentCharityPoints = 0;
        CurrentFaithPoints = 0;
    }

    public virtual bool DuringOpenHours(GameClock newClock = null)
    {
        GameClock clock = newClock ?? GameManager.Instance.GameClock;
        return clock.Time >= OpenTime && clock.Time < ClosingTime;
    }

    public virtual void PopMyIcon(string name = "", int items = -1, GameClock time = null)
    {
        if (string.IsNullOrEmpty(name)) name = GetType().Name;
        if (items < 0) items = RequiredItems;
        if (time == null) time = DeadlineTime;

        if (HouseUIActive)
        {
            UI.Instance.SideNotificationPush(name, items, time, GetType().Name);
            PopIcon.gameObject.SetActive(false);
            return;
        }
        else
        {
            UI.Instance.SideNotificationPop(GetType().Name);
        }

        PopIcon.gameObject.SetActive(true);
        PopIcon.Init(name, items, time);
    }

    public override void OnPlayerMoved(Energy energy, MapTile tile)
    {
        base.OnPlayerMoved(energy, tile);
        if (tile.GetInstanceID() == GetInstanceID())
        {
            Camera.main.GetComponent<CameraControls>().SetCameraTarget(transform.TransformPoint(-7.95f, 10.92f, -6.11f));
            CameraLockOnMe = true;
            HouseUIActive = true;
            PopIcon.gameObject.SetActive(false);
            UI.Instance.EnableInventoryUI(true);
            SoundManager.Instance.PlayOneShotSfx("Zoom", 0.25f);
            if(DuringOpenHours())
            {
                if (BuildingState == BuildingState.NORMAL)
                    SoundManager.Instance.PlayHouseAmbience(GetType().Name, true, 0.3f);
            }
            if (BuildingState == BuildingState.RUBBLE)
                SoundManager.Instance.PlayHouseAmbience("Construction", true, 0.3f);
            SoundManager.Instance.FadeAmbience(0.1f);
            OnEnterHouse?.Invoke(true);
            TriggerCustomEvent();
            UI.Instance.EnableTreasuryUI(true);
            UI.Instance.RefreshTreasuryBalance(0);
        }
        else if(CameraLockOnMe)
        {
            Camera.main.GetComponent<CameraControls>().SetCameraTarget(Vector3.zero);
            CameraLockOnMe = false;
            HouseUIActive = false;
            UI.Instance.EnableInventoryUI(false);
            SoundManager.Instance.PlayOneShotSfx("Zoom", 0.25f);
            if (BuildPoints >= 4)
                SoundManager.Instance.PlayHouseAmbience(GetType().Name, false, 0.3f);
            else
                SoundManager.Instance.PlayHouseAmbience("Construction", false, 0.3f);
            SoundManager.Instance.FadeAmbience(0.3f);
            OnEnterHouse?.Invoke(false);
            ResetActionProgress();
            UI.Instance.EnableTreasuryUI(false);
        }

        InfoPopup.gameObject.SetActive(false);
        RubbleInfoPopup.gameObject.SetActive(false);
    }

    public virtual void TriggerCustomEvent()
    {
        if (GameSettings.Instance.FTUE) return;
        if (EventsTriggered > 0) return;
        if (EventsManager.Instance.EventInProgress) return;
        if (BuildingState != BuildingState.NORMAL) return;
        if (!DuringOpenHours()) return;
        if (EventsManager.Instance.CurrentEvents.Count > 3) return;

        if (Random.Range(0, 100) < 50)
        {
            switch (GetType().Name)
            {
                case "InteractableChurch":
                    if(GameManager.Instance.Player.StatusEffect != PlayerStatusEffect.FATIGUED)
                        EventsManager.Instance.AddEventToList(GameDataManager.Instance.GetRandomEvent(EventGroup.CHURCH).Id);
                    break;
                case "InteractableHospital":
                    EventsManager.Instance.AddEventToList(GameDataManager.Instance.GetRandomEvent(EventGroup.HOSPITAL).Id);
                    break;
                case "InteractableKitchen":
                    EventsManager.Instance.AddEventToList(GameDataManager.Instance.GetRandomEvent(EventGroup.KITCHEN).Id);
                    break;
                case "InteractableOrphanage":
                    EventsManager.Instance.AddEventToList(GameDataManager.Instance.GetRandomEvent(EventGroup.ORPHANAGE).Id);
                    break;
                case "InteractableShelter":
                    EventsManager.Instance.AddEventToList(GameDataManager.Instance.GetRandomEvent(EventGroup.SHELTER).Id);
                    break;
                case "InteractableSchool":
                    EventsManager.Instance.AddEventToList(GameDataManager.Instance.GetRandomEvent(EventGroup.SCHOOL).Id);
                    break;
            }
            EventsTriggered++;
        }
    }

    public virtual void ResetActionProgress()
    {
        VolunteerCountdown = 0;
        BuildingActivityState = BuildingActivityState.NONE;
    }

    private void OnEventDialogTriggered(bool started)
    {
        if (started && CameraLockOnMe)
        {
            PopUI.gameObject.SetActive(false);
        }
        else if (!started && CameraLockOnMe)
        {
            PopUI.gameObject.SetActive(true);
        }

        if(CanBuild() || DeadlineSet || GetType().Name == "InteractableChurch") //Only reason to have popicons displaying
        {
            if(!started && !HouseUIActive)
                PopIcon.gameObject.SetActive(true);            
            PopIcon.UIPopped(started);
            if (CanBuild()) PopIcon.Init("Rubble", 0, new GameClock(-1));
            else PopMyIcon();
        }
    }

    public void BuildRelationship(ThankYouType thanks, int amount = 1)
    {
        RelationshipPoints += Mathf.Clamp(amount, 0, 100);
        RelationshipReward(thanks);
        SoundManager.Instance.PlayOneShotSfx("Success", 1f, 5f);
        if(!GameSettings.Instance.FTUE)
            SaveDataManager.Instance.SaveGame();
    }

    public virtual void RelationshipReward(ThankYouType thanks)
    {
        if(RelationshipPoints >= 65)
        {
            //Special Item
            ThankYouMessage(thanks);

            if (Random.Range(0, 100) < 50)
            {
                MoneyThanks();
                TreasuryManager.Instance.DonateMoney(Random.Range(45, 55));
            }
        }
        else if (RelationshipPoints >= 30)
        {
            ThankYouMessage(thanks);

            if(Random.Range(0,100) < 50)
            {
                MoneyThanks();
                TreasuryManager.Instance.DonateMoney(Random.Range(30, 40));
            }
        }
        else if (RelationshipPoints >= 10)
        {
            ThankYouMessage(thanks);

            if(Random.Range(0,100) < 50)
            {
                MoneyThanks();
                TreasuryManager.Instance.DonateMoney(Random.Range(15, 25));
            }
        }
        else if (RelationshipPoints > 1)
        {
            ThankYouMessage(thanks);
            if (Random.Range(0, 100) < 50)
            {
                MoneyThanks();
                TreasuryManager.Instance.DonateMoney(Random.Range(15, 25));
            }
        }
        else if (RelationshipPoints == 1)
        {
            ThankYouMessage(thanks);
            MoneyThanks();
            TreasuryManager.Instance.DonateMoney(Random.Range(15, 25));
        }
    }

    public bool CanBuild()
    {
        if (BuildPoints >= 4 || BuildingState == BuildingState.NORMAL) return false;
        if (!GameDataManager.Instance.ConstructionAvailability.ContainsKey(GetType().Name)) return true;

        ConstructionAvailabilityData myAvailability = GameDataManager.Instance.ConstructionAvailability[GetType().Name];
        GameClock myClock = new GameClock(myAvailability.Time, myAvailability.Day);
        GameClock currentClock = GameManager.Instance.GameClock;
        int CurrentWeek = MissionManager.Instance.CurrentMission.CurrentWeek;
        return CurrentWeek >= myAvailability.Week && currentClock >= myClock;
    }

    protected virtual void OnEventExecuted(CustomEventData e)
    {

    }

    public virtual bool CanDoAction(string actionName)
    {
        switch (actionName)
        {
            case "BUILD":
                return CanBuild();

            case "PRAY": return true;
            case "SLEEP": return true;
        }

        return false;
    }

    public virtual float SetButtonTimer(string actionName)
    {
        return 1f;
    }

    public virtual TooltipStats GetTooltipStatsForButton(string button)
    {
        switch (button)
        {
            case "PRAY":
                return new TooltipStats() { Ticks = 1, FP = 1, CP = 0, Energy = 1 };
            case "SLEEP":
                return new TooltipStats() { Ticks = 1, FP = 0, CP = 0, Energy = 3 };
            case "VOLUNTEER":
                if(4-VolunteerCountdown == 1)
                    return new TooltipStats() { Ticks = 1, FP = 0, CP = VolunteerPoints, Energy = -GameManager.Instance.Player.ModifyEnergyConsumption(amount: EnergyConsumption) };
                else
                    return new TooltipStats() { Ticks = 1, FP = 0, CP = 0, Energy = -GameManager.Instance.Player.ModifyEnergyConsumption(amount: EnergyConsumption) };

            case "CONSTRUCT":
                if(4-BuildPoints == 1)
                    return new TooltipStats() { Ticks = 1, FP = 0, CP = VolunteerPoints * 2, Energy = -GameManager.Instance.Player.ModifyEnergyConsumption(amount: EnergyConsumption) };
                else
                    return new TooltipStats() { Ticks = 1, FP = 0, CP = 0, Energy = -GameManager.Instance.Player.ModifyEnergyConsumption(amount: EnergyConsumption) };
        }

        return new TooltipStats() { Ticks = 0, FP = 0, CP = 0, Energy = 0 };
    }

    public override void OnMouseOver()
    {
        if (UI.Instance.CrossFading) return;
        if (EventsManager.Instance.EventInProgress) return;
        if (HouseUIActive || EventsManager.Instance.HasEventsInQueue()) return;
        if (!CameraControls.ZoomComplete) return;

        if (BuildingState == BuildingState.RUBBLE)
        {
            RubbleInfoPopup.gameObject.SetActive(true); 
            if(GameManager.Instance.Player.WeCanMove(CurrentGroundTile))
                ToolTipManager.Instance.ShowToolTip("Tooltip_ConstructionSite", new TooltipStats() { Ticks = 1, FP = 0, CP = 0, Energy = -GameManager.Instance.Player.ModifyEnergyConsumption(CurrentGroundTile, true) });
            else
                ToolTipManager.Instance.ShowToolTip("Tooltip_ConstructionSite");
        }
        else
        {
            InfoPopup.gameObject.SetActive(true);

            if (GameManager.Instance.Player.WeCanMove(CurrentGroundTile))
                ToolTipManager.Instance.ShowToolTip("Tooltip_"+GetType().Name, new TooltipStats() { Ticks = 1, FP = 0, CP = 0, Energy = -GameManager.Instance.Player.ModifyEnergyConsumption(CurrentGroundTile, true) });
            else
                ToolTipManager.Instance.ShowToolTip("Tooltip_" + GetType().Name);
        }

        InfoPopup.Init(GetType().Name, OpenTime, ClosingTime, RelationshipPoints, DuringOpenHours());
        PopIcon.gameObject.SetActive(false);
        base.OnMouseOver();
    }

    private bool HouseJumping = false;
    public virtual void HouseJump()
    {
        if (HouseJumping) return;

        SoundManager.Instance.PlayOneShotSfx("HouseJump");
        HouseJumping = true;
        StartCoroutine(HouseJumpAsync());
    }

    private IEnumerator HouseJumpAsync()
    {
        var tilePos = transform.position;
        var newPos = new Vector3(tilePos.x, tilePos.y + 0.5f, tilePos.z);
        while (Mathf.Abs(transform.position.y - newPos.y) > 0.05f)
        {
            transform.position = Vector3.Lerp(transform.position, newPos, Time.deltaTime * 15);
            yield return null;
        }

        while (Mathf.Abs(transform.position.y - tilePos.y) > 0.05f)
        {
            transform.position = Vector3.Lerp(transform.position, tilePos, Time.deltaTime * 15);
            yield return null;
        }

        transform.position = tilePos;
        HouseJumping = false;
    }

    public override void OnMouseExit()
    {
        if (UI.Instance.CrossFading) return;
        if (EventsManager.Instance.EventInProgress) return;
        if (HouseUIActive) return;

        RubbleInfoPopup.gameObject.SetActive(false);
        InfoPopup.gameObject.SetActive(false);
        ToolTipManager.Instance.ShowToolTip("");
        var clock = GameManager.Instance.GameClock;
        clock.Ping();
        base.OnMouseExit();
    }

    public void LoadData()
    {
        var data = GameManager.Instance.SaveData;
        switch (GetType().Name)
        {
            case "InteractableOrphanage":
                RelationshipPoints = data.OrphanageRelationshipPoints;
                BuildingState = data.OrphanageBuildingState;
                break;
            case "InteractableHospital":
                RelationshipPoints = data.HospitalRelationshipPoints;
                BuildingState = data.HospitalBuildingState;
                break;
            case "InteractableShelter":
                RelationshipPoints = data.OrphanageRelationshipPoints;
                BuildingState = data.ShelterBuildingState;
                break;
            case "InteractableSchool":
                RelationshipPoints = data.SchoolRelationshipPoints;
                BuildingState = data.SchoolBuildingState;
                break;
            case "InteractableClothesBank":
                RelationshipPoints = data.ClothesRelationshipPoints;
                BuildingState = data.ClothesBuildingState;
                break;
            case "InteractableKitchen":
                BuildingState = data.KitchenBuildingState;
                break;
            case "InteractableChurch":
                BuildingState = BuildingState.NORMAL;
                break;
            case "InteractableMarket":
                BuildingState = BuildingState.NORMAL;
                break;
        }
    }

    public override void OnDisable()
    {
        UI.Meditate -= Meditated;
        MissionManager.EndOfDay -= EndofDay;
        MissionManager.EndOfDay -= ReportScores;
        EventsManager.EventDialogTriggered -= OnEventDialogTriggered;
        EventsManager.EventExecuted -= OnEventExecuted;

        HouseUIActive = false;
        base.OnDisable();
    }
}
