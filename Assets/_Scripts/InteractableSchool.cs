using UnityEngine;

public class InteractableSchool : InteractableHouse
{
    public int TeachPoints;
    private int TeachCountdown = 0;
    private float MaxTeachPoints = 4f;

    protected override void Start()
    {
        PopUILocation = "UI/ExternalUI";
        base.Start();
    }

    public override void GetInteriorPopUI()
    {
        InteriorPopUI = UI.Instance.transform.Find("SchoolUI").GetComponent<PopUI>();
        base.GetInteriorPopUI();
    }

    public override void OnPlayerMoved(Energy energy, MapTile tile)
    {
        base.OnPlayerMoved(energy, tile);
        GameClock clock = GameManager.Instance.GameClock;
        if (tile.GetInstanceID() == GetInstanceID())
        {
            if (BuildingState != BuildingState.RUBBLE)
            {
                StartCoroutine(FadeAndSwitchCamerasAsync(InteriorLightsOn));
                MaxVolunteerPoints = CalculateMaxVolunteerPoints();
            }
            else
            {
                ExteriorPopUI.gameObject.SetActive(true);
                ExteriorPopUI.Init(PopUICallback, GetType().Name, RequiredItems, DeadlineTime, this, InteriorCam == null ? null : InteriorCam?.GetComponent<CameraControls>());
                PopIcon.UIPopped(true);
            }
        }
        else
        {
            ExteriorPopUI.gameObject.SetActive(false);
            PopIcon.UIPopped(false);
        }
    }

    public override float CalculateMaxVolunteerPoints(int amount = 6)
    {
        return base.CalculateMaxVolunteerPoints(amount);
    }

    protected override void SetObjectiveParameters()
    {
        if (MyObjective == null) return;

        base.SetObjectiveParameters();
    }

    public void Teach()
    {
        GameClock clock = GameManager.Instance.GameClock;
        Player player = GameManager.Instance.Player;
        if (player.EnergyDepleted())
        {
            UI.Instance.ErrorFlash("Energy");
            return;
        }
        if (DuringOpenHours() || (!DuringOpenHours() && TeachCountdown > 0))
        {
            BuildingActivityState = BuildingActivityState.VOLUNTEERING;
            UI.Instance.DisplayMessage("Taught a Class!!");
            for (int i = 0; i < MaxVolunteerPoints; i++)
            {
                BuildingActivityState = BuildingActivityState.VOLUNTEERING;
                clock.Tick();
            }
        }
        else
        {
            UI.Instance.DisplayMessage("SCHOOL CLOSED!");
        }
    }

    public override void Tick(double time, int day)
    {
        base.Tick(time, day);
    }

    public override void TriggerHazardousMode(double time, int day)
    {
     //   if (HazardCounter > 0) return;
     //   if (MissionManager.Instance.CurrentMission.CurrentWeek < 2) return;

        TeachCountdown = 0;
        base.TriggerHazardousMode(time, day);
    }

    public override void BuildRelationship(ThankYouType thanks, int amount = 1)
    {
        if(thanks == ThankYouType.VOLUNTEER)
        {
            var schoolMaterials = InventoryManager.Instance.GetProvision(Provision.SCHOOL_RELATIONSHIP_BUILDER);
            amount += schoolMaterials?.Value ?? 0;
        }
        base.BuildRelationship(thanks, amount);
    }

    public override void DeliverItem(InteractableHouse house, bool autoDeliver = false)
    {
        if (house != this) return;

        ItemType item = InventoryManager.Instance.GetItem(ItemType.STATIONERY);

        if (item != ItemType.NONE)
        {
            UI.Instance.DisplayMessage("DELIVERED STATIONERY!");
            UpdateCharityPoints(ItemDeliveryPoints * DeadlineDeliveryBonus, 0);
            base.DeliverItem(house, autoDeliver);
        }
        else
        {
            UI.Instance.DisplayMessage("YOU HAVE NO STATIONERY TO GIVE!");
        }
    }

    public override void ItemDeliveryThanks()
    {
        EventsManager.Instance.AddEventToList(CustomEventType.THANKYOU_ITEM_SCHOOL);
        base.ItemDeliveryThanks();
    }

    public override void UpgradeThanks()
    {
        EventsManager.Instance.AddEventToList(CustomEventType.THANKYOU_UPGRADE_SCHOOL);
    }

    public override bool DuringOpenHours(GameClock newClock = null)
    {
        GameClock clock = newClock ?? GameManager.Instance.GameClock;
        return base.DuringOpenHours() && clock.Day <= 5;
    }

    public override void PopUICallback(string button)
    {
        base.PopUICallback(button);

        switch (button)
        {
            case "TEACH":
                Teach();
                break;

            case "STATIONERY":
                DeliverItem(this);
                break;

            case "PRAY":
                UI.Meditate?.Invoke(this);
                break;
        }
    }

