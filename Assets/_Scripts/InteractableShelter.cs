public class InteractableShelter : InteractableHouse
{
    public int FeedPoints;

    protected override void Start()
    {
        UI.DonatedFood += GiveFood;
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

    public void GiveFood()
    {
        Player player = GameManager.Instance.Player;
        PlayerItem item = player.GetItem(ItemType.FOOD);

        if (item != null)
        {
            UI.Instance.DisplayMessage("FED THE HUNGRY!");
            UpdateCharityPoints(FeedPoints);
        }
        else
        {
            UI.Instance.DisplayMessage("YOU HAVE NO FOOD TO GIVE!");
        }
    }
    public override void OnDisable()
    {
        UI.DonatedFood -= GiveFood;
        base.OnDisable();
    }
}
