using UnityEngine;

public class InteractableHospital : InteractableHouse
{
    public GameClock StartDelivery;
    public GameClock EndDelivery;
    private bool DeliveryTimeSet;
    private bool FailedDelivery;
    public int BabyPoints;

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

                UI.Instance.DisplayMessage($"BABY DUE B/W {StartDelivery.Time}:00 AND {EndDelivery.Time}:00!");
            }
        }
        if (DeliveryTimeSet && clock > EndDelivery)
        {
            DeliveryTimeSet = false;
            FailedDelivery = true;
            UI.Instance.DisplayMessage($"FAILED TO DELIVER BABY!");
        }
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

            UI.Instance.DisplayMessage($"BABY DUE B/W {StartDelivery.Time}:00 AND {EndDelivery.Time}:00!");
        }
    }

    public void DeliveredBaby()
    {
        DeliveryTimeSet = false;
        GameClock clock = GameManager.Instance.GameClock;
        Player player = GameManager.Instance.Player;

        if (clock >= StartDelivery && clock < EndDelivery)
        {
            player.ConsumeEnergy(EnergyConsumption);
            clock.Tick();
            UI.Instance.DisplayMessage("Delivering a Baby!!");
            UpdateTownPoints(BabyPoints);
        }

        if (clock == EndDelivery)
        {
            UI.Instance.DisplayMessage("Baby Delivered Successfuly!!");
            UpdateTownPoints(BabyPoints*2);
        }

        if (StartDelivery == null || EndDelivery == null || clock < StartDelivery || clock > EndDelivery)
        {
            UI.Instance.DisplayMessage($"NO BABY TO DELIVER!");
        }
    }

    public override void ReportScores()
    {
        GameManager.Instance.MissionManager.UpdateTownPoints(CurrentTownPoints > 0 ? CurrentTownPoints : FailedDelivery ? (NeglectedPoints * NeglectedMultiplier) : 0, this);
        FailedDelivery = false;

        if (CurrentTownPoints <= 0 && FailedDelivery)
        {
            NeglectedMultiplier++;
        }
        else
        {
            NeglectedMultiplier = 1;
        }

        CurrentTownPoints = 0;
    }

    private void OnDisable()
    {
        UI.DeliverBaby -= DeliveredBaby;
        UI.Taught -= CheckBabyDelivery;
        UI.Slept -= CheckBabyDelivery;
        UI.Prayed -= CheckBabyDelivery;
        UI.Meditate -= Meditated;
    }
}
