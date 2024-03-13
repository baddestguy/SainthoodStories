using UnityEngine;

public class InteractableKitchen : InteractableHouse
{
    public ParticleSystem CookFX;

    protected override void Start()
    {
        PopUILocation = "UI/ExternalUI";
        base.Start();
    }

    public override void GetInteriorPopUI()
    {
        InteriorPopUI = UI.Instance.transform.Find("KitchenUI").GetComponent<PopUI>();
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

    public override void UpgradeThanks()
    {
        EventsManager.Instance.AddEventToList(CustomEventType.THANKYOU_UPGRADE_KITCHEN);
    }

    public void Cook()
    {

        Player player = GameManager.Instance.Player;
        if (player.EnergyDepleted())
        {
            UI.Instance.ErrorFlash("Energy");
            return;
        }

        ItemType item = InventoryManager.Instance.GetItem(ItemType.GROCERIES);
        GameClock clock = GameManager.Instance.GameClock;

        if (item != ItemType.NONE)
        {
            var utensils = InventoryManager.Instance.GetProvision(Provision.COOKING_UTENSILS);
            
            if (RelationshipPoints >= 15)
            {
                player.ConsumeEnergy(EnergyConsumption + utensils?.Value ?? 0);
            }
            else
            {
                player.ConsumeEnergy(EnergyConsumption + utensils?.Value ?? 0);
                clock.Tick();
            }

            BuildRelationship(ThankYouType.VOLUNTEER);
            TreasuryManager.Instance.DonateMoney(2);

            InventoryManager.Instance.AddToInventory(ItemType.MEAL);
            CookFX.Play();
            UI.Instance.DisplayMessage("COOKED MEAL!");
        }
        else
        {
            UI.Instance.DisplayMessage("YOU HAVE NO FOOD TO COOK!");
        }
    }

    public override void BuildRelationship(ThankYouType thanks, int amount = 1)
    {
        if (thanks == ThankYouType.VOLUNTEER)
        {
            var utensils = InventoryManager.Instance.GetProvision(Provision.COOKING_UTENSILS);
            amount += utensils?.Value ?? 0;
        }
        base.BuildRelationship(thanks, amount);
    }

    public override void SetDeadlineTime(double time, int day)
    {

    }

    public override void ReportScores()
    {
        //Do absolutely nothing!
    }

    public override void PopUICallback(string button)
    {
        base.PopUICallback(button);

        switch (button)
        {
            case "COOK":
                Cook();
                break;

            case "PRAY":
                UI.Meditate?.Invoke(this);
                break;
        }
    }

    public override bool CanDoAction(string actionName)
    {
        Player player = GameManager.Instance.Player;
        switch (actionName)
        {
            case "COOK":
                return !player.EnergyDepleted() && DuringOpenHours() && InventoryManager.Instance.CheckItem(ItemType.GROCERIES);
        }

        return base.CanDoAction(actionName);
    }
    protected override void AutoDeliver(ItemType item)
    {

    }

    public override void TriggerStory()
    {
        if (HasBeenDestroyed) return;

        if (RelationshipPoints >= GameDataManager.MAX_RP_THRESHOLD && !MyStoryEvents.Contains(CustomEventType.KITCHEN_STORY_3))
        {
            EventsManager.Instance.AddEventToList(CustomEventType.KITCHEN_STORY_3);
            MyStoryEvents.Add(CustomEventType.KITCHEN_STORY_3);
        }
        else if (RelationshipPoints >= GameDataManager.MED_RP_THRESHOLD && !MyStoryEvents.Contains(CustomEventType.KITCHEN_STORY_2))
        {
            EventsManager.Instance.AddEventToList(CustomEventType.KITCHEN_STORY_2);
            MyStoryEvents.Add(CustomEventType.KITCHEN_STORY_2);
        }
        else if (RelationshipPoints >= GameDataManager.MIN_RP_THRESHOLD && !MyStoryEvents.Contains(CustomEventType.KITCHEN_STORY_1))
        {
            EventsManager.Instance.AddEventToList(CustomEventType.KITCHEN_STORY_1);
            MyStoryEvents.Add(CustomEventType.KITCHEN_STORY_1);
        }
    }

    public override CustomEventType GetEndGameStory()
    {
        return CustomEventType.ENDGAME_KITCHEN;
    }

    public override void OnDisable()
    {
        base.OnDisable();
    }
}
