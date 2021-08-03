using UnityEngine;

public class InteractableOrphanage : InteractableHouse
{
    public int AdoptionPoints;

    protected override void Start()
    {
        PopUILocation = "UI/ExternalUI";
        base.Start();
    }

    public override void OnPlayerMoved(Energy energy, MapTile tile)
    {
        base.OnPlayerMoved(energy, tile);
        if (tile.GetInstanceID() == GetInstanceID())
        {
            if (BuildingState != BuildingState.RUBBLE)
            {
                StartCoroutine(FadeAndSwitchCamerasAsync(InteriorLightsOn));
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
    public override void Tick(double time, int day)
    {
        if(day > 5)
        {
            OpenTime = 9;
            ClosingTime = 22;
        }
        else
        {
            OpenTime = 15;
            ClosingTime = 22;
        }

        base.Tick(time, day);
    }

    public override void DeliverItem(InteractableHouse house)
    {
        if (house != this) return;

        ItemType item = InventoryManager.Instance.GetItem(ItemType.TOYS);

        if (item != ItemType.NONE)
        {
            UI.Instance.DisplayMessage("GAVE TOYS TO THE KIDS!");
            UpdateCharityPoints(ItemDeliveryPoints * DeadlineDeliveryBonus, 0);
            base.DeliverItem(house);
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

        if (!DuringOpenHours()) return;
        if ((DeadlineTime.Time != -1)) return;

        double futureTime = time + RandomFutureTimeByDifficulty();
    //    if (futureTime > ClosingTime) return;

        switch (MissionDifficulty)
        {
            case MissionDifficulty.EASY:
                if (DeadlineCounter < 1)
                {
                    if (Random.Range(0, 100) < 1)
                    {
                        DeadlineCounter++;
                        DeadlineTime.SetClock(futureTime, day);
                        DeadlineDeliveryBonus = 4;
                        RequiredItems = 1;
                        DeadlineSet = true;
                        PopMyIcon();
                        Debug.LogWarning($"{name}: DEADLINE SET FOR {DeadlineTime.Time} : {DeadlineTime.Day}!");
                    }
                }
                break;

            case MissionDifficulty.NORMAL:
                if (DeadlineCounter < 2)
                {
                    if (Random.Range(0, 100) < 3)
                    {
                        DeadlineCounter++;
                        DeadlineTime.SetClock(futureTime, day);
                        DeadlineDeliveryBonus = 3;
                        RequiredItems = Random.Range(1, 3);
                        DeadlineSet = true;
                        PopMyIcon();
                        Debug.LogWarning($"{name}: DEADLINE SET FOR {DeadlineTime.Time} : {DeadlineTime.Day}!");
                    }
                }
                break;

            case MissionDifficulty.HARD:
                if (DeadlineCounter < 3)
                {
                    var mission = GetBuildingMission(BuildingEventType.DELIVER_ITEM);
                    if (mission != null || (!SameDayAsMission() && Random.Range(0, 100) < 10))
                    {
                        DeadlineCounter++;
                        DeadlineTime.SetClock(mission != null ? mission.DeadlineHours : futureTime, day);
                        if (MissionManager.Instance.CurrentMission.CurrentWeek > 2)
                            RequiredItems = mission != null ? mission.RequiredItems : Random.Range(1, 3); //Depending on Season
                        else
                            RequiredItems = mission != null ? mission.RequiredItems : 1; //Depending on Season
                        DeadlineDeliveryBonus = 2;
                        DeadlineSet = true;
                        PopMyIcon();
                        SoundManager.Instance.PlayOneShotSfx("Notification");
                        Debug.LogWarning($"{name}: DEADLINE SET FOR {DeadlineTime.Time} : {DeadlineTime.Day}!");
                    }
                }
                break;
        }
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

    public override void VolunteerWork(InteractableHouse house)
    {
        if (house != this) return;

        GameClock clock = GameManager.Instance.GameClock;
        Player player = GameManager.Instance.Player;
        if (player.EnergyDepleted()) return;

        if (DuringOpenHours())
        {
            BuildingActivityState = BuildingActivityState.VOLUNTEERING;
            CustomEventData e = EventsManager.Instance.CurrentEvents.Find(i => i.Id == CustomEventType.ORPHANAGE_BONUS);
            player.ConsumeEnergy(EnergyConsumption);
            UI.Instance.DisplayMessage("VOLUNTEERED AT ORPHANAGE!");
            base.VolunteerWork(house);
            clock.Tick();
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
                return !player.EnergyDepleted() && DuringOpenHours();

            case "TOYS":
                return InventoryManager.Instance.CheckItem(ItemType.TOYS);
        }

        return base.CanDoAction(actionName);
    }
}
