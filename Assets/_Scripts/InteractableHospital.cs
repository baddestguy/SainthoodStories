public class InteractableHospital : InteractableHouse
{
    public int StartDelivery;
    public int EndDelivery;

    void Start()
    {
        UI.DeliverBaby += DeliveredBaby;
    }

    public override void OnPlayerMoved(Energy energy, MapTile tile)
    {
        if (tile.GetInstanceID() == GetInstanceID())
        {
            GameClock clock = GameManager.Instance.GameClock;
            if (clock.Time >= StartDelivery && clock.Time < EndDelivery)
            {
                UI.Instance.EnableHospital(true);
            }
        }
        else
        {
            UI.Instance.EnableHospital(false);
        }
    }

    public void DeliveredBaby()
    {
        GameClock clock = GameManager.Instance.GameClock;
        Player player = GameManager.Instance.Player;

        if (clock.Time >= OpenTime && clock.Time < ClosingTime)
        {
            player.ConsumeEnergy(EnergyConsumption);
            clock.Tick();
            UI.Instance.DisplayMessage("Delivered a Baby!!");
        }

        if (clock.Time < StartDelivery || clock.Time > EndDelivery)
        {
            UI.Instance.EnableHospital(false);
        }
    }

    private void OnDisable()
    {
        UI.DeliverBaby -= DeliveredBaby;
    }
}
