using System.Linq;
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

            if (MyObjective != null)
            {
                RequiredItems--;
                var amt = MyObjective.RequiredAmount - RequiredItems;
                OnActionProgress?.Invoke(amt / (float)MyObjective.RequiredAmount, this, 2);

                var extraPoints = 0;
                if (PopUI.CriticalHitCount == 1) extraPoints = 1;

                if (RequiredItems <= 0)
                {
                    RequiredItems = 0;

                    if (MyObjective?.Event == BuildingEventType.COOK || MyObjective?.Event == BuildingEventType.COOK_URGENT)
                    {
                        BuildRelationship(ThankYouType.VOLUNTEER);
                        CurrentMissionId++;
                        var moddedCPReward = extraPoints;
                        UpdateCharityPoints(MyObjective.Reward + moddedCPReward, 0);
                        MyObjective = null;
                        CurrentMissionCompleteToday = true;
                    
                        var obj = GameDataManager.Instance.HouseObjectivesData[HouseName][CurrentMissionId];
                        RequiredItems = obj.RequiredAmount;
                    }
                }            
            }
            else
            {
                var shelter = GameManager.Instance.Houses.Where(h => h is InteractableShelter).FirstOrDefault();
                if (shelter != null && shelter.MyObjective != null)
                {
                    if(InventoryManager.Instance.CountItem(ItemType.MEAL) < shelter.RequiredItems)
                    {
                        InventoryManager.Instance.AddToInventory(ItemType.MEAL);
                    }
                }
            }

            TreasuryManager.Instance.DonateMoney(2);
            CookFX.Play();

            var moddedEnergy = EnergyConsumption + utensils?.Value ?? 0;
            var ticks = 0;
            if (UpgradeLevel == 3)
            {
                ticks = 4;
            }
            else if (UpgradeLevel == 2)
            {
                ticks = 8;
            }
            else if (UpgradeLevel == 1)
            {
                ticks = 12;
            }
            else
            {
                ticks = 14;
            }

            player.ConsumeEnergy(moddedEnergy);
            for(int i = 0; i < ticks; i++)
            {
                clock.Tick();
            }

            UI.Instance.DisplayMessage("COOKED MEAL!");
        }
        else
        {
            UI.Instance.DisplayMessage("YOU HAVE NO FOOD TO COOK!");
        }
    }

    public override void BuildRelationship(ThankYouType thanks, int amount = 3)
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

    public override void TriggerUpgradeStory()
    {
        if (HasBeenDestroyed) return;

        if (UpgradeLevel == 3 && !MyStoryEvents.Contains(CustomEventType.KITCHEN_STORY_3))
        {
            EventsManager.Instance.AddEventToList(CustomEventType.KITCHEN_STORY_3);
            MyStoryEvents.Add(CustomEventType.KITCHEN_STORY_3);
        }
        else if (UpgradeLevel == 2 && !MyStoryEvents.Contains(CustomEventType.KITCHEN_STORY_2))
        {
            EventsManager.Instance.AddEventToList(CustomEventType.KITCHEN_STORY_2);
            MyStoryEvents.Add(CustomEventType.KITCHEN_STORY_2);
        }
        else if (UpgradeLevel == 1 && !MyStoryEvents.Contains(CustomEventType.KITCHEN_STORY_1))
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
