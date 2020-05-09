public class InteractableMarket : InteractableHouse
{
    protected override void Start()
    {
        UI.BoughtFood += Food;
        UI.BoughtClothes += Clothes;
        UI.BoughtToys += Toys;
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

    public void Food()
    {
        GameClock clock = GameManager.Instance.GameClock;
        if (clock.Time >= OpenTime && clock.Time < ClosingTime)
        {
            UI.Instance.DisplayMessage("PICKED UP FOOD!");
            GameManager.Instance.Player.AddToInventory(new PlayerItem(ItemType.FOOD));
        }
        else
        {
            UI.Instance.DisplayMessage("SHOP CLOSED!");
        }
    }

    public void Clothes()
    {
        GameClock clock = GameManager.Instance.GameClock;
        if (clock.Time >= OpenTime && clock.Time < ClosingTime)
        {
            UI.Instance.DisplayMessage("PICKED UP CLOTHES!");
            GameManager.Instance.Player.AddToInventory(new PlayerItem(ItemType.CLOTHES));
        }
        else
        {
            UI.Instance.DisplayMessage("SHOP CLOSED!");
        }
    }

    public void Toys()
    {
        GameClock clock = GameManager.Instance.GameClock;
        if (clock.Time >= OpenTime && clock.Time < ClosingTime)
        {
            UI.Instance.DisplayMessage("PICKED UP TOYS!");
            GameManager.Instance.Player.AddToInventory(new PlayerItem(ItemType.TOYS));
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

    private void OnDisable()
    {
        UI.BoughtFood -= Food;
        UI.BoughtClothes -= Clothes;
        UI.Meditate -= Meditated;
    }
}
