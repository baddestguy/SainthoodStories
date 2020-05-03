public class InteractableClothesBank : InteractableHouse
{
    public override void OnPlayerMoved(Energy energy, MapTile tile)
    {
        if (tile.GetInstanceID() == GetInstanceID())
        {
            Player player = GameManager.Instance.Player;
            PlayerItem item = player.GetItem(ItemType.CLOTHES);

            if (item != null)
            {
                UI.Instance.DisplayMessage("CLOTHED THE NAKED!");
            }
            else
            {
                UI.Instance.DisplayMessage("YOU HAVE NO CLOTHES TO GIVE!");
            }

            UI.Instance.EnableClothes(true, this);
        }
        else
        {
            UI.Instance.EnableClothes(false, this);
        }
    }
}
