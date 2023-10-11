using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InteractableHospital : InteractableHouse
{
    public GameClock EndDelivery;
    private bool DeliveryTimeSet;
    private bool FailedDelivery;
    private int DeliveryCountdown = 0;
    public int BabyPoints;
    public int FailedDeliveryPoints;
    private float MaxDeliveryPoints = 4f;

    private string RandomBabyIcon;

    protected override void Start()
    {
        PopUILocation = "UI/ExternalUI";
        base.Start();
    }

    public override void GetInteriorPopUI()
    {
        InteriorPopUI = UI.Instance.transform.Find("HospitalUI").GetComponent<PopUI>();
        base.GetInteriorPopUI();
    }

    public override void OnPlayerMoved(Energy energy, MapTile tile)
    {
        base.OnPlayerMoved(energy, tile);

        if (tile.GetInstanceID() == GetInstanceID())
        {
            if(BuildingState != BuildingState.RUBBLE)
            {
                StartCoroutine(FadeAndSwitchCamerasAsync(InteriorLightsOn));
                MaxDeliveryPoints = CalculateMaxVolunteerPoints();
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

    protected override int ModVolunteerEnergyWithProvisions()
    {
        if (BuildingActivityState != BuildingActivityState.DELIVERING_BABY) return 0;

        var hospitalMaterials = InventoryManager.Instance.GetProvision(Provision.HOSPITAL_RELATIONSHIP_BUILDER);
        return hospitalMaterials?.Value ?? 0;
    }

    public override void Tick(double time, int day)
    {
        if (GameClock.DeltaTime)
        {
            CheckBabyDelivery();
            CheckFailedDelivery();

            if (BuildingActivityState == BuildingActivityState.DELIVERING_BABY)
            {
                GameClock clock = GameManager.Instance.GameClock;
                CustomEventData e = EventsManager.Instance.CurrentEvents.Find(i => i.Id == CustomEventType.HOSPITAL_BONUS);

                if (DeliveryCountdown >= MaxDeliveryPoints || clock == EndDelivery)
                {
                    //Add events with percentage chance to potentially lose the baby if not full participation
                    UI.Instance.DisplayMessage("Baby Delivered Successfuly!!");
                    var moddedEnergy = GameManager.Instance.Player.ModifyEnergyConsumption(amount: EnergyConsumption);
                    moddedEnergy += ModVolunteerEnergyWithProvisions();
                    GameManager.Instance.Player.ConsumeEnergy(EnergyConsumption);
                    var extraPoints = 0;
                    if (PopUI.CriticalHitCount == MaxDeliveryPoints) extraPoints = 1;

                    UpdateCharityPoints(BabyPoints + (e != null ? (int)e.Gain : 0) + extraPoints, moddedEnergy);
                    PopIcon.gameObject.SetActive(false);
                    UI.Instance.SideNotificationPop(GetType().Name);
                    DeadlineCounter--;
                    DeliveryTimeSet = false;
                    DeliveryCountdown = 0;
                    EndDelivery.SetClock(clock.Time - 1, clock.Day);
                    BuildRelationship(ThankYouType.BABY, 2);
                    OnActionProgress?.Invoke(1f, this, 2);
                }
            }
        }

        var mission = GetBuildingMission(BuildingEventType.BABY);
        if (mission != null && !DeliveryTimeSet)
        {
            SetBabyDelivery(mission);
        }
        if (DeliveryTimeSet)
        {
            PopMyIcon(RandomBabyIcon, RequiredItems, EndDelivery);
        }

        base.Tick(time, day);
    }

    public override void TriggerHazardousMode(double time, int day)
    {
        if (HazardCounter > 0) return;
        if (MissionManager.Instance.CurrentMission.CurrentWeek < 2) return;

        DeadlineCounter--;
        DeliveryTimeSet = false;
        DeliveryCountdown = 0;
        EndDelivery?.SetClock(time - 1, day);

        base.TriggerHazardousMode(time, day);
    }

    public override void BuildRelationship(ThankYouType thanks, int amount = 1)
    {
        if(thanks == ThankYouType.BABY || thanks == ThankYouType.VOLUNTEER)
        {
            var hospitalMaterials = InventoryManager.Instance.GetProvision(Provision.HOSPITAL_RELATIONSHIP_BUILDER);
            amount += hospitalMaterials?.Value ?? 0;
        }
        base.BuildRelationship(thanks, amount);
    }

    private void CheckBabyDelivery()
    {
        if (BuildingState != BuildingState.NORMAL) return;

        CustomEventData e = EventsManager.Instance.CurrentEvents.Find(i => i.Id == CustomEventType.BABY_FEVER);
        var mission = GetBuildingMission(BuildingEventType.BABY);
        if (mission != null || (!SameDayAsMission() && DeadlineCounter < 3 && !DeliveryTimeSet && !DeadlineSet && Random.Range(0, 1.0f) > (e != null ? e.Cost : 0.97f)))
        {
            SetBabyDelivery(mission);
        }
    }

    private void SetBabyDelivery(BuildingMissionData bMission)
    {
        if (DeliveryTimeSet || VolunteerCountdown > 0) return;

        GameClock clock = GameManager.Instance.GameClock;
        DeliveryTimeSet = true;
        DeliveryCountdown = 0;
        DeadlineCounter++;

        EndDelivery = new GameClock(clock.Time, clock.Day);
        EndDelivery.AddTime(bMission != null ? bMission.DeadlineHours : 9);
        RandomBabyIcon = "Baby" + Random.Range(1, 3);
        PopMyIcon(RandomBabyIcon, RequiredItems, EndDelivery);
        SoundManager.Instance.PlayOneShotSfx("Notification_SFX");
        //    UI.Instance.DisplayMessage($"BABY DUE B/W {(int)StartDelivery.Time}:{(StartDelivery.Time % 1 == 0 ? "00" : "30")} AND {(int)EndDelivery.Time}:{(EndDelivery.Time % 1 == 0 ? "00" : "30")}!");
    }

    private void CheckFailedDelivery()
    {
        if (BuildingActivityState == BuildingActivityState.DELIVERING_BABY) return;
        GameClock clock = GameManager.Instance.GameClock;
        if (DeliveryTimeSet)
        {
            PopMyIcon(RandomBabyIcon, RequiredItems, EndDelivery);
            if (clock >= EndDelivery)
            {
                DeliveryTimeSet = false;
                DeadlineCounter--;
                FailedDelivery = true;
                PopIcon.gameObject.SetActive(false);
                UI.Instance.SideNotificationPop(GetType().Name);
                UpdateCharityPoints(-FailedDeliveryPoints, 0);
                UI.Instance.DisplayMessage($"FAILED TO DELIVER BABY!");
                SoundManager.Instance.PlayOneShotSfx("FailedDeadline_SFX");
            }
        }
    }

    public override void TriggerStory()
    {
        if (HasBeenDestroyed) return;

        //These values are temporary just for the DEMO!!

        if (RelationshipPoints >= 65 && !MyStoryEvents.Contains(CustomEventType.HOSPITAL_STORY_3))
        {
            EventsManager.Instance.AddEventToList(CustomEventType.HOSPITAL_STORY_3);
            MyStoryEvents.Add(CustomEventType.HOSPITAL_STORY_3);
        }
        else if (RelationshipPoints >= 30 && !MyStoryEvents.Contains(CustomEventType.HOSPITAL_STORY_2))
        {
            EventsManager.Instance.AddEventToList(CustomEventType.HOSPITAL_STORY_2);
            MyStoryEvents.Add(CustomEventType.HOSPITAL_STORY_2);
        }
        else if (RelationshipPoints >= 5 && !MyStoryEvents.Contains(CustomEventType.HOSPITAL_STORY_1))
        {
            EventsManager.Instance.AddEventToList(CustomEventType.HOSPITAL_STORY_1);
            MyStoryEvents.Add(CustomEventType.HOSPITAL_STORY_1);
        }
    }

    public override void TriggerCustomEvent()
    {
        if (DeliveryTimeSet) return;
        base.TriggerCustomEvent();
    }

    public override void PopUICallback(string button)
    {
        base.PopUICallback(button);

        switch (button)
        {
            case "BABY":
                DeliveredBaby();
                break;

            case "MEDS":
                DeliverItem(this);
                break;

            case "VOLUNTEER":
                VolunteerWork(this);
                break;

            case "PRAY":
                UI.Meditate?.Invoke(this);
                break;
        }
    }

    public void DeliveredBaby()
    {
        GameClock clock = GameManager.Instance.GameClock;
        Player player = GameManager.Instance.Player;
        if (player.EnergyDepleted())
        {
            UI.Instance.ErrorFlash("Energy");
            return;
        }
        CustomEventData e = EventsManager.Instance.CurrentEvents.Find(i => i.Id == CustomEventType.HOSPITAL_BONUS);
        if (clock < EndDelivery)
        {
            BuildingActivityState = BuildingActivityState.DELIVERING_BABY;
            UI.Instance.DisplayMessage("Delivering a Baby!!");
            DeliveryCountdown++;
            OnActionProgress?.Invoke(DeliveryCountdown/ MaxDeliveryPoints, this, 2);   
            clock.Tick();
        }
        else if (EndDelivery == null || clock > EndDelivery)
        {
            UI.Instance.DisplayMessage($"NO BABY TO DELIVER!");
        }
    }

    public override void DeliverItem(InteractableHouse house, bool autoDeliver = false)
    {
        if (house != this) return;

        ItemType item = InventoryManager.Instance.GetItem(ItemType.MEDS);

        if (item != ItemType.NONE)
        {
            UI.Instance.DisplayMessage("DELIVERED MEDS!");
            UpdateCharityPoints(ItemDeliveryPoints * DeadlineDeliveryBonus, 0);
            base.DeliverItem(house, autoDeliver);
        }
        else
        {
            UI.Instance.DisplayMessage("YOU HAVE NO MEDS TO GIVE!");
        }
    }

    public override void ItemDeliveryThanks()
    {
        EventsManager.Instance.AddEventToList(CustomEventType.THANKYOU_ITEM_HOSPITAL);
        if (GameSettings.Instance.FTUE)
        {
            EventsManager.Instance.AddEventToList(CustomEventType.Tutorial_61);
        }

        base.ItemDeliveryThanks();
    }

    public override void SetDeadlineTime(double time, int day)
    {
        if (DeliveryTimeSet) return;
        base.SetDeadlineTime(time, day);
    }

    public override void ReportScores()
    {
        if (DeliveryTimeSet) UpdateCharityPoints(-2, 0);
        base.ReportScores();
    }

    public override void VolunteerWork(InteractableHouse house)
    {
        if (house != this) return;

        CustomEventData e = EventsManager.Instance.CurrentEvents.Find(i => i.Id == CustomEventType.HOSPITAL_BONUS);
        GameClock clock = GameManager.Instance.GameClock;
        Player player = GameManager.Instance.Player;
        if (player.EnergyDepleted())
        {
            UI.Instance.ErrorFlash("Energy");
            return;
        }

        BuildingActivityState = BuildingActivityState.VOLUNTEERING;
        var moddedEnergy = player.ModifyEnergyConsumption(amount: EnergyConsumption);
        UI.Instance.DisplayMessage("VOLUNTEERED HOSPITAL!");
        base.VolunteerWork(house);
        clock.Tick();
    }
    public override TooltipStats GetTooltipStatsForButton(string button)
    {
        switch (button)
        {
            case "BABY":
                CustomEventData e = EventsManager.Instance.CurrentEvents.Find(i => i.Id == CustomEventType.HOSPITAL_BONUS);
                if (MaxDeliveryPoints - DeliveryCountdown == 1)
                    return GameDataManager.Instance.GetToolTip(TooltipStatId.BABY, cpModifier: (int?)e?.Gain ?? 0, energyModifier: -GameManager.Instance.Player.ModifyEnergyConsumption(amount: EnergyConsumption));
                else
                    return GameDataManager.Instance.GetToolTip(TooltipStatId.TIME);
        }

        return base.GetTooltipStatsForButton(button);
    }

    public override bool HasResetActionProgress()
    {
        return DeliveryCountdown == 0 && base.HasResetActionProgress();
    }

    public override void VolunteerThanks()
    {
        EventsManager.Instance.AddEventToList(CustomEventType.THANKYOU_VOLUNTEER_HOSPITAL);
    }

    public override void RelationshipReward(ThankYouType thanks)
    {
        if (RelationshipPoints == 100)
        {
            //One time special reward!
        }

        if (RelationshipPoints >= 65)
        {
            //Special Item
        }
        else 
        {
            base.RelationshipReward(thanks);
        }
    }

    public override bool CanDoAction(string actionName)
    {
        Player player = GameManager.Instance.Player;
        switch (actionName)
        {
            case "BABY":
                return !player.EnergyDepleted() && DeliveryTimeSet;

            case "VOLUNTEER":
                return !player.EnergyDepleted() && DuringOpenHours() && !DeliveryTimeSet;

            case "MEDS":
                return InventoryManager.Instance.CheckItem(ItemType.MEDS);
        }

        return base.CanDoAction(actionName);
    }

    public override void ResetActionProgress()
    {
        DeliveryCountdown = 0;
        base.ResetActionProgress();
    }

    protected override void AutoDeliver(ItemType item)
    {
        if(item == ItemType.MEDS)
        {
            EventsManager.Instance.AddEventToList(CustomEventType.AUTO_DELIVER_COMPLETE);
            UpdateCharityPoints(ItemDeliveryPoints * DeadlineDeliveryBonus, 0);
            base.DeliverItem(this, true);
        }
    }

    public override HouseSaveData LoadData()
    {
        var data = base.LoadData();
        if (data == null) return data;

        DeliveryTimeSet = data.DeliveryTimeSet;
        EndDelivery = new GameClock(data.DeliveryTime, data.DeliveryDay);
        RandomBabyIcon = "Baby" + Random.Range(1, 3);

        return data;
    }

    public override HouseSaveData GetHouseSave()
    {
        var save = base.GetHouseSave();
        save.DeliveryTimeSet = DeliveryTimeSet;
        save.DeliveryTime = EndDelivery?.Time ?? 0;
        save.DeliveryDay = EndDelivery?.Day ?? 1;

        return save;
    }

    public override void OnDisable()
    {
        base.OnDisable();
    }
}
