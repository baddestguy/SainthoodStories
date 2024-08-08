public class InteractableShelter : InteractableHouse
{
    protected override void Start()
    {
        PopUILocation = "UI/ExternalUI";
        base.Start();
    }

    public override void GetInteriorPopUI()
    {
        InteriorPopUI = UI.Instance.transform.Find("FoodShelterUI").GetComponent<PopUI>();
        base.GetInteriorPopUI();
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

    public override void SetObjectiveParameters()
    {
        if (MyObjective == null) return;

        base.SetObjectiveParameters();
    }

    public override void DeliverItem(InteractableHouse house, bool autoDeliver = false)
    {
        if (house != this) return;

        if(DeadlineSet)
        {
            DeliverDeadlineItem(autoDeliver);
            return;
        }

        ItemType groceries = InventoryManager.Instance.GetItem(ItemType.GROCERIES);

        if (groceries != ItemType.NONE)
        {
            UI.Instance.DisplayMessage("DELIVERED GROCERIES!");
            base.DeliverItem(house, autoDeliver);
            return;
        }

        ItemType meal = InventoryManager.Instance.GetItem(ItemType.MEAL);
        if (meal != ItemType.NONE)
        {
            UI.Instance.DisplayMessage("FED THE HUNGRY!");
            UpdateCharityPoints(1, 0);
            base.DeliverItem(house, autoDeliver);
            return;
        }

        UI.Instance.DisplayMessage("YOU HAVE NO FOOD TO GIVE!");
    }

    public override void ItemDeliveryThanks()
    {
        EventsManager.Instance.AddEventToList(CustomEventType.THANKYOU_ITEM_FOOD);
        base.ItemDeliveryThanks();
    }

    private void DeliverDeadlineItem( bool autoDeliver = false)
    {
        ItemType meal = InventoryManager.Instance.GetItem(ItemType.MEAL);
        if (meal != ItemType.NONE)
        {
            UI.Instance.DisplayMessage("FED THE HUNGRY!");
            UpdateCharityPoints(1, 0);
            base.DeliverItem(this, autoDeliver);
        }
        else
        {
            ItemType grocery = InventoryManager.Instance.GetItem(ItemType.GROCERIES);
            if (grocery != ItemType.NONE)
            {
                UI.Instance.DisplayMessage("FED THE HUNGRY!");
                base.DeliverItem(this, autoDeliver);
            }
        }
    }

    public override void BuildRelationship(ThankYouType thanks, int amount = 1)
    {
        if(thanks == ThankYouType.ITEM)
        {
            var shelterMaterials = InventoryManager.Instance.GetProvision(Provision.SHELTER_RELATIONSHIP_BUILDER);
            amount += shelterMaterials?.Value ?? 0;
            TreasuryManager.Instance.DonateMoney(shelterMaterials?.Value ?? 0);
        }
        if (thanks == ThankYouType.IMMEDIATE_ASSISTANCE)
        {
            amount += 2;
        }
        base.BuildRelationship(thanks, amount);
    }

    public override void RelationshipReward(ThankYouType thanks)
    {
        if (RelationshipPoints == 100)
        {
            //One time special reward!
        }

        base.RelationshipReward(thanks);
        
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
                return MyObjective != null && (InventoryManager.Instance.CheckItem(ItemType.GROCERIES) || InventoryManager.Instance.CheckItem(ItemType.MEAL));
        }

        return base.CanDoAction(actionName);
    }

    protected override void AutoDeliver(ItemType item)
    {
        if (item == ItemType.GROCERIES)
        {
            EventsManager.Instance.AddEventToList(CustomEventType.AUTO_DELIVER_COMPLETE);
            base.DeliverItem(this, true);
        }
    }

    public override void TriggerUpgradeStory()
    {
        if (HasBeenDestroyed) return;

        if (UpgradeLevel == 3 && !MyStoryEvents.Contains(CustomEventType.SHELTER_STORY_3))
        {
            EventsManager.Instance.AddEventToList(CustomEventType.SHELTER_STORY_3);
            MyStoryEvents.Add(CustomEventType.SHELTER_STORY_3);
        }
        else if (UpgradeLevel == 2 && !MyStoryEvents.Contains(CustomEventType.SHELTER_STORY_2))
        {
            EventsManager.Instance.AddEventToList(CustomEventType.SHELTER_STORY_2);
            MyStoryEvents.Add(CustomEventType.SHELTER_STORY_2);
        }
        else if (UpgradeLevel == 1 && !MyStoryEvents.Contains(CustomEventType.SHELTER_STORY_1))
        {
            EventsManager.Instance.AddEventToList(CustomEventType.SHELTER_STORY_1);
            MyStoryEvents.Add(CustomEventType.SHELTER_STORY_1);
        }
    }

    public override CustomEventType GetEndGameStory()
    {
        return CustomEventType.ENDGAME_SHELTER;
    }

}
