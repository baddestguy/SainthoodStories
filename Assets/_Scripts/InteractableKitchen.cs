public class InteractableKitchen : InteractableHouse
{
    protected override void Start()
    {
        UI.Cooked += Cook;
        base.Start();
    }

    public override void OnPlayerMoved(Energy energy, MapTile tile)
    {
        if (tile.GetInstanceID() == GetInstanceID())
        {
            UI.Instance.EnableKitchen(true, this);
        }
        else
        {
            UI.Instance.EnableKitchen(false, this);
        }
    }

    public void Cook()
    {

        Player player = GameManager.Instance.Player;
        if (player.EnergyDepleted()) return;

        PlayerItem item = player.GetItem(ItemType.GROCERIES);
        GameClock clock = GameManager.Instance.GameClock;

        if (item != null)
        {
            player.ConsumeEnergy(EnergyConsumption);
            clock.Tick();
            player.AddToInventory(new PlayerItem(ItemType.MEAL));
            UI.Instance.DisplayMessage("COOKED MEAL!");
        }
        else
        {
            UI.Instance.DisplayMessage("YOU HAVE NO FOOD TO COOK!");
        }
    }

    public override void SetDeadlineTime(double time, int day)
    {

    }

    public override void ReportScores()
    {
        //Do absolutely nothing!
    }

    public override void OnDisable()
    {
        UI.Cooked -= Cook;
        base.OnDisable();
    }
}
