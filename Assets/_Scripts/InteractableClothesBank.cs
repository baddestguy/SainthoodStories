public class InteractableClothesBank : InteractableHouse
{
    protected override void Start()
    {
        PopUILocation = "UI/ExternalUI";
        base.Start();
    }

    public override void OnPlayerMoved(Energy energy, MapTile tile)
    {
        base.OnPlayerMoved(energy, tile);
        if (tile.GetInstanceID() == GetInstanceID())
        {
            if (BuildingState != BuildingState.RUBBLE)
            {
                StartCoroutine(FadeAndSwitchCamerasAsync(InteriorLightsOn));
            }
            else
            {
                ExteriorPopUI.gameObject.SetActive(true);
                ExteriorPopUI.Init(PopUICallback, GetType().Name, RequiredItems, DeadlineTime, this, InteriorCam == null ? null : InteriorCam?.GetComponent<CameraControls>());
                PopIcon.UIPopped(true);
            }
        }
        else
        {
            ExteriorPopUI.gameObject.SetActive(false);
            PopIcon.UIPopped(false);
        }
    }

    public override void DeliverItem(InteractableHouse house, bool autoDeliver = false)
    {
        if (house != this) return;

        ItemType item = InventoryManager.Instance.GetItem(ItemType.CLOTHES);

        if (item != ItemType.NONE)
        {
            UI.Instance.DisplayMessage("CLOTHED THE NAKED!");
            UpdateCharityPoints(ItemDeliveryPoints * DeadlineDeliveryBonus, 0);
            base.DeliverItem(house, autoDeliver);
        }
        else
        {
            UI.Instance.DisplayMessage("YOU HAVE NO CLOTHES TO GIVE!");
        }
    }

    public override void RelationshipReward(ThankYouType thanks)
    {
        if (RelationshipPoints == 100)
        {
            //One time special reward!
        }

        if (RelationshipPoints >= 65)
        {
            //Special Item
        }
        else
        {
            base.RelationshipReward(thanks);
        }
    }

    public override void ItemDeliveryThanks()
    {
        EventsManager.Instance.AddEventToList(CustomEventType.THANKYOU_ITEM_CLOTHES);
        base.ItemDeliveryThanks();
    }

    public override void UpgradeThanks()
    {
        EventsManager.Instance.AddEventToList(CustomEventType.THANKYOU_UPGRADE_CLOTHES);
    }

    public override void PopUICallback(string button)
    {
        base.PopUICallback(button);

        switch (button)
        {
            case "CLOTHES":
                DeliverItem(this);
                break;

            case "PRAY":
                UI.Meditate?.Invoke(this);
                break;
        }
    }

    public override bool CanDoAction(string actionName)
    {
        switch (actionName)
        {
            case "CLOTHES":
                return InventoryManager.Instance.CheckItem(ItemType.CLOTHES);
        }

        return base.CanDoAction(actionName);
    }
}
