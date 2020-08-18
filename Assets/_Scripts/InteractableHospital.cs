using UnityEngine;

public class InteractableHospital : InteractableHouse
{
    public GameClock StartDelivery;
    public GameClock EndDelivery;
    private bool DeliveryTimeSet;
    private bool FailedDelivery;
    private int DeliveryCountdown = 4;
    public int BabyPoints;
    public int FailedDeliveryPoints;

    private string RandomBabyIcon;

    protected override void Start()
    {
        UI.DeliverBaby += DeliveredBaby;
        PopUILocation = "UI/HospitalUI";
        base.Start();
    }

    public override void OnPlayerMoved(Energy energy, MapTile tile)
    {
        base.OnPlayerMoved(energy, tile);

        if (tile.GetInstanceID() == GetInstanceID())
        {
            PopUI.gameObject.SetActive(true);
            PopUI.Init(PopUICallback, GetType().Name, RequiredItems, DeadlineTime);
            PopIcon.UIPopped(true);
        }
        else
        {
            PopUI.gameObject.SetActive(false);
            PopIcon.UIPopped(false);
        }
    }

    public override void Tick(double time, int day)
    {
        CheckBabyDelivery();
        CheckFailedDelivery();

        if (BuildingActivityState == BuildingActivityState.DELIVERING_BABY)
        {
            GameClock clock = GameManager.Instance.GameClock;
            CustomEventData e = EventsManager.Instance.CurrentEvents.Find(i => i.Id == EventType.HOSPITAL_BONUS);

            if (DeliveryCountdown <= 0 || clock == EndDelivery)
            {
                UI.Instance.DisplayMessage("Baby Delivered Successfuly!!");
                UpdateCharityPoints((BabyPoints + (e != null ? (int)e.Gain : 0)) * 2);
                PopIcon.gameObject.SetActive(false);
                UI.Instance.SideNotificationPop(GetType().Name);
                DeliveryTimeSet = false;
                DeliveryCountdown = 4;
                EndDelivery.SetClock(clock.Time - 1, clock.Day);
                BuildRelationship(ThankYouType.BABY);
            }
        }

        base.Tick(time, day);
    }

    private void CheckBabyDelivery()
    {
        if (BuildingState != BuildingState.NORMAL) return;

        CustomEventData e = EventsManager.Instance.CurrentEvents.Find(i => i.Id == EventType.BABY_FEVER);
        if ((!DeliveryTimeSet && !DeadlineSet) && Random.Range(0, 1.0f) > (e != null ? e.Cost : 0.95f))
        {
            SetBabyDelivery();
        }
    }

    private void SetBabyDelivery()
    {
        GameClock clock = GameManager.Instance.GameClock;
        DeliveryTimeSet = true;
        DeliveryCountdown = 4;

        StartDelivery = new GameClock(clock.Time, clock.Day);
        EndDelivery = new GameClock(clock.Time, clock.Day);
        StartDelivery.AddTime(6);
        EndDelivery.AddTime(9);
        RandomBabyIcon = "Baby" + Random.Range(1, 3);
        PopMyIcon(RandomBabyIcon, RequiredItems, EndDelivery);
    //    UI.Instance.DisplayMessage($"BABY DUE B/W {(int)StartDelivery.Time}:{(StartDelivery.Time % 1 == 0 ? "00" : "30")} AND {(int)EndDelivery.Time}:{(EndDelivery.Time % 1 == 0 ? "00" : "30")}!");
    }

    private void CheckFailedDelivery()
    {
        GameClock clock = GameManager.Instance.GameClock;
        if (DeliveryTimeSet)
        {
            PopMyIcon(RandomBabyIcon, RequiredItems, EndDelivery);
            if (clock > EndDelivery)
            {
                DeliveryTimeSet = false;
                FailedDelivery = true;
                PopIcon.gameObject.SetActive(false);
                UI.Instance.SideNotificationPop(GetType().Name);
                UI.Instance.DisplayMessage($"FAILED TO DELIVER BABY!");
            }
        }
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
        if (player.EnergyDepleted()) return;

        CustomEventData e = EventsManager.Instance.CurrentEvents.Find(i => i.Id == EventType.HOSPITAL_BONUS);
        if (clock < EndDelivery)
        {
            BuildingActivityState = BuildingActivityState.DELIVERING_BABY;
            player.ConsumeEnergy(EnergyConsumption);
            UI.Instance.DisplayMessage("Delivering a Baby!!");
            UpdateCharityPoints(BabyPoints + (e != null ? (int)e.Gain : 0));
            DeliveryCountdown--;
            clock.Tick();
        }
        else if (EndDelivery == null || clock > EndDelivery)
        {
            UI.Instance.DisplayMessage($"NO BABY TO DELIVER!");
        }
    }

    public override void DeliverItem(InteractableHouse house)
    {
        if (house != this) return;

        ItemType item = InventoryManager.Instance.GetItem(ItemType.MEDS);

        if (item != ItemType.NONE)
        {
            UI.Instance.DisplayMessage("DELIVERED MEDS!");
            UpdateCharityPoints(ItemDeliveryPoints * DeadlineDeliveryBonus);
            base.DeliverItem(house);
        }
        else
        {
            UI.Instance.DisplayMessage("YOU HAVE NO MEDS TO GIVE!");
        }
    }

    public override void ItemDeliveryThanks()
    {
        EventsManager.Instance.AddEventToList(EventType.THANKYOU_ITEM_HOSPITAL);
        base.ItemDeliveryThanks();
    }

    public override void SetDeadlineTime(double time, int day)
    {
        if (DeliveryTimeSet) return;
        base.SetDeadlineTime(time, day);
    }

    public override void ReportScores()
    {
        CheckFailedDelivery();
        GameManager.Instance.MissionManager.UpdateCharityPoints(CurrentCharityPoints > 0 ? CurrentCharityPoints : FailedDelivery ? (FailedDeliveryPoints * NeglectedMultiplier) : (NeglectedPoints * NeglectedMultiplier), this);
        FailedDelivery = false;

        if (CurrentCharityPoints <= 0 && FailedDelivery)
        {
            NeglectedMultiplier++;
        }
        else
        {
            NeglectedMultiplier = 1;
        }

        CurrentCharityPoints = 0;
    }

    public override void VolunteerWork(InteractableHouse house)
    {
        if (house != this) return;

        CustomEventData e = EventsManager.Instance.CurrentEvents.Find(i => i.Id == EventType.HOSPITAL_BONUS);
        GameClock clock = GameManager.Instance.GameClock;
        Player player = GameManager.Instance.Player;
        if (player.EnergyDepleted()) return;

        BuildingActivityState = BuildingActivityState.VOLUNTEERING;
        player.ConsumeEnergy(EnergyConsumption);
        UI.Instance.DisplayMessage("VOLUNTEERED HOSPITAL!");
        UpdateCharityPoints(VolunteerPoints + (e != null ? (int)e.Gain : 0));
        base.VolunteerWork(house);
        clock.Tick();
    }

    public override void VolunteerThanks()
    {
        EventsManager.Instance.AddEventToList(EventType.THANKYOU_VOLUNTEER_HOSPITAL);
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

    public override void OnDisable()
    {
        UI.DeliverBaby -= DeliveredBaby;
        base.OnDisable();
    }
}
