public class InteractableMarket : InteractableHouse
{
    protected override void Start()
    {
        UI.BoughtItem += BoughtItem;
        base.Start();
    }

    public override void OnPlayerMoved(Energy energy, MapTile tile)
    {
        GameClock clock = GameManager.Instance.GameClock;
        if (tile.GetInstanceID() == GetInstanceID())
        {
            if (clock.Time >= OpenTime && clock.Time < ClosingTime)
            {
            }
            else
            {
                UI.Instance.DisplayMessage("SHOP CLOSED!");
            }
            UI.Instance.EnableShop(true, this);
        }
        else
        {
            UI.Instance.EnableShop(false, this);
        }
    }

    public void BoughtItem(ItemType item)
    {
        GameClock clock = GameManager.Instance.GameClock;
        if (clock.Time >= OpenTime && clock.Time < ClosingTime)
        {
            UI.Instance.DisplayMessage($"PICKED UP {item}!");
            GameManager.Instance.Player.AddToInventory(new PlayerItem(item));
        }
        else
        {
            UI.Instance.DisplayMessage("SHOP CLOSED!");
        }
    }

    public override void ReportScores()
    {
        //Do absolutely nothing!
    }

    public override void SetDeadlineTime(double time, int day)
    {

    }

    public override void OnDisable()
    {
        UI.BoughtItem -= BoughtItem;
        base.OnDisable();
    }
}
