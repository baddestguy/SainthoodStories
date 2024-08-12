using UnityEngine;

public class InteractableOrphanage : InteractableHouse
{
    public int AdoptionPoints;

    protected override void Start()
    {
        PopUILocation = "UI/ExternalUI";
        base.Start();
    }

    public override void GetInteriorPopUI()
    {
        InteriorPopUI = UI.Instance.transform.Find("OrphanageUI").GetComponent<PopUI>();
        base.GetInteriorPopUI();
    }

    public override void OnPlayerMoved(Energy energy, MapTile tile)
    {
        base.OnPlayerMoved(energy, tile);
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

    public override float CalculateMaxVolunteerPoints(int amount = 4)
    {
        return base.CalculateMaxVolunteerPoints(amount);
    }

    public override void SetObjectiveParameters()
    {
        if (MyObjective == null) return;

        base.SetObjectiveParameters();
    }

    public override void Tick(double time, int day)
    {
        if(day > 5)
        {
            OpenTime = 9;
            ClosingTime = 23.5;
        }
        else
        {
            OpenTime = 16;
            ClosingTime = 23.5;
        }

        base.Tick(time, day);
    }

    public override void DeliverItem(InteractableHouse house, bool autoDeliver = false)
    {
        if (house != this) return;

        ItemType item = InventoryManager.Instance.GetItem(ItemType.TOYS);

        if (item != ItemType.NONE)
        {
            UI.Instance.DisplayMessage("GAVE TOYS TO THE KIDS!");
            base.DeliverItem(house, autoDeliver);
        }
        else
        {
            UI.Instance.DisplayMessage("YOU HAVE NO TOYS TO GIVE!");
        }
    }

    public override void ItemDeliveryThanks()
    {
        EventsManager.Instance.AddEventToList(CustomEventType.THANKYOU_ITEM_ORPHANAGE);
        base.ItemDeliveryThanks();
    }

    public override void SetDeadlineTime(double time, int day)
    {
        if (BuildingState != BuildingState.NORMAL) return;
        if (time >= 17 || time < 6) return;

        //   if (!DuringOpenHours()) return;
        if ((DeadlineTime.Time != -1)) return;

        double futureTime = time + RandomFutureTimeByDifficulty();
    //    if (futureTime > ClosingTime) return;

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
                        SoundManager.Instance.PlayOneShotSfx("Notification_SFX");
                        Debug.LogWarning($"{name}: DEADLINE SET FOR {DeadlineTime.Time} : {DeadlineTime.Day}!");
                    }
                }
                break;
        }
    }

    public override void BuildRelationship(ThankYouType thanks, int amount = 1)
    {
        if(thanks == ThankYouType.VOLUNTEER)
        {
            var orphanageMaterials = InventoryManager.Instance.GetProvision(Provision.ORPHANAGE_RELATIONSHIP_BUILDER);
            amount += 2 + (orphanageMaterials?.Value ?? 0);
            TreasuryManager.Instance.DonateMoney(orphanageMaterials?.Value ?? 0);
        }
        base.BuildRelationship(thanks, amount);
    }
    protected override int ModVolunteerEnergyWithProvisions()
    {
        var orphanageMaterials = InventoryManager.Instance.GetProvision(Provision.ORPHANAGE_RELATIONSHIP_BUILDER);
        return orphanageMaterials?.Value ?? 0;
    }

    public override void RelationshipReward(ThankYouType thanks)
    {
        if (RelationshipPoints == 100)
        {
            //One time special reward!
        }

        if (RelationshipPoints >= 65)
        {
        }
        else if (RelationshipPoints >= 30)
        {
        }
        else if (RelationshipPoints >= 10)
        {
        }
        else
        {

        }
        base.RelationshipReward(thanks);
    }

    public override void VolunteerWork(InteractableHouse house)
    {
        if (house != this) return;

        GameClock clock = GameManager.Instance.GameClock;
        Player player = GameManager.Instance.Player;
        if (player.EnergyDepleted())
        {
            UI.Instance.ErrorFlash("Energy");
            return;
        }
        if (DuringOpenHours() || (!DuringOpenHours() && VolunteerCountdown > 0))
        {
            BuildingActivityState = BuildingActivityState.VOLUNTEERING;
            CustomEventData e = EventsManager.Instance.CurrentEvents.Find(i => i.Id == CustomEventType.ORPHANAGE_BONUS);
            UI.Instance.DisplayMessage("VOLUNTEERED AT ORPHANAGE!");
            base.VolunteerWork(house);
            for (int i = 0; i < MaxVolunteerPoints; i++)
            {
                BuildingActivityState = BuildingActivityState.VOLUNTEERING;
                clock.Tick();
            }
        }
        else
        {
            UI.Instance.DisplayMessage("ORPHANAGE CLOSED!");
        }
    }

    public override void VolunteerThanks()
    {
        EventsManager.Instance.AddEventToList(CustomEventType.THANKYOU_VOLUNTEER_ORPHANAGE);
    }

    public override void PopUICallback(string button)
    {
        base.PopUICallback(button);

        switch (button)
        {
            case "TOYS":
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

    public override bool CanDoAction(string actionName)
    {
        Player player = GameManager.Instance.Player;
        switch (actionName)
        {
            case "VOLUNTEER":
                return !player.EnergyDepleted() && (AllObjectivesComplete || (MyObjective != null && (MyObjective.Event == BuildingEventType.VOLUNTEER || MyObjective.Event == BuildingEventType.VOLUNTEER_URGENT)));

            case "TOYS":
                return InventoryManager.Instance.CheckItem(ItemType.TOYS);
        }

        return base.CanDoAction(actionName);
    }

    public override void TriggerUpgradeStory()
    {
        if (HasBeenDestroyed) return;

        if (UpgradeLevel == 3 && !MyStoryEvents.Contains(CustomEventType.ORPHANAGE_STORY_3))
        {
            EventsManager.Instance.AddEventToList(CustomEventType.ORPHANAGE_STORY_3);
            MyStoryEvents.Add(CustomEventType.ORPHANAGE_STORY_3);
        }
        else if (UpgradeLevel == 2 && !MyStoryEvents.Contains(CustomEventType.ORPHANAGE_STORY_2))
        {
            EventsManager.Instance.AddEventToList(CustomEventType.ORPHANAGE_STORY_2);
            MyStoryEvents.Add(CustomEventType.ORPHANAGE_STORY_2);
        }
        else if (UpgradeLevel == 1 && !MyStoryEvents.Contains(CustomEventType.ORPHANAGE_STORY_1))
        {
            EventsManager.Instance.AddEventToList(CustomEventType.ORPHANAGE_STORY_1);
            MyStoryEvents.Add(CustomEventType.ORPHANAGE_STORY_1);
        }
    }

    protected override void AutoDeliver(ItemType item)
    {
        if (item == ItemType.TOYS)
        {
            EventsManager.Instance.AddEventToList(CustomEventType.AUTO_DELIVER_COMPLETE);
            base.DeliverItem(this, true);
        }
    }

    public override CustomEventType GetEndGameStory()
    {
        return CustomEventType.ENDGAME_ORPHANAGE;
    }
}
