public class InteractableOrphanage : InteractableHouse
{
    public int ToyPoints;
    public int AdoptionPoints;

    protected override void Start()
    {
        UI.DonatedToys += GiveToys;
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

    public void GiveToys()
    {
        GameClock clock = GameManager.Instance.GameClock;
        if (clock.Time >= OpenTime && clock.Time < ClosingTime)
        {
            Player player = GameManager.Instance.Player;
            PlayerItem item = player.GetItem(ItemType.TOYS);

            if (item != null)
            {
                UI.Instance.DisplayMessage("GAVE TOYS TO THE KIDS!");
                UpdateTownPoints(ToyPoints);
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

    private void OnDisable()
    {
        UI.DonatedToys -= GiveToys;
        UI.Meditate -= Meditated;
    }
}
