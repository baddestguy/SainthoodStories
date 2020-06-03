public class InteractableClothesBank : InteractableHouse
{
    protected override void Start()
    {
        PopUILocation = "UI/ClothesBankUI";
        base.Start();
    }

    public override void OnPlayerMoved(Energy energy, MapTile tile)
    {
        if (tile.GetInstanceID() == GetInstanceID())
        {
            PopUI.gameObject.SetActive(true);
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

        Player player = GameManager.Instance.Player;
        PlayerItem item = player.GetItem(ItemType.CLOTHES);

        if (item != null)
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
