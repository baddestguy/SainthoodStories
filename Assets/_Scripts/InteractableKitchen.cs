public class InteractableKitchen : InteractableHouse
{
    protected override void Start()
    {
        UI.Cooked += Cook;
        PopUILocation = "UI/KitchenUI";
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

    public void Cook()
    {

        Player player = GameManager.Instance.Player;
        if (player.EnergyDepleted()) return;

        ItemType item = InventoryManager.Instance.GetItem(ItemType.GROCERIES);
        GameClock clock = GameManager.Instance.GameClock;

        if (item != ItemType.NONE)
        {
            player.ConsumeEnergy(EnergyConsumption);
            clock.Tick();
            InventoryManager.Instance.AddToInventory(ItemType.MEAL);
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

    public override void PopUICallback(string button)
    {
        switch (button)
        {
            case "COOK":
                Cook();
                break;

            case "PRAY":
                UI.Meditate?.Invoke(this);
                break;
        }
    }

    public override void OnDisable()
    {
        UI.Cooked -= Cook;
        base.OnDisable();
    }
}
