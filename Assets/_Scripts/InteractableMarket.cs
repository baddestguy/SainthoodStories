public class InteractableMarket : InteractableHouse
{
    void Start()
    {
        UI.BoughtFood += Food;
        UI.BoughtClothes += Clothes;
    }

    public override void OnPlayerMoved(Energy energy, MapTile tile)
    {
        GameClock clock = GameManager.Instance.GameClock;
        if (tile.GetInstanceID() == GetInstanceID())
        {
            if (clock.Time >= OpenTime && clock.Time <= ClosingTime)
            {
                UI.Instance.EnableShop(true);
            }
            else
            {
                UI.Instance.DisplayMessage("SHOP CLOSED!");
            }
        }
        else
        {
            UI.Instance.EnableShop(false);
        }
    }

    public void Food()
    {
        UI.Instance.DisplayMessage("PICKED UP FOOD!");
        GameManager.Instance.Player.AddToInventory(new PlayerItem(ItemType.FOOD));
    }

    public void Clothes()
    {
        UI.Instance.DisplayMessage("PICKED UP CLOTHES!");
        GameManager.Instance.Player.AddToInventory(new PlayerItem(ItemType.CLOTHES));
    }

    private void OnDisable()
    {
        UI.BoughtFood -= Food;
        UI.BoughtClothes -= Clothes;
    }
}
