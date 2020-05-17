public class InteractableOrphanage : InteractableHouse
{
    public int AdoptionPoints;

    protected override void Start()
    {
        base.Start();
    }


    public override void OnPlayerMoved(Energy energy, MapTile tile)
    {
        if (tile.GetInstanceID() == GetInstanceID())
        {
            UI.Instance.EnableOrphanage(true, this);
        }
        else
        {
            UI.Instance.EnableOrphanage(false, this);
        }
    }

    public override void DeliverItem(InteractableHouse house)
    {
        if (house != this) return;

        GameClock clock = GameManager.Instance.GameClock;
        if (clock.Time >= OpenTime && clock.Time < ClosingTime)
        {
            Player player = GameManager.Instance.Player;
            PlayerItem item = player.GetItem(ItemType.TOYS);

            if (item != null)
            {
                UI.Instance.DisplayMessage("GAVE TOYS TO THE KIDS!");
                UpdateCharityPoints(ItemDeliveryPoints);
            }
            else
            {
                UI.Instance.DisplayMessage("YOU HAVE NO TOYS TO GIVE!");
            }

        }
        else
        {
            UI.Instance.DisplayMessage("ORPHANAGE CLOSED!");
        }
    }

    public override void Meditated(InteractableHouse house)
    {
        if (house != this) return;
        base.Meditated(house);
    }

    public override void OnDisable()
    {
        base.OnDisable();
    }
}
