public class InteractableShelter : InteractableHouse
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

    public override void DeliverItem(InteractableHouse house)
    {
        if (house != this) return;

        if(DeadlineSet)
        {
            DeliverDeadlineItem();
            return;
        }

        ItemType groceries = InventoryManager.Instance.GetItem(ItemType.GROCERIES);

        if (groceries != ItemType.NONE)
        {
            UI.Instance.DisplayMessage("DELIVERED GROCERIES!");
            UpdateCharityPoints(ItemDeliveryPoints, 0);
            base.DeliverItem(house);
            return;
        }

        ItemType meal = InventoryManager.Instance.GetItem(ItemType.MEAL);
        if (meal != ItemType.NONE)
        {
            UI.Instance.DisplayMessage("FED THE HUNGRY!");
            UpdateCharityPoints(ItemDeliveryPoints*2, 0);
            base.DeliverItem(house);
            return;
        }

        UI.Instance.DisplayMessage("YOU HAVE NO FOOD TO GIVE!");
    }

    public override void ItemDeliveryThanks()
    {
        EventsManager.Instance.AddEventToList(CustomEventType.THANKYOU_ITEM_FOOD);
        base.ItemDeliveryThanks();
    }

    private void DeliverDeadlineItem()
    {
        ItemType meal = InventoryManager.Instance.GetItem(ItemType.MEAL);
        if (meal != ItemType.NONE)
        {
            UI.Instance.DisplayMessage("FED THE HUNGRY!");
            UpdateCharityPoints(ItemDeliveryPoints * 2 * DeadlineDeliveryBonus, 0);
            base.DeliverItem(this);
        }
        else
        {
            ItemType grocery = InventoryManager.Instance.GetItem(ItemType.GROCERIES);
            if (grocery != ItemType.NONE)
            {
                UI.Instance.DisplayMessage("FED THE HUNGRY!");
                UpdateCharityPoints(ItemDeliveryPoints * 1 * DeadlineDeliveryBonus, 0);
                base.DeliverItem(this);
            }
        }
    }

    public override void BuildRelationship(ThankYouType thanks, int amount = 1)
    {
        if(thanks == ThankYouType.ITEM)
        {
            var shelterMaterials = InventoryManager.Instance.GetProvision(Provision.SHELTER_RELATIONSHIP_BUILDER);
            amount += shelterMaterials?.Value ?? 0;
        }
        base.BuildRelationship(thanks, amount);
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

    public override void PopUICallback(string button)
    {
        base.PopUICallback(button);

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

    public override bool CanDoAction(string actionName)
    {
        switch (actionName)
        {
            case "FOOD":
                return (InventoryManager.Instance.CheckItem(ItemType.GROCERIES) || InventoryManager.Instance.CheckItem(ItemType.MEAL));
        }

        return base.CanDoAction(actionName);
    }

    protected override void AutoDeliver(ItemType item)
    {
        if (item == ItemType.GROCERIES)
        {
            UpdateCharityPoints(ItemDeliveryPoints * DeadlineDeliveryBonus, 0);
            base.DeliverItem(this);
        }
    }
}