    public override void SetDeadlineTime(double time, int day)
    {
        if (BuildingState != BuildingState.NORMAL) return;
        if (time >= 17 || time < 6) return;

    //    if (!DuringOpenHours()) return;
        if ((DeadlineTime.Time != -1)) return;
        if (DeadlineTriggeredForTheDay) return;

        double futureTime = time + RandomFutureTimeByDifficulty();
   //     if (futureTime > ClosingTime) return;

        switch (MissionDifficulty)
        {
            case MissionDifficulty.HARD:
                if (DeadlineCounter < 3)
                {
                    var mission = GetBuildingMission(BuildingEventType.DELIVER_ITEM);
                    if (mission != null || (!SameDayAsMission() && Random.Range(0, 100) < DeadlinePercentChance))
                    {
                        DeadlineCounter++;
                        DeadlineTime.SetClock(mission != null ? mission.DeadlineHours : futureTime, day);
                        if (MissionManager.Instance.CurrentMission.CurrentWeek > 2)
                            RequiredItems = mission != null ? mission.RequiredItems : Random.Range(1, 3); //Depending on Season
                        else
                            RequiredItems = mission != null ? mission.RequiredItems : 1; //Depending on Season
                        DeadlineDeliveryBonus = 1;
                        DeadlineSet = true;
                        DeadlineTriggeredForTheDay = true;
                        PopMyIcon();
                        SoundManager.Instance.PlayOneShotSfx("Notification_SFX");
                        Debug.LogWarning($"{name}: DEADLINE SET FOR {DeadlineTime.Time} : {DeadlineTime.Day}!");
                    }
                }
                break;
        }
    }

    public override void RelationshipReward(ThankYouType thanks)
    {
        var amount = 0;
        if (RelationshipPoints == 100)
        {
            //One time special reward!
        }

        if (RelationshipPoints >= 65)
        {
            amount = Random.Range(9, 10);
        }
        else if (RelationshipPoints >= 30)
        {
            amount = Random.Range(7, 8);
        }
        else if (RelationshipPoints >= 10)
        {
            amount = Random.Range(5, 6);
        }
        else
        {
            amount = Random.Range(4, 5);
        }

        TreasuryManager.Instance.DonateMoney(amount);
        base.RelationshipReward(thanks);
        MoneyThanks();
    }

    public override void VolunteerThanks()
    {
        EventsManager.Instance.AddEventToList(CustomEventType.THANKYOU_TEACH);
    }

    public override TooltipStats GetTooltipStatsForButton(string button)
    {
        switch (button)
        {
            case "VOLUNTEER":
                if (MaxTeachPoints - TeachCountdown == 1)
                    return GameDataManager.Instance.GetToolTip(TooltipStatId.VOLUNTEER, energyModifier: -GameManager.Instance.Player.ModifyEnergyConsumption(amount: EnergyConsumption));
                else
                    return GameDataManager.Instance.GetToolTip(TooltipStatId.TIME);
        }

        return base.GetTooltipStatsForButton(button);
    }

    public override bool CanDoAction(string actionName)
    {
        switch (actionName)
        {
            case "STATIONERY":
                return InventoryManager.Instance.CheckItem(ItemType.STATIONERY);

            case "TEACH":
                Player player = GameManager.Instance.Player;
                return !player.EnergyDepleted() && (DuringOpenHours() || (!DuringOpenHours() && TeachCountdown > 0));
        }

        return base.CanDoAction(actionName);
    }

    protected override void OnEventExecuted(CustomEventData e)
    {
        switch (e.Id)
        {
            case CustomEventType.SCHOOL_CLOSED:
                ClosingTime = 0;
                break;
        }
    }

    public override void ResetActionProgress()
    {
        TeachCountdown = 0;
        base.ResetActionProgress();
    }

    public override bool HasResetActionProgress()
    {
        return TeachCountdown == 0 && base.HasResetActionProgress();
    }


    protected override void AutoDeliver(ItemType item)
    {
        if (item == ItemType.STATIONERY)
        {
            EventsManager.Instance.AddEventToList(CustomEventType.AUTO_DELIVER_COMPLETE);
            UpdateCharityPoints(ItemDeliveryPoints * DeadlineDeliveryBonus, 0);
            base.DeliverItem(this, true);
        }
    }

    public override void TriggerStory()
    {
        if (HasBeenDestroyed) return;

        if (RelationshipPoints >= GameDataManager.MAX_RP_THRESHOLD && !MyStoryEvents.Contains(CustomEventType.SCHOOL_STORY_3))
        {
            EventsManager.Instance.AddEventToList(CustomEventType.SCHOOL_STORY_3);
            MyStoryEvents.Add(CustomEventType.SCHOOL_STORY_3);
        }
        else if (RelationshipPoints >= GameDataManager.MED_RP_THRESHOLD && !MyStoryEvents.Contains(CustomEventType.SCHOOL_STORY_2))
        {
            EventsManager.Instance.AddEventToList(CustomEventType.SCHOOL_STORY_2);
            MyStoryEvents.Add(CustomEventType.SCHOOL_STORY_2);
        }
        else if (RelationshipPoints >= GameDataManager.MIN_RP_THRESHOLD && !MyStoryEvents.Contains(CustomEventType.SCHOOL_STORY_1))
        {
            EventsManager.Instance.AddEventToList(CustomEventType.SCHOOL_STORY_1);
            MyStoryEvents.Add(CustomEventType.SCHOOL_STORY_1);
        }
    }

    public override CustomEventType GetEndGameStory()
    {
        return CustomEventType.ENDGAME_SCHOOL;
    }

    public override HouseSaveData LoadData()
    {
        var data = base.LoadData();
        CustomEventData e = EventsManager.Instance.CurrentEvents.Find(i => i.Id == CustomEventType.SCHOOL_CLOSED);
        if (e != null) ClosingTime = 0;
        return data;
    }

    public override void OnDisable()
    {
        base.OnDisable();
    }
}
