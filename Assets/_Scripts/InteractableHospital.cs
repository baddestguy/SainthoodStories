using UnityEngine;

public class InteractableHospital : InteractableHouse
{
    public GameClock StartDelivery;
    public GameClock EndDelivery;
    private bool DeliveryTimeSet;

    void Start()
    {
        UI.DeliverBaby += DeliveredBaby;
        UI.Taught += CheckBabyDelivery;
        UI.Slept += CheckBabyDelivery;
        UI.Prayed += CheckBabyDelivery;
    }

    public override void OnPlayerMoved(Energy energy, MapTile tile)
    {
        GameClock clock = GameManager.Instance.GameClock;
        if (tile.GetInstanceID() == GetInstanceID())
        {
            if (clock >= StartDelivery && clock < EndDelivery)
            {
                UI.Instance.EnableHospital(true);
            }
        }
        else
        {
            UI.Instance.EnableHospital(false);
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
            UI.Instance.DisplayMessage($"FAILED TO DELIVER BABY!");
        }
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
            UI.Instance.DisplayMessage("Delivered a Baby!!");
        }

        if (clock < StartDelivery || clock >= EndDelivery)
        {
            UI.Instance.DisplayMessage("Baby Delivered Successfuly!!");
            UI.Instance.EnableHospital(false);
        }
    }

    private void OnDisable()
    {
        UI.DeliverBaby -= DeliveredBaby;
        UI.Taught -= CheckBabyDelivery;
        UI.Slept -= CheckBabyDelivery;
        UI.Prayed -= CheckBabyDelivery;
    }
}
