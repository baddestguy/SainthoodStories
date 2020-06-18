public class InteractableClothesBank : InteractableHouse
{
    protected override void Start()
    {
        PopUILocation = "UI/ClothesBankUI";
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

    public override void DeliverItem(InteractableHouse house)
    {
        if (house != this) return;

        ItemType item = InventoryManager.Instance.GetItem(ItemType.CLOTHES);

        if (item != ItemType.NONE)
        {
            UI.Instance.DisplayMessage("CLOTHED THE NAKED!");
            UpdateCharityPoints(ItemDeliveryPoints * DeadlineDeliveryBonus);
            base.DeliverItem(house);
        }
        else
        {
            UI.Instance.DisplayMessage("YOU HAVE NO CLOTHES TO GIVE!");
        }
    }

    public override void PopUICallback(string button)
    {
        base.PopUICallback(button);

        switch (button)
        {
            case "CLOTHES":
                DeliverItem(this);
                break;

            case "PRAY":
                UI.Meditate?.Invoke(this);
                break;
        }
    }
}
