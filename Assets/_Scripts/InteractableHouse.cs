using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CompassNavigatorPro;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
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
    public bool DeadlineTriggeredForTheDay;
    public int RequiredItems;
    public int DeadlinePercentChance;

    public MissionDifficulty MissionDifficulty;
    public static int DeadlineCounter;
    public static int HazardCounter;

    protected int CurrentCharityPoints;
    protected int CurrentFaithPoints;
    protected const int MeditationPoints = 1;
    public int NeglectedPoints;
    protected int NeglectedMultiplier = 1;

    protected PopIcon PopIcon;
    protected BuildingInformationPopup InfoPopup;
    protected BuildingInformationPopup RubbleInfoPopup;
    public PopUI ExteriorPopUI;
    public PopUI InteriorPopUI;
    protected string PopUILocation = "";
    protected string OriginalPopUILocation = "";

    public UnityEvent ButtonCallback;
    private bool CameraLockOnMe;
    private PopUIFX PopUIFX;
    public static bool HouseUIActive;
    public static bool InsideHouse;

    public BuildingState BuildingState;
    protected int BuildPoints = 0;
    protected float MaxBuildPoints = 4f;
    public GameObject RubbleGo;
    public GameObject BuildingGo;

    public int RelationshipPoints;
    protected int RelationshipBonus = 0;//TODO: SAVE THIS
    protected int FPBonus = 0;//TODO: SAVE THIS
    protected int VolunteerCountdown = 0;
    protected float MaxVolunteerPoints = 4f;
    public int EventsTriggered;
    public static CustomEventType HouseTriggeredEvent;

    public static UnityAction<bool> OnEnterHouse;
    public BuildingActivityState BuildingActivityState = BuildingActivityState.NONE;
    protected IEnumerable<BuildingMissionData> MyMissions;

    public static UnityAction<float, InteractableHouse, int> OnActionProgress;

    public Camera InteriorCam;
    public Camera InteriorUICamera;
    public GameObject InteriorSpace;

    public int PrayersProgress = 0;
    public float MaxPrayerProgress = 4f;

    public int SturdyMaterials = 0; //TODO: SAVE THIS
    public int CurrentSturdyMaterials = 0;
    public int EnvironmentalHazardDestructionChance = 10;
    public int EnvironmentalHazardDestructionCountdown = -1;
    public bool HasBeenDestroyed;

    protected List<CustomEventType> MyStoryEvents = new List<CustomEventType>();
    public ObjectivesData MyObjective;
    protected virtual void Start()
    {
        UI.Meditate += Meditated;
        MissionManager.EndOfDay += ReportScores;
        MissionManager.EndOfDay += EndofDay;
        UI.UIHidden += OnUIEnabled;
        EventsManager.EventExecuted += OnEventExecuted;
        GameControlsManager.TryZoom += TryZoom;
        InteractableMarket.AutoDeliverToHouse += AutoDeliver;
        WeatherManager.WeatherForecastActive += WeatherAlert;

        LoadData();
        Initialize();
    }

    public void Initialize()
    {
        var objectives = MissionManager.Instance.CurrentObjectives.Where(obj => obj.House == GetType().Name || (obj.House == "Any" && GetType().Name != "InteractableChurch"));
        if(objectives.Count() > 1)
        {
            MyObjective = objectives.Where(obj => obj.House == GetType().Name).FirstOrDefault();
        }
        else
        {
            MyObjective = objectives.FirstOrDefault();
        }

        switch (MissionManager.Instance.CurrentMission.CurrentWeek)
        {
            case 1: DeadlinePercentChance = 5; break;
            case 2: DeadlinePercentChance = 7; break;
            case 3: DeadlinePercentChance = 7; break;
        }

        if (BuildingState == BuildingState.NORMAL || BuildingState == BuildingState.HAZARDOUS)
        {
            PopIcon = Instantiate(Resources.Load<GameObject>("UI/PopIcon")).GetComponent<PopIcon>();
        }
        else if (BuildingState == BuildingState.RUBBLE)
        {
            PopIcon = Instantiate(Resources.Load<GameObject>("UI/PopConstructIcon")).GetComponent<PopIcon>();
            OriginalPopUILocation = PopUILocation;
            PopUILocation = "UI/ConstructUI";
        }

        GetInteriorPopUI();

        InfoPopup = Instantiate(Resources.Load<GameObject>("UI/BuildingInfoPopup")).GetComponent<BuildingInformationPopup>();
        RubbleInfoPopup = Instantiate(Resources.Load<GameObject>("UI/RubbleInfoPopup")).GetComponent<BuildingInformationPopup>();
        
        RubbleGo.SetActive(BuildingState == BuildingState.RUBBLE && CanBuild());
        GetComponent<BoxCollider>().enabled = BuildingState == BuildingState.NORMAL || BuildingState == BuildingState.HAZARDOUS || (BuildingState == BuildingState.RUBBLE && CanBuild());
        BuildingGo.SetActive(BuildingState == BuildingState.NORMAL || BuildingState == BuildingState.HAZARDOUS);

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
            ExteriorPopUI = Instantiate(Resources.Load<GameObject>(PopUILocation)).GetComponent<PopUI>();
            ExteriorPopUI.transform.SetParent(transform);
            ExteriorPopUI.transform.localPosition = new Vector3(0, 1, 0);
            ExteriorPopUI.gameObject.SetActive(false);
        }

        if (PopUIFX == null)
        {
            PopUIFX = Instantiate(Resources.Load("UI/PopUIFX") as GameObject).GetComponent<PopUIFX>();
            PopUIFX.gameObject.SetActive(false);
        }

        MyMissions = GameDataManager.Instance.GetBuildingMissionData(GetType().Name);

        var tile = GameManager.Instance.GetNextRandomMapTile(GetType().Name);
        if (!GameSettings.Instance.FTUE && tile != null)
        {
            CurrentGroundTile = tile;
            var newPos = CurrentGroundTile.transform.position;
            transform.position = new Vector3(newPos.x, newPos.y + 1.2f, newPos.z);
        }

        SetObjectiveParameters();
    }

    protected virtual void SetObjectiveParameters()
    {
        if (MyObjective == null) return;

        if(MyObjective.CustomEventId != CustomEventType.NONE && !(GameManager.Instance.SaveData.MissionEvents?.Contains(MyObjective.CustomEventId) ?? false))
        {
            EventsManager.Instance.AddEventToList(MyObjective.CustomEventId);
            EventsManager.Instance.TriggeredMissionEvents.Add(MyObjective.CustomEventId);
        }

        if(MyObjective.Event == BuildingEventType.DELIVER_ITEM)
        {
            RequiredItems = MyObjective.RequiredAmount;
            DeadlineSet = true;
            DeadlineTriggeredForTheDay = true;
            PopMyIcon();
            SoundManager.Instance.PlayOneShotSfx("Notification_SFX");
        }
        else if(MyObjective.Event == BuildingEventType.REPAIR)
        {
            var clock = GameManager.Instance.GameClock;
            TriggerHazardousMode(clock.Time, clock.Day);
        }
    }

    public virtual void GetInteriorPopUI()
    {

    }

    public void Init(int deadline, MapTile groundTile, TileData tileData, Sprite[] sprites, int sortingOrder = 0)
    {
        Init(groundTile, tileData, sprites, sortingOrder);
    }

    public override void Tick(double time, int day)
    {
         //   Debug.LogWarning($"{name}: Deadline: {DeadlineTime.Time} : DAY {DeadlineTime.Day} : {RequiredItems} Items!!");

        if(GameClock.DeltaTime)
        {
        //    SetDeadlineTime(time, day);
        }

        if(InteriorPopUI) //TEMP
            InteriorPopUI.Init(PopUICallback, GetType().Name, RequiredItems, DeadlineTime, this, InteriorCam.GetComponent<CameraControls>());
        ExteriorPopUI.Init(PopUICallback, GetType().Name, RequiredItems, DeadlineTime, this);

        if (BuildingState == BuildingState.NORMAL && GameManager.Instance.CurrentHouse == this && DuringOpenHours())
        {
            SoundManager.Instance.PlayHouseAmbience(GetType().Name, true, 0.3f);
        }
        if (BuildingState == BuildingState.NORMAL && GameManager.Instance.CurrentHouse == this && !DuringOpenHours())
        {
            SoundManager.Instance.PlayHouseAmbience(GetType().Name, false);
        }

        switch (BuildingActivityState)
        {
            case BuildingActivityState.VOLUNTEERING:
                VolunteerCountdown++;
                var extraPoints = 0;
                if (PopUI.CriticalHitCount == MaxVolunteerPoints) extraPoints = 1;
                OnActionProgress?.Invoke(VolunteerCountdown / MaxVolunteerPoints, this, 1);
                if (VolunteerCountdown >= MaxVolunteerPoints)
                {
                    BuildRelationship(ThankYouType.VOLUNTEER);
                    Player player = GameManager.Instance.Player;
                    var moddedEnergy = player.ModifyEnergyConsumption(amount: EnergyConsumption);
                    moddedEnergy += ModVolunteerEnergyWithProvisions();
                    player.ConsumeEnergy(EnergyConsumption);
                    UpdateCharityPoints(VolunteerPoints + extraPoints, moddedEnergy);
                    VolunteerCountdown = 0;
                    if(MyObjective?.Event == BuildingEventType.VOLUNTEER)
                    {
                        MissionManager.Instance.CompleteObjective(MyObjective);
                        MyObjective = null;
                    }
                }
                break;
        }

        BuildingActivityState = BuildingActivityState.NONE;

        if (CanBuild())
        {
            PopMyIcon("Rubble", 0, new GameClock(0, GameManager.Instance.GameClock.Day+1));
            RubbleGo.SetActive(true);
            GetComponent<BoxCollider>().enabled = true;
        }

        if (GetType().Name != "InteractableChurch" && GetType().Name != "InteractableMarket")
        {
            if (GameClock.DeltaTime)
            {
                if (WeatherManager.Instance.IsStormy())
                {
                    if(CurrentSturdyMaterials == 0)
                    {
                        if (BuildingState == BuildingState.NORMAL && Random.Range(0, 100) < EnvironmentalHazardDestructionChance)
                        {
                            //TriggerHazardousMode(time, day);
                        }
                    }
                    else
                    {
                        CurrentSturdyMaterials--;
                    }
                }
                if (BuildingState == BuildingState.HAZARDOUS)
                {
                    EnvironmentalHazardDestructionCountdown--;
                    //if (EnvironmentalHazardDestructionCountdown < 0)
                    //{
                    //    DestroyBuilding();
                    //}
                }
            }
        }
        if (BuildingState == BuildingState.HAZARDOUS)
        {
            PopMyIcon();
        }
    }

    public void DestroyBuilding()
    {
        BuildingState = BuildingState.RUBBLE;
        HasBeenDestroyed = true;
        BuildPoints = 0;
        RelationshipPoints = 0;
        RelationshipBonus = 0;
        SturdyMaterials = 0;
        PopIcon.gameObject.SetActive(false);
        UI.Instance.SideNotificationPop(GetType().Name + GetHazardIcon());
        Destroy(ExteriorPopUI.gameObject);
        Initialize();
        if (InsideHouse && CameraLockOnMe)
        {
            StartCoroutine(FadeAndSwitchCamerasAsync(InteriorLightsOff));
        }
        UpdateCharityPoints(-5, 0);
     
        SaveDataManager.Instance.SaveGame();
    }

    public void WeatherAlert(WeatherType weather, GameClock start, GameClock end)
    {
        if (weather != WeatherType.NONE) return;
        CurrentSturdyMaterials = SturdyMaterials;
    }

    public virtual void TriggerHazardousMode(double time, int day)
    {
        if (HazardCounter > 0) return;
     //   if (MissionManager.Instance.CurrentMission.CurrentWeek < 2) return;
        if (InsideHouse && CameraLockOnMe) return;
     //   if (time >= 21) return;

        BuildingState = BuildingState.HAZARDOUS;
        EnvironmentalHazardDestructionCountdown = 13;
        HazardCounter++;

        DeadlineTime.SetClock(-1, day);
        DeadlineSet = false;
        RequiredItems = 0;
        PopIcon.gameObject.SetActive(false);
        UI.Instance.SideNotificationPop(GetType().Name);
        OnActionProgress?.Invoke(1f, this, 1); //Reset all progress bars

        if(HouseUIActive)
            UI.Instance.SideNotificationPush(GetHazardIcon(), 0, new GameClock(GameManager.Instance.GameClock.Time + EnvironmentalHazardDestructionCountdown/2d, GameManager.Instance.GameClock.Day), GetType().Name + GetHazardIcon());
    }

    protected virtual int ModVolunteerEnergyWithProvisions()
    {
        return 0;
    }

    protected virtual void TryZoom(float zoom)
    {
        if (EventsManager.Instance.EventInProgress || CustomEventPopup.IsDisplaying) return;
        StartCoroutine("TryZoomAsync", zoom);
    }

    public static bool Zooming;
    IEnumerator TryZoomAsync(float zoom) 
    {
        if (GameSettings.Instance.FTUE && TutorialManager.Instance.CurrentTutorialStep < 2) yield break;

        InfoPopup.gameObject.SetActive(false);
        RubbleInfoPopup.gameObject.SetActive(false);
        if (!CameraLockOnMe || Zooming)
        {
            yield break;
        }
        Zooming = true;
        SoundManager.Instance.PlayOneShotSfx("Zoom_SFX");

        if (zoom > 0) //Zoom in
        {
            ExteriorCamera.Instance.GetComponent<CameraControls>().SetCameraTarget(transform.TransformPoint(-7.95f, 10.92f, -6.11f));
            ExteriorCamera.Instance.GetComponent<CameraControls>().SetZoomTarget(3f);
            ExteriorPopUI.gameObject.SetActive(true);
            HouseUIActive = true;
            PopIcon.gameObject.SetActive(false);
        }
        else
        {
            if (InteriorCam && InsideHouse)
            {
                PopUICallback("EXIT");
            }
            else
            {
                ExteriorCamera.Instance.GetComponent<CameraControls>().SetCameraTarget(Vector3.zero);
                ExteriorPopUI.gameObject.SetActive(false);
                HouseUIActive = false;
                PopIcon.UIPopped(false);
            }
        }
        var clock = GameManager.Instance.GameClock;
        clock.Ping();

        yield return new WaitForSeconds(0.5f);
        Zooming = false;
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
        if (time >= 17 || time < 6) return;
        if ((DeadlineTime.Time != -1)) return;
        if (DeadlineTriggeredForTheDay) return;

        switch (MissionDifficulty)
        {
            case MissionDifficulty.HARD:
                if (DeadlineCounter < 3)
                {
                    var mission = GetBuildingMission(BuildingEventType.DELIVER_ITEM);
                    if (mission != null || (!SameDayAsMission() && Random.Range(0, 100) < DeadlinePercentChance))
                    {
                        DeadlineCounter++;
                        if(time < 6)
                            DeadlineTime.SetClock(time + (mission != null ? mission.DeadlineHours : (6-time) + RandomFutureTimeByDifficulty()), day);
                        else
                            DeadlineTime.SetClock(time + (mission != null ? mission.DeadlineHours : RandomFutureTimeByDifficulty()), day);

                        if(MissionManager.Instance.CurrentMission.CurrentWeek > 2)
                            RequiredItems = mission != null ? mission.RequiredItems : Random.Range(1, 3); //Depending on Season
                        else
                            RequiredItems = mission != null ? mission.RequiredItems : 1; //Depending on Season

                        DeadlineDeliveryBonus = 1;
                        DeadlineSet = true;
                        DeadlineTriggeredForTheDay = true;
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
        DeadlineCounter = 0;
        DeadlineTriggeredForTheDay = false;
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

        UI.Instance.DisplayMessage("MEDITATED!!");
        PrayersProgress++;
        var extraPoints = 0;
        extraPoints += PopUI.CriticalHitCount == MaxPrayerProgress ? 1 : 0;
        OnActionProgress?.Invoke(PrayersProgress / MaxPrayerProgress, this, 0);
        if (PrayersProgress == MaxPrayerProgress)
        {
            var rosary = InventoryManager.Instance.GetProvision(Provision.ROSARY);
            var koboko = InventoryManager.Instance.GetProvision(Provision.KOBOKO);

            if(koboko != null)
            {
                extraPoints += koboko?.Value ?? 0;
                player.ConsumeEnergy(koboko.Value);
            }
            extraPoints += rosary?.Value ?? 0;

            UpdateFaithPoints(MeditationPoints + FPBonus + extraPoints);
            PrayersProgress = 0;
        }
        clock.Tick();
    }

    public virtual void DeliverItem(InteractableHouse house, bool autoDeliver = false)
    {
        if (house != this) return;

        RequiredItems--;
        PopMyIcon();
        if(InteriorPopUI)
            InteriorPopUI.Init(PopUICallback, GetType().Name, RequiredItems, DeadlineTime, this, InteriorCam.GetComponent<CameraControls>());
        ExteriorPopUI.Init(PopUICallback, GetType().Name, RequiredItems, DeadlineTime, this);

        if (RequiredItems <= 0)
        {
            DeadlineCounter = Mathf.Max(0, DeadlineCounter - 1);
            DeadlineTime.SetClock(-1, DeadlineTime.Day);
            DeadlineDeliveryBonus = 1;
            DeadlineSet = false;
            RequiredItems = 0;
            PopIcon.gameObject.SetActive(false);
            UI.Instance.SideNotificationPop(GetType().Name);
            if(MyObjective?.Event == BuildingEventType.DELIVER_ITEM)
            {
                MissionManager.Instance.CompleteObjective(MyObjective);
                MyObjective = null;
            }

            BuildRelationship(ThankYouType.ITEM);
            if (!autoDeliver)
            {
                GameClock.ExecuteEvents?.Invoke();
            }
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

    public virtual void TriggerStory()
    {
        if (HasBeenDestroyed) return;

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
        yield break;
        //while(stack.Count > 0)
        //{
        //    var item = stack.Pop();
        //    PopUIFXIcons(item.Item1, item.Item2);
        //    yield return new WaitForSeconds(0.5f);
        //}
    }

    public virtual void Build()
    {
        Player player = GameManager.Instance.Player;
        if (player.EnergyDepleted() || !CanBuild())
        {
            UI.Instance.ErrorFlash("Energy");
            return;
        }

        BuildPoints++;
        var extraPoints = 0;
        if (PopUI.CriticalHitCount == MaxBuildPoints) extraPoints = 1;
        OnActionProgress?.Invoke(BuildPoints / MaxBuildPoints, this, 0);

        UI.Instance.DisplayMessage("BUILDING!");
        if(BuildPoints >= MaxBuildPoints)
        {
            //Play Cool Construction Vfx and Animation!
            BuildingState = BuildingState.NORMAL;
            PopUILocation = OriginalPopUILocation;
            Destroy(ExteriorPopUI.gameObject);
            Initialize();
            StartCoroutine(ClearToolTipAfterBuildingAsync());
            StartCoroutine(FadeAndSwitchCamerasAsync(InteriorLightsOn));
            SoundManager.Instance.PlayOneShotSfx("Success_SFX", 1f, 5f);
            SoundManager.Instance.PlayHouseAmbience("Construction", false, 0.3f);
            SoundManager.Instance.PlayOneShotSfx("Cheer_SFX", 1f,5f);
            if (DuringOpenHours())
            {
                SoundManager.Instance.PlayHouseAmbience(GetType().Name, true, 0.3f);
            }
            if (!HasBeenDestroyed)
            {
                BuildingCompleteDialog();
            }
            var moddedEnergy = player.ModifyEnergyConsumption(amount: EnergyConsumption);
            player.ConsumeEnergy(EnergyConsumption);
            var tents = InventoryManager.Instance.GetProvision(Provision.CONSTRUCTION_TENTS);
            var moddedCPReward = extraPoints + (tents?.Value ?? 0);
            UpdateCharityPoints(1 + moddedCPReward, moddedEnergy);

            var buildingBlueprint = InventoryManager.Instance.GetProvision(Provision.BUILDING_BLUEPRINT);
            RelationshipBonus= buildingBlueprint?.Value ?? 0;

            var chapelBlueprint = InventoryManager.Instance.GetProvision(Provision.CHAPEL_BLUEPRINT);
            FPBonus = chapelBlueprint?.Value ?? 0;

            //Reduce chance of environmental hazards
            var sturdyMaterials = InventoryManager.Instance.GetProvision(Provision.STURDY_BUILDING_MATERIALS);
            SturdyMaterials = sturdyMaterials?.Value ?? 0;
            CurrentSturdyMaterials = SturdyMaterials;
            UI.Instance.SideNotificationPop(GetType().Name);
            InsideHouse = true;
            GamepadCursor.CursorSpeed = 2000f;

            if(MyObjective?.Event == BuildingEventType.CONSTRUCT)
            {
                MissionManager.Instance.CompleteObjective(MyObjective);
                MyObjective = null;
            }
        }
        else
        {
            SoundManager.Instance.PlayOneShotSfx("Build_SFX", 1f, 5f);
        }
        GameClock clock = GameManager.Instance.GameClock;
        clock.Tick();
    }

    private IEnumerator ClearToolTipAfterBuildingAsync()
    {
        yield return null;
        UI.Instance.TooltipDisplay.transform.parent.gameObject.SetActive(false);
    }

    public virtual void BuildingCompleteDialog()
    {
        switch (GetType().Name)
        {
            case "InteractableHospital":
                if(!EventsManager.Instance.TriggeredMissionEvents.Contains(CustomEventType.HOSPITAL_COMPLETE))
                    EventsManager.Instance.AddEventToList(CustomEventType.HOSPITAL_COMPLETE);
                break;
            case "InteractableOrphanage":
                if(!EventsManager.Instance.TriggeredMissionEvents.Contains(CustomEventType.ORPHANAGE_COMPLETE))
                    EventsManager.Instance.AddEventToList(CustomEventType.ORPHANAGE_COMPLETE);
                break;
            case "InteractableKitchen":
                if(!EventsManager.Instance.TriggeredMissionEvents.Contains(CustomEventType.KITCHEN_COMPLETE))
                    EventsManager.Instance.AddEventToList(CustomEventType.KITCHEN_COMPLETE);
                break;
            case "InteractableShelter":
                if(!EventsManager.Instance.TriggeredMissionEvents.Contains(CustomEventType.SHELTER_COMPLETE))
                    EventsManager.Instance.AddEventToList(CustomEventType.SHELTER_COMPLETE);
                break;
            case "InteractableSchool":
                if(!EventsManager.Instance.TriggeredMissionEvents.Contains(CustomEventType.SCHOOL_COMPLETE))
                    EventsManager.Instance.AddEventToList(CustomEventType.SCHOOL_COMPLETE);
                break;
            case "InteractableClothesBank":
                if(!EventsManager.Instance.TriggeredMissionEvents.Contains(CustomEventType.CLOTHES_COMPLETE))
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

            case "EXIT":
                SoundManager.Instance.PlayHouseAmbience(GetType().Name, false, 0.3f);
                InsideHouse = false;
                OnEnterHouse?.Invoke(InsideHouse);
                StartCoroutine(FadeAndSwitchCamerasAsync(InteriorLightsOff));
                break;

            case "WORLD":
                if(GetType().Name == "InteractableChurch")
                {
                    foreach (var house in GameManager.Instance.Houses)
                    {
                        if (house.MyObjective != null && house.MyObjective.Event == BuildingEventType.DELIVER_ITEM)
                        {
                            UI.Instance.EnablePackageSelector(true, this);
                            return;
                        }
                    }
                    if(!InventoryManager.HasChosenProvision)
                    {
                        InventoryManager.Instance.GenerateProvisionsForNewDay();
                        return;
                    }
                }

                GoToWorldMap();

                break;

            case "ENTER":
                OnPlayerMoved(GameManager.Instance.Player.Energy, this);
                break;
        }
    }

    public void GoToWorldMap()
    {
        StartCoroutine(GoToWorldMapAsync());
    }

    IEnumerator GoToWorldMapAsync()
    {
        UI.Instance.EnablePackageSelector(false);

        SoundManager.Instance.PlayHouseAmbience(GetType().Name, false, 0.3f);
        InsideHouse = false;
        OnEnterHouse?.Invoke(InsideHouse);
        yield return StartCoroutine(FadeAndSwitchCamerasAsync(InteriorLightsOff));
        GameManager.Instance.ExitHouse();
    }

    public void InteriorLightsOff()
    {
        ExteriorPopUI.gameObject.SetActive(true);
        ExteriorPopUI.Init(PopUICallback, GetType().Name, RequiredItems, DeadlineTime, this, InteriorCam.GetComponent<CameraControls>());

        InteriorPopUI.gameObject.SetActive(false);
        InteriorCam.enabled = false;
        InteriorUICamera.enabled = false;
        ExteriorCamera.Instance.Camera.enabled = true;
        ExteriorCamera.Instance.UICamera.enabled = true;
        ExteriorCamera.Instance.GetComponent<CameraControls>().SetZoomTarget(3f);
        InteriorCam.GetComponent<CameraControls>().SetZoomTarget(7f);
        InteriorSpace.SetActive(false);
    }

    public void InteriorLightsOn()
    {
        ExteriorPopUI.gameObject.SetActive(false);
        if(!EventsManager.Instance.EventInProgress)
            InteriorPopUI.gameObject.SetActive(true);
        InteriorPopUI.Init(PopUICallback, GetType().Name, RequiredItems, DeadlineTime, this, InteriorCam == null ? null : InteriorCam?.GetComponent<CameraControls>());
        PopIcon.UIPopped(true);
        InteriorCam.enabled = true;
        InteriorUICamera.enabled = true;
        ExteriorCamera.Instance.Camera.enabled = false;
        ExteriorCamera.Instance.UICamera.enabled = false;
        InteriorSpace.SetActive(true);
    }

    public virtual void VolunteerWork(InteractableHouse house)
    {
    }

    public virtual void UpdateCharityPoints(int amount, int energy)
    {
        CustomEventData e = EventsManager.Instance.CurrentEvents.Find(i => i.Id == CustomEventType.HIGH_MORALE || i.Id == CustomEventType.LOW_MORALE);
        int charityMultiplier = 1;
        if (e != null && BuildingState != BuildingState.HAZARDOUS && amount > 0)
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

    public virtual void UpdateFaithPoints(int amount, int energy = -1)
    {
        CustomEventData e = EventsManager.Instance.CurrentEvents.Find(i => i.Id == CustomEventType.HIGH_SPIRIT || i.Id == CustomEventType.LOW_SPIRIT);

        int faithBonus = 0;
        if(e != null)
        {
            if(Random.Range(0,100) < 20)
            {
                faithBonus += e.Id == CustomEventType.HIGH_SPIRIT ? (int)e.Gain : 0;
            }
        }
        Stack<Tuple<string, int>> stack = new Stack<Tuple<string, int>>();
        stack.Push(new Tuple<string, int>("InteractableChurch", amount * faithBonus));
        if (energy != 0) stack.Push(new Tuple<string, int>("Energy", energy));
        StartCoroutine(PopUIFXIconsAsync(stack));

        GameManager.Instance.Player.ConsumeEnergy(energy);
        MissionManager.Instance.UpdateFaithPoints(amount + faithBonus);
    }

    public virtual void ReportScores()
    {
        if (DeadlineSet) UpdateCharityPoints(-1, 0);
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
            if (BuildingState == BuildingState.HAZARDOUS)
            {
                UI.Instance.SideNotificationPush(GetHazardIcon(), 0, new GameClock(GameManager.Instance.GameClock.Time + EnvironmentalHazardDestructionCountdown/2d, GameManager.Instance.GameClock.Day), GetType().Name + GetHazardIcon());
            }
            else
            {
                UI.Instance.SideNotificationPush(name, items, time, GetType().Name);
            }
            PopIcon.gameObject.SetActive(false);
            return;
        }
        else
        {
            UI.Instance.SideNotificationPop(GetType().Name);
            UI.Instance.SideNotificationPop(GetType().Name+GetHazardIcon());
        }

        PopIcon.gameObject.SetActive(true);
        if (BuildingState == BuildingState.HAZARDOUS)
        {
            PopIcon.Init(GetHazardIcon(), items, new GameClock(GameManager.Instance.GameClock.Time + EnvironmentalHazardDestructionCountdown / 2d));
        }
        else
        {
            PopIcon.Init(name, items, time);
        }
    }

    public virtual string GetHazardIcon()
    {
        return "Hazard";
    }

    public IEnumerator FadeAndSwitchCamerasAsync(Action callback)
    {
        if (UI.Instance.WeekBeginCrossFade)
        {
            callback();
            yield break;
        }

        UI.Instance.CrossFade(1f, 15f);
        while (UI.Instance.CrossFading) yield return null;

        if (MissionManager.MissionOver) yield break;

        callback();

        UI.Instance.CrossFade(0f, 15f);
    }

    public override void OnPlayerMoved(Energy energy, MapTile tile)
    {
        base.OnPlayerMoved(energy, tile);
        if (tile.GetInstanceID() == GetInstanceID())
        {
            var tools = InventoryManager.Instance.GetProvision(Provision.CONSTRUCTION_TOOLS);
            if (tools != null)
            {
                MaxBuildPoints = tools.Value;
            }

            MaxVolunteerPoints = CalculateMaxVolunteerPoints();
            var rosary = InventoryManager.Instance.GetProvision(Provision.ROSARY);
            MaxPrayerProgress = rosary != null ? 5f : 4f;

            if (GameManager.Instance.CurrentHouse != this) //Entered a new building
            {
                if(BuildingState == BuildingState.HAZARDOUS)
                {
                    TriggerHazardousEvent();
                }
                else
                {
                    TriggerCustomEvent();
                }
            }

            GameManager.Instance.CurrentHouse = this;
            GameManager.Instance.CurrentBuilding = GameManager.Instance.CurrentHouse.GetType().Name;
            ExteriorCamera.Instance.GetComponent<CameraControls>().SetCameraTarget(transform.TransformPoint(-7.95f, 10.92f, -6.11f));
            ExteriorCamera.Instance.GetComponent<CameraControls>().SetZoomTarget(3f);
            if (InteriorCam)
            {
                ExteriorCamera.Instance.GetComponent<CameraControls>().SetZoomTarget(2.5f);
                InteriorCam.GetComponent<CameraControls>().SetCameraTarget(InteriorSpace.transform.TransformPoint(3.68f, 7.44f, -1.12f), false);
                InteriorCam.GetComponent<CameraControls>().SetZoomTarget(6f);
            }
            CameraLockOnMe = true;
            HouseUIActive = true;
            InsideHouse = true;
            PopIcon.gameObject.SetActive(false);
            SoundManager.Instance.PlayOneShotSfx("Zoom_SFX");
            if(DuringOpenHours())
            {
                if (BuildingState == BuildingState.NORMAL) //TODO: IF HAZARDOUS, PLAY HAZARDOUS AMBIENCE
                    SoundManager.Instance.PlayHouseAmbience(GetType().Name, true, 0.3f);
            }
            if (BuildingState == BuildingState.RUBBLE)
                SoundManager.Instance.PlayHouseAmbience("Construction", true, 0.3f);
            SoundManager.Instance.FadeAmbience(0.1f);
            OnEnterHouse?.Invoke(InsideHouse);
            UI.Instance.RefreshTreasuryBalance(0);

            if (GameManager.Instance.Player.StatusEffects.Contains(PlayerStatusEffect.FROZEN))
            {
                GameManager.Instance.Player.StatusEffects.Remove(PlayerStatusEffect.FROZEN);
            }
        }
        else if(CameraLockOnMe)
        {
            ExteriorCamera.Instance.GetComponent<CameraControls>().SetCameraTarget(Vector3.zero);
            if (InteriorCam)
            {
                InteriorCam.GetComponent<CameraControls>().SetCameraTarget(Vector3.zero, false);
                InteriorCam.GetComponent<CameraControls>().SetZoomTarget(12f);
                InteriorPopUI.gameObject.SetActive(false);
            }
            CameraLockOnMe = false;
            HouseUIActive = false;
            InsideHouse = false;
            SoundManager.Instance.PlayOneShotSfx("Zoom_SFX");
            if (BuildPoints >= MaxBuildPoints)
                SoundManager.Instance.PlayHouseAmbience(GetType().Name, false, 0.3f);
            else
                SoundManager.Instance.PlayHouseAmbience("Construction", false, 0.3f);
            SoundManager.Instance.FadeAmbience(0.3f);
            OnEnterHouse?.Invoke(InsideHouse);
            ResetActionProgress();
            GameManager.Instance.CurrentHouse = null;
        }

        InfoPopup.gameObject.SetActive(false);
        RubbleInfoPopup.gameObject.SetActive(false);

        if (CameraLockOnMe && BuildingState == BuildingState.RUBBLE && InsideHouse)
        {
            PopUICallback("EXIT");
        }
    }

    public virtual void TriggerCustomEvent()
    {
        if (GameSettings.Instance.FTUE) return;
        if (EventsTriggered > 0) return;
        if (EventsManager.Instance.EventInProgress) return;
        if (BuildingState != BuildingState.NORMAL) return;
        if (!DuringOpenHours()) return;
     //   if (EventsManager.Instance.CurrentEvents.Count > 3) return;
        if (UI.Instance.WeekBeginCrossFade) return;
        if (GameManager.Instance.PreviousSceneID == SceneID.SaintsShowcase_Day) return;
        if (DeadlineSet) return;
        if (GameManager.Instance.Player.StatusEffects.Count > 0) return;
        if (GameManager.Instance.GameClock.Time > 22.5 || GameManager.Instance.GameClock.Time < 6) return;

        if (Random.Range(0, 100) < 100)
        {
            CustomEventType triggeredEvent = CustomEventType.NONE;
            switch (GetType().Name)
            {
                case "InteractableChurch":
                    triggeredEvent = GameDataManager.Instance.GetRandomEvent(EventGroup.CHURCH).Id;
                    EventsManager.Instance.AddEventToList(triggeredEvent);
                    break;
                case "InteractableHospital":
                    triggeredEvent = GameDataManager.Instance.GetRandomEvent(EventGroup.HOSPITAL).Id;
                    EventsManager.Instance.AddEventToList(triggeredEvent);
                    break;
                case "InteractableKitchen":
                    triggeredEvent = GameDataManager.Instance.GetRandomEvent(EventGroup.KITCHEN).Id;
                    EventsManager.Instance.AddEventToList(triggeredEvent);
                    break;
                case "InteractableOrphanage":
                    triggeredEvent = GameDataManager.Instance.GetRandomEvent(EventGroup.ORPHANAGE).Id;
                    EventsManager.Instance.AddEventToList(triggeredEvent);
                    break;
                case "InteractableShelter":
                    triggeredEvent = GameDataManager.Instance.GetRandomEvent(EventGroup.SHELTER).Id;
                    EventsManager.Instance.AddEventToList(triggeredEvent);
                    break;
                case "InteractableSchool":
                    triggeredEvent = GameDataManager.Instance.GetRandomEvent(EventGroup.SCHOOL).Id;
                    EventsManager.Instance.AddEventToList(triggeredEvent);
                    break;
            }
            HouseTriggeredEvent = triggeredEvent;
            EventsTriggered++;
        }
    }

    public virtual void TriggerHazardousEvent()
    {
        switch (GetType().Name)
        {
            case "InteractableHospital":
                EventsManager.Instance.AddEventToList(CustomEventType.SAVE_HOSPITAL);
                break;
            case "InteractableKitchen":
                EventsManager.Instance.AddEventToList(CustomEventType.SAVE_KITCHEN);
                break;
            case "InteractableOrphanage":
                EventsManager.Instance.AddEventToList(CustomEventType.SAVE_ORPHANAGE);
                break;
            case "InteractableShelter":
                EventsManager.Instance.AddEventToList(CustomEventType.SAVE_SHELTER);
                break;
            case "InteractableSchool":
                EventsManager.Instance.AddEventToList(CustomEventType.SAVE_SCHOOL);
                break;
        }
    }

    public virtual void ClearHazard()
    {
        if (BuildingState != BuildingState.HAZARDOUS) return;

        BuildingState = BuildingState.NORMAL;
        PopIcon.gameObject.SetActive(false);
        UI.Instance.SideNotificationPop(GetType().Name + GetHazardIcon());
        if (MyObjective?.Event == BuildingEventType.REPAIR)
        {
            MissionManager.Instance.CompleteObjective(MyObjective);
            MyObjective = null;
        }
    }

    public virtual void ResetActionProgress()
    {
        VolunteerCountdown = 0;
        BuildingActivityState = BuildingActivityState.NONE;
    }

    public virtual bool HasResetActionProgress()
    {
        return VolunteerCountdown == 0 && PrayersProgress == 0 || (BuildingState == BuildingState.RUBBLE && BuildPoints == 0);
    }

    private void OnUIEnabled(bool enabled)
    {
        if (CameraLockOnMe)
        {
            ExteriorPopUI.gameObject.SetActive(enabled);
            if(InteriorPopUI && InteriorSpace.activeSelf)
            InteriorPopUI.gameObject.SetActive(enabled);
        }

        if(CanBuild() || DeadlineSet || GetType().Name == "InteractableChurch") //Only reason to have popicons displaying
        {
            if(enabled && !HouseUIActive)
                PopIcon.gameObject.SetActive(true);            
            PopIcon.UIPopped(!enabled);
            if (CanBuild()) PopIcon.Init("Rubble", 0, new GameClock(-1));
            else PopMyIcon();
        }
    }

    public float CalculateMaxVolunteerPoints()
    {
        if (RelationshipPoints >= 65)
        {
            return 1;
        }
        else if (RelationshipPoints >= 30)
        {
            return 2;
        }
        else if (RelationshipPoints >= 5) //ONLY FOR THE DEMO!
        {
            return 3;
        }
        else
        {
            return 4;
        }
    }

    public int GetNextRPMilestone()
    {
        if (RelationshipPoints >= 65)
        {
            return 100;
        }
        else if (RelationshipPoints >= 30)
        {
            return 65;
        }
        else if (RelationshipPoints >= 10)
        {
            return 30;
        }

        return 10;
    }

    public virtual void BuildRelationship(ThankYouType thanks, int amount = 1)
    {
        RelationshipPoints += Mathf.Clamp(amount + RelationshipBonus, 0, 100);
        RelationshipReward(thanks);
        SoundManager.Instance.PlayOneShotSfx("Success_SFX", 1f, 5f);
    }

    public virtual void RelationshipReward(ThankYouType thanks)
    {
        //Add (or subtract) a Season Bonus
        if(RelationshipPoints >= 65)
        {
            //Special Item
            ThankYouMessage(thanks);

            if (Random.Range(0, 100) < 50)
            {
                MoneyThanks();
                TreasuryManager.Instance.DonateMoney(Random.Range(4, 5));
            }
        }
        else if (RelationshipPoints >= 30)
        {
            ThankYouMessage(thanks);

            if(Random.Range(0,100) < 50)
            {
                MoneyThanks();
                TreasuryManager.Instance.DonateMoney(Random.Range(3,4));
            }
        }
        else if (RelationshipPoints >= 10)
        {
            ThankYouMessage(thanks);

            if(Random.Range(0,100) < 50)
            {
                MoneyThanks();
                TreasuryManager.Instance.DonateMoney(Random.Range(2, 3));
            }
        }
        else if (RelationshipPoints > 1)
        {
            ThankYouMessage(thanks);
            if (Random.Range(0, 100) < 50)
            {
                MoneyThanks();
                TreasuryManager.Instance.DonateMoney(Random.Range(1, 2));
            }
        }
        else if (RelationshipPoints == 1)
        {
            ThankYouMessage(thanks);
            MoneyThanks();
            TreasuryManager.Instance.DonateMoney(Random.Range(1, 2));
        }
        TriggerStory();
        SaveDataManager.Instance.SaveGame();
    }

    public bool CanBuild()
    {
        if (BuildPoints >= MaxBuildPoints || BuildingState != BuildingState.RUBBLE || MyObjective == null) return false;

        if (MyObjective?.Event == BuildingEventType.CONSTRUCT) return true;

        return false;
    }

    protected virtual void OnEventExecuted(CustomEventData e)
    {

    }

    public virtual bool CanDoAction(string actionName)
    {
        switch (actionName)
        {
            case "BUILD":
                return !GameManager.Instance.Player.EnergyDepleted() && CanBuild();

            case "PRAY": return true;
            case "SLEEP": return true;// MissionManager.Instance.CurrentObjectives.Any(obj => obj.Event == BuildingEventType.RETURN);
            case "EXIT": return true;
            case "WORLD": return true;
            case "ENTER": return true;
            case "SAINTS": return true;
        }

        return false;
    }

    public virtual float SetButtonTimer(string actionName)
    {
        return 1f;
    }

    public virtual int GetEnergyCostForCustomEvent(int eventCost)
    {
        return 1;
    }

    public virtual TooltipStats GetTooltipStatsForButton(string button)
    {
        switch (button)
        {
            case "PRAY":
                if (MaxPrayerProgress - PrayersProgress == 1)
                {
                    var rosary = InventoryManager.Instance.GetProvision(Provision.ROSARY);
                    var koboko = InventoryManager.Instance.GetProvision(Provision.KOBOKO);
                    return GameDataManager.Instance.GetToolTip(TooltipStatId.PRAY, energyModifier: koboko?.Value ?? 0, fpModifier: FPBonus + rosary?.Value ?? 0);
                }
                else
                    return GameDataManager.Instance.GetToolTip(TooltipStatId.TIME);
            case "VOLUNTEER":
                if (4 - VolunteerCountdown == 1)
                    return GameDataManager.Instance.GetToolTip(TooltipStatId.VOLUNTEER, energyModifier: -GameManager.Instance.Player.ModifyEnergyConsumption(amount: EnergyConsumption));
                else
                    return GameDataManager.Instance.GetToolTip(TooltipStatId.TIME);

            case "CONSTRUCT":
                if (MaxBuildPoints - BuildPoints == 1)
                {
                    var tents = InventoryManager.Instance.GetProvision(Provision.CONSTRUCTION_TENTS);
                    return GameDataManager.Instance.GetToolTip(TooltipStatId.CONSTRUCT, cpModifier: tents?.Value ?? 0, energyModifier: -GameManager.Instance.Player.ModifyEnergyConsumption(amount: EnergyConsumption));
                }
                else
                    return GameDataManager.Instance.GetToolTip(TooltipStatId.TIME);
        }

        return new TooltipStats() { Ticks = 0, FP = 0, CP = 0, Energy = 0 };
    }

    public override void Hover()
    {
        if (UI.Instance.WeekBeginCrossFade) return;
        if (EventsManager.Instance.EventInProgress) return;
        if (HouseUIActive || EventsManager.Instance.HasEventsInQueue()) return;
        if (!CameraControls.ZoomComplete) return;

        if (!InfoPopup.gameObject.activeSelf)
        {
            BuildingGo.transform.DOComplete();
            BuildingGo.transform.DOPunchScale(transform.localScale * 0.15f, 0.5f, elasticity: 0f);
        }
        if (BuildingState == BuildingState.RUBBLE)
        {
            RubbleInfoPopup.gameObject.SetActive(true);
            if (CanBuild()) RubbleInfoPopup.UpdateReadyForConstruction();
            if(GameManager.Instance.Player.WeCanMove(CurrentGroundTile))
                ToolTipManager.Instance.ShowToolTip("Tooltip_ConstructionSite", GameDataManager.Instance.GetToolTip(TooltipStatId.MOVE, energyModifier: -GameManager.Instance.Player.ModifyEnergyConsumption(CurrentGroundTile, true)));
            else
                ToolTipManager.Instance.ShowToolTip("Tooltip_ConstructionSite");
        }
        else
        {
            InfoPopup.gameObject.SetActive(true);

            if (GameManager.Instance.Player.WeCanMove(CurrentGroundTile))
                ToolTipManager.Instance.ShowToolTip("Tooltip_"+GetType().Name, GameDataManager.Instance.GetToolTip(TooltipStatId.MOVE, energyModifier: -GameManager.Instance.Player.ModifyEnergyConsumption(CurrentGroundTile, true)));
            else
                ToolTipManager.Instance.ShowToolTip("Tooltip_" + GetType().Name);
        }

        InfoPopup.Init(GetType().Name, OpenTime, ClosingTime, RelationshipPoints, DuringOpenHours());
        PopIcon.gameObject.SetActive(false);
        base.Hover();
    }

    private bool HouseJumping = false;
    public virtual void HouseJump()
    {
        if (HouseJumping) return;

        SoundManager.Instance.PlayOneShotSfx("HouseJump_SFX");
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

    public override void HoverExit()
    {
        if (UI.Instance.WeekBeginCrossFade) return;
        if (EventsManager.Instance.EventInProgress) return;
        if (HouseUIActive) return;

        RubbleInfoPopup.gameObject.SetActive(false);
        InfoPopup.gameObject.SetActive(false);
        ToolTipManager.Instance.ShowToolTip("");
        var clock = GameManager.Instance.GameClock;
        clock.Ping();
        base.HoverExit();
    }

    public virtual HouseSaveData LoadData()
    {
        var data = GameManager.Instance.SaveData.Houses?.Where(h => h.HouseName == GetType().Name).FirstOrDefault();
        if (data == null)
        {
            if ((GameSettings.Instance.DEMO_MODE && !GameSettings.Instance.FTUE && GetType().Name != "InteractableClothesBank") || GetType().Name == "InteractableChurch" || GetType().Name == "InteractableMarket")
            {
                BuildingState = BuildingState.NORMAL;
            }
            else
            {
                BuildingState = BuildingState.RUBBLE;
            }

            return data;
        }

        if (GameSettings.Instance.DEMO_MODE && !GameSettings.Instance.FTUE && GameManager.Instance.GameClock.Day == 1 && GameManager.Instance.GameClock.Time == 6 && GetType().Name != "InteractableClothesBank")
        {
            BuildingState = BuildingState.NORMAL;
        }
        else
        {
            BuildingState = data.BuildingState;
        }
        FPBonus = data.FPBonus;
        RelationshipBonus = data.RelationshipBonus;
        RelationshipPoints = data.RelationshipPoints;
        SturdyMaterials = data.SturdyMaterials;
        DeadlineSet = data.DeadlineSet;
        DeadlineCounter = data.DeadlineCounter;
        DeadlineTime = new GameClock(data.DeadlineTime, data.DeadlineDay);
        RequiredItems = data.RequiredItems;
        EnvironmentalHazardDestructionCountdown = data.EnvironmentalHazardDestructionCountdown;
        HazardCounter = data.HazardCounter;
        HasBeenDestroyed = data.HasBeenDestroyed;
        DeadlineTriggeredForTheDay = data.DeadlineTriggeredForTheDay;
        MyStoryEvents = data.MyStoryEvents;

        return data;
    }

    public virtual HouseSaveData GetHouseSave()
    {
        return new HouseSaveData()
        {
            HouseName = GetType().Name,
            BuildingState = BuildingState,
            FPBonus = FPBonus,
            RelationshipBonus = RelationshipBonus,
            RelationshipPoints = RelationshipPoints,
            SturdyMaterials = SturdyMaterials,
            DeadlineSet = DeadlineSet,
            DeadlineCounter = DeadlineCounter,
            DeadlineTime = DeadlineTime.Time,
            DeadlineDay = DeadlineTime.Day,
            DeadlineTriggeredForTheDay = DeadlineTriggeredForTheDay,
            RequiredItems = RequiredItems,
            EnvironmentalHazardDestructionCountdown = EnvironmentalHazardDestructionCountdown,
            HazardCounter = HazardCounter,
            HasBeenDestroyed = HasBeenDestroyed,
            MyStoryEvents = MyStoryEvents
        };
    }

    protected virtual void AutoDeliver(ItemType item)
    {
        DeliverItem(this, true);
    }

    public void OverrideState(int missionId)
    {
        switch (GetType().Name)
        {
            case "InteractableHospital":
                if (missionId > 1)
                {
                    BuildingState = BuildingState.NORMAL;
                }    
                break;
            case "InteractableOrphanage":
                if (missionId > 2)
                {
                    BuildingState = BuildingState.NORMAL;
                }
                break;
            case "InteractableKitchen":
                if (missionId > 5)
                {
                    BuildingState = BuildingState.NORMAL;
                }
                break;
            case "InteractableShelter":
                if (missionId > 4)
                {
                    BuildingState = BuildingState.NORMAL;
                }
                break;
            case "InteractableSchool":
                if (missionId > 3)
                {
                    BuildingState = BuildingState.NORMAL;
                }
                break;
            case "InteractableClothesBank":
                if (missionId > 6)
                {
                    BuildingState = BuildingState.NORMAL;
                }
                break;
        }
    }

    public override void OnDisable()
    {
        InteractableMarket.AutoDeliverToHouse -= AutoDeliver;
        UI.Meditate -= Meditated;
        MissionManager.EndOfDay -= EndofDay;
        MissionManager.EndOfDay -= ReportScores;
        UI.UIHidden -= OnUIEnabled;
        EventsManager.EventExecuted -= OnEventExecuted;
        GameControlsManager.TryZoom -= TryZoom;
        WeatherManager.WeatherForecastActive -= WeatherAlert;

        HouseUIActive = false;
        base.OnDisable();
    }
}
