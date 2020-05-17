public class InteractableShelter : InteractableHouse
{
    protected override void Start()
    {
        base.Start();
    }

    public override void OnPlayerMoved(Energy energy, MapTile tile)
    {
        if (tile.GetInstanceID() == GetInstanceID())
        {            
            UI.Instance.EnableFood(true, this);
        }
        else
        {
            UI.Instance.EnableFood(false, this);
        }
    }

    public override void DeliverItem(InteractableHouse house)
    {
        if (house != this) return;

        Player player = GameManager.Instance.Player;
        PlayerItem item = player.GetItem(ItemType.GROCERIES);

        if (item != null)
        {
            UI.Instance.DisplayMessage("FED THE HUNGRY!");
            UpdateCharityPoints(ItemDeliveryPoints);
        }
        else
        {
            UI.Instance.DisplayMessage("YOU HAVE NO FOOD TO GIVE!");
        }
    }
    public override void OnDisable()
    {
        base.OnDisable();
    }
}
