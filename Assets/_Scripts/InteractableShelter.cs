public class InteractableShelter : InteractableHouse
{
    protected override void Start()
    {
        PopUILocation = "UI/FoodShelterUI";
        base.Start();
    }

    public override void OnPlayerMoved(Energy energy, MapTile tile)
    {
        if (tile.GetInstanceID() == GetInstanceID())
        {
            PopUI.gameObject.SetActive(true);
            PopIcon.UIPopped(true);
        }
        else
        {
            PopUI.gameObject.SetActive(false);
            PopIcon.UIPopped(false);
        }
    }

    public override void DeliverItem(InteractableHouse house)
    {
        if (house != this) return;

        if(DeadlineSet)
        {
            DeliverDeadlineItem();
            return;
        }

        Player player = GameManager.Instance.Player;
        PlayerItem groceries = player.GetItem(ItemType.GROCERIES);

        if (groceries != null)
        {
            UI.Instance.DisplayMessage("DELIVERED GROCERIES!");
            UpdateCharityPoints(ItemDeliveryPoints);
            base.DeliverItem(house);
            return;
        }

        PlayerItem meal = player.GetItem(ItemType.MEAL);
        if (meal != null)
        {
            UI.Instance.DisplayMessage("FED THE HUNGRY!");
            UpdateCharityPoints(ItemDeliveryPoints*2);
            base.DeliverItem(house);
            return;
        }

        UI.Instance.DisplayMessage("YOU HAVE NO FOOD TO GIVE!");
    }

    private void DeliverDeadlineItem()
    {
        Player player = GameManager.Instance.Player;
        PlayerItem meal = player.GetItem(ItemType.MEAL);
        if (meal != null)
        {
            UI.Instance.DisplayMessage("FED THE HUNGRY!");
            UpdateCharityPoints(ItemDeliveryPoints * 2 * DeadlineDeliveryBonus);
            base.DeliverItem(this);
        }
    }

    public override void PopUICallback(string button)
    {
        switch (button)
        {
            case "FOOD":
                DeliverItem(this);
                break;

            case "PRAY":
                UI.Meditate?.Invoke(this);
                break;
        }
    }

}
