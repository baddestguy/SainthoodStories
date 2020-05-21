public class InteractableClothesBank : InteractableHouse
{
    protected override void Start()
    {
        base.Start();
    }

    public override void OnPlayerMoved(Energy energy, MapTile tile)
    {
        if (tile.GetInstanceID() == GetInstanceID())
        {
            UI.Instance.EnableClothes(true, this);
        }
        else
        {
            UI.Instance.EnableClothes(false, this);
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

    public override void OnDisable()
    {
        base.OnDisable();
    }
}
