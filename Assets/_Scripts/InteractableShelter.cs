public class InteractableShelter : InteractableHouse
{
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
        PlayerItem groceries = player.GetItem(ItemType.GROCERIES);

        if (groceries != null)
        {
            UI.Instance.DisplayMessage("DELIVERED GROCERIES!");
            UpdateCharityPoints(ItemDeliveryPoints * DeadlineDeliveryBonus);
            base.DeliverItem(house);
            return;
        }

        PlayerItem meal = player.GetItem(ItemType.MEAL);
        if (meal != null)
        {
            UI.Instance.DisplayMessage("FED THE HUNGRY!");
            UpdateCharityPoints(ItemDeliveryPoints*2 * DeadlineDeliveryBonus);
            base.DeliverItem(house);
            return;
        }

        UI.Instance.DisplayMessage("YOU HAVE NO FOOD TO GIVE!");
    }
}
