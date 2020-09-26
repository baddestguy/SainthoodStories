using System;
using System.Collections;
using System.Collections.Generic;
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
    public int BuildPoints = 3;
    public GameObject RubbleGo;
    public GameObject BuildingGo;

    public int RelationshipPoints;
    private int VolunteerCountdown = 4;

    public static UnityAction<bool> OnEnterHouse;
    public BuildingActivityState BuildingActivityState = BuildingActivityState.NONE;

    protected virtual void Start()
    {
        UI.Meditate += Meditated;
        UI.DeliverItem += DeliverItem;
        UI.Volunteer += VolunteerWork;
        MissionManager.EndOfDay += ReportScores;
        MissionManager.EndOfDay += EndofDay;
        EventsManager.EventDialogTriggered += OnEventDialogTriggered;
        EventsManager.EventExecuted += OnEventExecuted;

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
        SetDeadlineTime(time, day);

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
        }
        PopUI.Init(PopUICallback, GetType().Name, RequiredItems, DeadlineTime);

        switch (BuildingActivityState)
        {
            case BuildingActivityState.VOLUNTEERING:
                VolunteerCountdown--;
                if (VolunteerCountdown <= 0)
                {
                    BuildRelationship(ThankYouType.VOLUNTEER);
                    VolunteerCountdown = 4;
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
                    if(Random.Range(0, 100) < 1)
                    {
                        DeadlineCounter++;
                        DeadlineTime.SetClock(time + RandomFutureTimeByDifficulty(), day);
                        RequiredItems = 1;
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
                    if (Random.Range(0, 100) < 1)
                    {
                        DeadlineCounter++;
                        DeadlineTime.SetClock(time + RandomFutureTimeByDifficulty(), day);
                        RequiredItems = Random.Range(1,3);
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
                    if (Random.Range(0, 100) < 2)
                    {
                        DeadlineCounter++;
                        DeadlineTime.SetClock(time + RandomFutureTimeByDifficulty(), day);
                        RequiredItems = Random.Range(1,4);
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
            case MissionDifficulty.NORMAL: return Random.Range(5, 8);
            case MissionDifficulty.HARD: return Random.Range(5, 8);
        }

        return -1;
    }

    public virtual void EndofDay()
    {        
    }

    public override void MissionBegin(Mission mission)
    {
        MissionDifficulty = GameManager.MissionDifficulty;
        DeadlineTime = new GameClock(-1);
        DeadlineDeliveryBonus = 1;
    }

    public virtual void Meditated(InteractableHouse house)
    {
        if (house != this) return;

        GameClock clock = GameManager.Instance.GameClock;
        Player player = GameManager.Instance.Player;

        player.ConsumeEnergy(-1);
        clock.Tick();
        UI.Instance.DisplayMessage("MEDITATED!!");
        UpdateFaithPoints(MeditationPoints);
    }

    public virtual void DeliverItem(InteractableHouse house)
    {
        if (house != this) return;

        RequiredItems--;
        PopMyIcon();
        PopUI.Init(PopUICallback, GetType().Name, RequiredItems, DeadlineTime);
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

        BuildPoints--;
        UI.Instance.DisplayMessage("BUILDING!");
        if(BuildPoints <= 0)
        {
            //Play Cool Construction Vfx and Animation!
            BuildingState = BuildingState.NORMAL;
            PopUILocation = OriginalPopUILocation;
            Destroy(PopUI.gameObject);
            Initialize();
            PopUI.gameObject.SetActive(true);
            SoundManager.Instance.PlayOneShotSfx("Success", 1f, 5f);
            SoundManager.Instance.PlayHouseAmbience("Construction", false, 0.3f);
            if (GameManager.Instance.GameClock.Time >= OpenTime && GameManager.Instance.GameClock.Time <= ClosingTime)
            {
                SoundManager.Instance.PlayHouseAmbience(GetType().Name, true, 0.3f);
            }
        }
        else
        {
            SoundManager.Instance.PlayOneShotSfx("Build", 1f, 5f);
        }
        UpdateCharityPoints(VolunteerPoints);
        GameClock clock = GameManager.Instance.GameClock;
        player.ConsumeEnergy(EnergyConsumption); //Umm...?
        clock.Tick();
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

    public virtual void UpdateCharityPoints(int amount)
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
        if (EnergyConsumption != 0) stack.Push(new Tuple<string, int>("Energy", -EnergyConsumption));
        StartCoroutine(PopUIFXIconsAsync(stack));

        GameManager.Instance.MissionManager.UpdateCharityPoints(amount * charityMultiplier, null);
        UI.Instance.BuildingAlertPop(GetType().Name);
    }

    public virtual void UpdateFaithPoints(int amount)
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
        stack.Push(new Tuple<string, int>("Energy", 1)); //Prayer Energy should be a variable
        StartCoroutine(PopUIFXIconsAsync(stack));
  
        GameManager.Instance.MissionManager.UpdateFaithPoints(amount * faithMultiplier);
        Debug.LogWarning("FAITH: " + CurrentFaithPoints);
        SoundManager.Instance.PlayOneShotSfx("Pray", 0.5f, 5f);
    }

    public virtual void ReportScores()
    {
        GameManager.Instance.MissionManager.UpdateCharityPoints(CurrentCharityPoints > 0 ? 0 : (NeglectedPoints * NeglectedMultiplier), this);

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
            Camera.main.GetComponent<CameraControls>().SetCameraTarget(transform.TransformPoint(-7.53f, 11.6f, -5.78f));
            CameraLockOnMe = true;
            HouseUIActive = true;
            PopIcon.gameObject.SetActive(false);
            UI.Instance.EnableInventoryUI(true);
            SoundManager.Instance.PlayOneShotSfx("Zoom", 0.25f);
            if(GameManager.Instance.GameClock.Time >= OpenTime && GameManager.Instance.GameClock.Time <= ClosingTime)
            {
                if (BuildPoints <= 0)
                    SoundManager.Instance.PlayHouseAmbience(GetType().Name, true, 0.3f);
            }
            if (BuildPoints > 0)
                SoundManager.Instance.PlayHouseAmbience("Construction", true, 0.3f);
            SoundManager.Instance.FadeAmbience(0.1f);
            OnEnterHouse?.Invoke(true);
        }
        else if(CameraLockOnMe)
        {
            Camera.main.GetComponent<CameraControls>().SetCameraTarget(Vector3.zero);
            CameraLockOnMe = false;
            HouseUIActive = false;
            UI.Instance.EnableInventoryUI(false);
            SoundManager.Instance.PlayOneShotSfx("Zoom", 0.25f);
            if (BuildPoints <= 0)
                SoundManager.Instance.PlayHouseAmbience(GetType().Name, false, 0.3f);
            else
                SoundManager.Instance.PlayHouseAmbience("Construction", false, 0.3f);
            SoundManager.Instance.FadeAmbience(0.3f);
            OnEnterHouse?.Invoke(false);
        }

        InfoPopup.gameObject.SetActive(false);
        RubbleInfoPopup.gameObject.SetActive(false);
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
                TreasuryManager.Instance.DonateMoney(40);
            }
        }
        else if (RelationshipPoints >= 30)
        {
            ThankYouMessage(thanks);

            if(Random.Range(0,100) < 30)
            {
                MoneyThanks();
                TreasuryManager.Instance.DonateMoney(30);
            }
        }
        else if (RelationshipPoints >= 10)
        {
            ThankYouMessage(thanks);

            if(Random.Range(0,100) < 20)
            {
                MoneyThanks();
                TreasuryManager.Instance.DonateMoney(20);
            }
        }
        else if (RelationshipPoints >= 1)
        {
            ThankYouMessage(thanks);
        }
    }

    public bool CanBuild()
    {
        if (BuildPoints <= 0 || BuildingState == BuildingState.NORMAL) return false;
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

    public virtual void OnMouseOver()
    {
        if (HouseUIActive || EventsManager.Instance.HasEventsInQueue()) return;

        if (BuildingState == BuildingState.RUBBLE)
        {
            RubbleInfoPopup.gameObject.SetActive(true);
        }
        else
        {
            InfoPopup.gameObject.SetActive(true);
        }

        InfoPopup.Init(GetType().Name, OpenTime, ClosingTime, RelationshipPoints);
        PopIcon.gameObject.SetActive(false);
    }

    public virtual void OnMouseExit()
    {
        if (HouseUIActive) return;

        RubbleInfoPopup.gameObject.SetActive(false);
        InfoPopup.gameObject.SetActive(false);
        var clock = GameManager.Instance.GameClock;
        Tick(clock.Time, clock.Day);
    }

    public override void OnDisable()
    {
        UI.Meditate -= Meditated;
        UI.DeliverItem -= DeliverItem;
        UI.Volunteer -= VolunteerWork;
        MissionManager.EndOfDay -= EndofDay;
        MissionManager.EndOfDay -= ReportScores;
        EventsManager.EventDialogTriggered -= OnEventDialogTriggered;
        EventsManager.EventExecuted -= OnEventExecuted;

        HouseUIActive = false;
        base.OnDisable();
    }
}
