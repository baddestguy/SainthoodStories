using UnityEngine;

public class InteractableHospital : InteractableHouse
{
    public GameClock StartDelivery;
    public GameClock EndDelivery;
    private bool DeliveryTimeSet;
    private bool FailedDelivery;
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
        base.Tick(time, day);
    }

    private void CheckBabyDelivery()
    {
        if ((!DeliveryTimeSet && !DeadlineSet) && Random.Range(0, 1.0f) > 0.95f)
        {
            SetBabyDelivery();
        }
    }

    private void SetBabyDelivery()
    {
        GameClock clock = GameManager.Instance.GameClock;
        DeliveryTimeSet = true;

        StartDelivery = new GameClock(clock.Time, clock.Day);
        EndDelivery = new GameClock(clock.Time, clock.Day);
        StartDelivery.AddTime(6);
        EndDelivery.AddTime(9);
        RandomBabyIcon = "Baby" + Random.Range(1, 3);
        PopMyIcon(RandomBabyIcon, RequiredItems, StartDelivery);
        UI.Instance.DisplayMessage($"BABY DUE B/W {(int)StartDelivery.Time}:{(StartDelivery.Time % 1 == 0 ? "00" : "30")} AND {(int)EndDelivery.Time}:{(EndDelivery.Time % 1 == 0 ? "00" : "30")}!");
    }

    private void CheckFailedDelivery()
    {
        GameClock clock = GameManager.Instance.GameClock;
        if (DeliveryTimeSet)
        {
            PopMyIcon(RandomBabyIcon, RequiredItems, StartDelivery);
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

        if (clock >= StartDelivery && clock < EndDelivery)
        {
            player.ConsumeEnergy(EnergyConsumption);
            clock.Tick();
            UI.Instance.DisplayMessage("Delivering a Baby!!");
            UpdateCharityPoints(BabyPoints);
        }

        if (clock == EndDelivery)
        {
            UI.Instance.DisplayMessage("Baby Delivered Successfuly!!");
            UpdateCharityPoints(BabyPoints*2);
            PopIcon.gameObject.SetActive(false);
            UI.Instance.SideNotificationPop(GetType().Name);
            DeliveryTimeSet = false;
        }

        if (StartDelivery == null || EndDelivery == null || clock < StartDelivery || clock > EndDelivery)
        {
            UI.Instance.DisplayMessage($"NO BABY TO DELIVER!");
        }
    }

    public override void DeliverItem(InteractableHouse house)
    {
        if (house != this) return;

        Player player = GameManager.Instance.Player;
        PlayerItem item = player.GetItem(ItemType.MEDS);

        if (item != null)
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

        GameClock clock = GameManager.Instance.GameClock;
        Player player = GameManager.Instance.Player;
        if (player.EnergyDepleted()) return;

        player.ConsumeEnergy(EnergyConsumption);
        clock.Tick();
        UI.Instance.DisplayMessage("VOLUNTEERED HOSPITAL!");
        UpdateCharityPoints(VolunteerPoints);
    }

    public override void OnDisable()
    {
        UI.DeliverBaby -= DeliveredBaby;
        base.OnDisable();
    }
}
