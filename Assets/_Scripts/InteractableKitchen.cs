using UnityEngine;

public class InteractableKitchen : InteractableHouse
{
    public ParticleSystem CookFX;

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

    public void Cook()
    {

        Player player = GameManager.Instance.Player;
        if (player.EnergyDepleted()) return;

        ItemType item = InventoryManager.Instance.GetItem(ItemType.GROCERIES);
        GameClock clock = GameManager.Instance.GameClock;

        if (item != ItemType.NONE)
        {
            var utensils = InventoryManager.Instance.GetProvision(Provision.COOKING_UTENSILS);
            var moddedEnergy = player.ModifyEnergyConsumption(amount: EnergyConsumption);

            if (utensils != null)
            {
                moddedEnergy += utensils.Value;
            }

            if (RelationshipPoints >= 10 && RelationshipPoints < 30)
            {
                player.ConsumeEnergy(moddedEnergy);
            }
            else
            {
                player.ConsumeEnergy(moddedEnergy);
                clock.Tick();
            }

            BuildRelationship(ThankYouType.VOLUNTEER);

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

    public override void OnDisable()
    {
        base.OnDisable();
    }
}
