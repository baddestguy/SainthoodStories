public class InteractableOrphanage : InteractableHouse
{
    public override void OnPlayerMoved(Energy energy, MapTile tile)
    {
        if (tile.GetInstanceID() == GetInstanceID())
        {
            GameClock clock = GameManager.Instance.GameClock;
            if (clock.Time >= OpenTime && clock.Time < ClosingTime)
            {
                Player player = GameManager.Instance.Player;
                PlayerItem item = player.GetItem(ItemType.CLOTHES);

                if (item != null)
                {
                    UI.Instance.DisplayMessage("GAVE TOYS TO THE KIDS!");
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
    }
}
