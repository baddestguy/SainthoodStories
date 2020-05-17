public class InteractableOrphanage : InteractableHouse
{
    public int AdoptionPoints;

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

        if (DuringOpenHours())
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

    public override void VolunteerWork(InteractableHouse house)
    {
        if (house != this) return;

        GameClock clock = GameManager.Instance.GameClock;
        Player player = GameManager.Instance.Player;
        if (player.EnergyDepleted()) return;

        if (DuringOpenHours())
        {
            player.ConsumeEnergy(EnergyConsumption);
            clock.Tick();
            UI.Instance.DisplayMessage("VOLUNTEERED AT ORPHANAGE!");
            UpdateCharityPoints(VolunteerPoints);
        }
        else
        {
            UI.Instance.DisplayMessage("ORPHANAGE CLOSED!");
        }
    }
}
