using UnityEngine;

public class InteractableHospital : InteractableHouse
{
    public GameClock StartDelivery;
    public GameClock EndDelivery;
    private bool DeliveryTimeSet;
    private bool FailedDelivery;
    public int BabyPoints;
    public int FailedDeliveryPoints;

    protected override void Start()
    {
        UI.DeliverBaby += DeliveredBaby;
        UI.Taught += CheckBabyDelivery;
        UI.Slept += CheckBabyDelivery;
        UI.Prayed += CheckBabyDelivery;
        UI.Meditate += CheckBabyDelivery;
        base.Start();
    }

    public override void OnPlayerMoved(Energy energy, MapTile tile)
    {
        GameClock clock = GameManager.Instance.GameClock;
        if (tile.GetInstanceID() == GetInstanceID())
        {
            UI.Instance.EnableHospital(true, this);
        }
        else
        {
            UI.Instance.EnableHospital(false, this);
            if (!DeliveryTimeSet && !(tile is InteractableHouse) && Random.Range(0, 1.0f) > 0.95f)
            {
                DeliveryTimeSet = true;

                StartDelivery = new GameClock(clock.Time, clock.Day);
                EndDelivery = new GameClock(clock.Time, clock.Day);
                StartDelivery.AddTime(6);
                EndDelivery.AddTime(9);

                UI.Instance.DisplayMessage($"BABY DUE B/W {(int)StartDelivery.Time}:{(StartDelivery.Time % 1 == 0 ? "00" : "30")} AND {(int)EndDelivery.Time}:{(EndDelivery.Time % 1 == 0 ? "00" : "30")}!");
            }
        }
        CheckFailedDelivery();
    }

    private void CheckBabyDelivery(InteractableHouse house)
    {
        CheckBabyDelivery();
    }

    private void CheckBabyDelivery()
    {
        GameClock clock = GameManager.Instance.GameClock;
        if (!DeliveryTimeSet && Random.Range(0, 1.0f) > 0.95f)
        {
            DeliveryTimeSet = true;

            StartDelivery = new GameClock(clock.Time, clock.Day);
            EndDelivery = new GameClock(clock.Time, clock.Day);
            StartDelivery.AddTime(6);
            EndDelivery.AddTime(9);

            UI.Instance.DisplayMessage($"BABY DUE B/W {(int)StartDelivery.Time}:{(StartDelivery.Time % 1 == 0 ? "00" : "30")} AND {(int)EndDelivery.Time}:{(EndDelivery.Time % 1 == 0 ? "00" : "30")}!");
        }
    }

    private void CheckFailedDelivery()
    {
        GameClock clock = GameManager.Instance.GameClock;
        if (DeliveryTimeSet && clock > EndDelivery)
        {
            DeliveryTimeSet = false;
            FailedDelivery = true;
            UI.Instance.DisplayMessage($"FAILED TO DELIVER BABY!");
        }
    }

    public void DeliveredBaby()
    {
        DeliveryTimeSet = false;
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
            UpdateCharityPoints(ItemDeliveryPoints);
        }
        else
        {
            UI.Instance.DisplayMessage("YOU HAVE NO MEDS TO GIVE!");
        }
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
        UI.Taught -= CheckBabyDelivery;
        UI.Slept -= CheckBabyDelivery;
        UI.Prayed -= CheckBabyDelivery;
        base.OnDisable();
    }
}
