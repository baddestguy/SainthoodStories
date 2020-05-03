public class InteractableShelter : InteractableHouse
{
    public override void OnPlayerMoved(Energy energy, MapTile tile)
    {
        if (tile.GetInstanceID() == GetInstanceID())
        {
            Player player = GameManager.Instance.Player;
            PlayerItem item = player.GetItem(ItemType.FOOD);

            if(item != null)
            {
                UI.Instance.DisplayMessage("FED THE HUNGRY!");
            }
            else
            {
                UI.Instance.DisplayMessage("YOU HAVE NO FOOD TO GIVE!");
            }
            
            UI.Instance.EnableFood(true, this);
        }
        else
        {
            UI.Instance.EnableFood(false, this);
        }
    }
}
