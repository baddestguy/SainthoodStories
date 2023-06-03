using UnityEngine;

public class InteractableSchool : InteractableHouse
{
    public int TeachPoints;
    private int TeachCountdown = 0;
    private float MaxTeachPoints = 4f;

    protected override void Start()
    {
        PopUILocation = "UI/ExternalUI";
        base.Start();
    }

    public override void GetInteriorPopUI()
    {
        InteriorPopUI = UI.Instance.transform.Find("SchoolUI").GetComponent<PopUI>();
        base.GetInteriorPopUI();
    }

    public override void OnPlayerMoved(Energy energy, MapTile tile)
    {
        base.OnPlayerMoved(energy, tile);
        GameClock clock = GameManager.Instance.GameClock;
        if (tile.GetInstanceID() == GetInstanceID())
        {
            if (BuildingState != BuildingState.RUBBLE)
            {
                StartCoroutine(FadeAndSwitchCamerasAsync(InteriorLightsOn));
                MaxTeachPoints = CalculateMaxVolunteerPoints();
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

    public void Teach()
    {
        GameClock clock = GameManager.Instance.GameClock;
        Player player = GameManager.Instance.Player;
        if (player.EnergyDepleted()) return;

        if (DuringOpenHours())
        {
            BuildingActivityState = BuildingActivityState.TEACHING;
            UI.Instance.DisplayMessage("Taught a Class!!");
            clock.Tick();
        }
        else
        {
            UI.Instance.DisplayMessage("SCHOOL CLOSED!");
        }
    }

    public override void Tick(double time, int day)
    {
        if(BuildingActivityState == BuildingActivityState.TEACHING)
        {
            TeachSubject();
        }

        base.Tick(time, day);
    }

    public override void TriggerHazardousMode(double time, int day)
    {
        if (HazardCounter > 0) return;
        if (MissionManager.Instance.CurrentMission.CurrentWeek < 2) return;

        TeachCountdown = 0;
        base.TriggerHazardousMode(time, day);
    }

    public void TeachSubject()
    {
        TeachCountdown++;
        var extraPoints = 0;
        if (PopUI.CriticalHitCount == MaxTeachPoints) extraPoints = 1;
        OnActionProgress?.Invoke(TeachCountdown / MaxTeachPoints, this, 1);

        if (TeachCountdown >= MaxTeachPoints)
        {
            Player player = GameManager.Instance.Player;
            var moddedEnergy = player.ModifyEnergyConsumption(amount: EnergyConsumption);
            var schoolMaterials = InventoryManager.Instance.GetProvision(Provision.SCHOOL_RELATIONSHIP_BUILDER);
            moddedEnergy += schoolMaterials?.Value ?? 0;
            player.ConsumeEnergy(EnergyConsumption);

            UpdateCharityPoints(TeachPoints+ extraPoints, moddedEnergy);
            BuildRelationship(ThankYouType.TEACH);
            TeachCountdown = 0;
        }
    }

    public override void BuildRelationship(ThankYouType thanks, int amount = 1)
    {
        if(thanks == ThankYouType.TEACH)
        {
            var schoolMaterials = InventoryManager.Instance.GetProvision(Provision.SCHOOL_RELATIONSHIP_BUILDER);
            amount += schoolMaterials?.Value ?? 0;
        }
        base.BuildRelationship(thanks, amount);
    }

    public override void DeliverItem(InteractableHouse house, bool autoDeliver = false)
    {
        if (house != this) return;

        ItemType item = InventoryManager.Instance.GetItem(ItemType.STATIONERY);

        if (item != ItemType.NONE)
        {
            UI.Instance.DisplayMessage("DELIVERED STATIONERY!");
            UpdateCharityPoints(ItemDeliveryPoints * DeadlineDeliveryBonus, 0);
            base.DeliverItem(house, autoDeliver);
        }
        else
        {
            UI.Instance.DisplayMessage("YOU HAVE NO STATIONERY TO GIVE!");
        }
    }

    public override void ItemDeliveryThanks()
    {
        EventsManager.Instance.AddEventToList(CustomEventType.THANKYOU_ITEM_SCHOOL);
        base.ItemDeliveryThanks();
    }

    public override bool DuringOpenHours(GameClock newClock = null)
    {
        GameClock clock = newClock ?? GameManager.Instance.GameClock;
        return base.DuringOpenHours() && clock.Day <= 5;
    }

    public override void PopUICallback(string button)
    {
        base.PopUICallback(button);

        switch (button)
        {
            case "TEACH":
                Teach();
                break;

            case "STATIONERY":
                DeliverItem(this);
                break;

            case "PRAY":
                UI.Meditate?.Invoke(this);
                break;
        }
    }

    public override void SetDeadlineTime(double time, int day)
    {
        if (BuildingState != BuildingState.NORMAL) return;
        if (time >= 19) return;

    //    if (!DuringOpenHours()) return;
        if ((DeadlineTime.Time != -1)) return;

        double futureTime = time + RandomFutureTimeByDifficulty();
   //     if (futureTime > ClosingTime) return;

        switch (MissionDifficulty)
        {
            case MissionDifficulty.EASY:
                if (DeadlineCounter < 1)
                {
                    if (Random.Range(0, 100) < 1)
                    {
                        DeadlineCounter++;
                        DeadlineTime.SetClock(futureTime, day);
                        DeadlineDeliveryBonus = 4;
                        RequiredItems = 1;
                        DeadlineSet = true;
                        PopMyIcon();
                        Debug.LogWarning($"{name}: DEADLINE SET FOR {DeadlineTime.Time} : {DeadlineTime.Day}!");
                    }
                }
                break;

            case MissionDifficulty.NORMAL:
                if (DeadlineCounter < 2)
                {
                    if (Random.Range(0, 100) < 3)
                    {
                        DeadlineCounter++;
                        DeadlineTime.SetClock(futureTime, day);
                        DeadlineDeliveryBonus = 3;
                        RequiredItems = Random.Range(1, 3);
                        DeadlineSet = true;
                        PopMyIcon();
                        Debug.LogWarning($"{name}: DEADLINE SET FOR {DeadlineTime.Time} : {DeadlineTime.Day}!");
                    }
                }
                break;

            case MissionDifficulty.HARD:
                if (DeadlineCounter < 3)
                {
                    var mission = GetBuildingMission(BuildingEventType.DELIVER_ITEM);
                    if (mission != null || (!SameDayAsMission() && Random.Range(0, 100) < 10))
                    {
                        DeadlineCounter++;
                        DeadlineTime.SetClock(mission != null ? mission.DeadlineHours : futureTime, day);
                        if (MissionManager.Instance.CurrentMission.CurrentWeek > 2)
                            RequiredItems = mission != null ? mission.RequiredItems : Random.Range(1, 3); //Depending on Season
                        else
                            RequiredItems = mission != null ? mission.RequiredItems : 1; //Depending on Season
                        DeadlineDeliveryBonus = 1;
                        DeadlineSet = true;
                        PopMyIcon();
                        SoundManager.Instance.PlayOneShotSfx("Notification_SFX");
                        Debug.LogWarning($"{name}: DEADLINE SET FOR {DeadlineTime.Time} : {DeadlineTime.Day}!");
                    }
                }
                break;
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

    public override TooltipStats GetTooltipStatsForButton(string button)
    {
        switch (button)
        {
            case "VOLUNTEER":
                if (MaxTeachPoints - TeachCountdown == 1)
                    return new TooltipStats() { Ticks = 1, FP = 0, CP = TeachPoints, Energy = -GameManager.Instance.Player.ModifyEnergyConsumption(amount: EnergyConsumption) };
                else
                    return new TooltipStats() { Ticks = 1, FP = 0, CP = 0, Energy = -GameManager.Instance.Player.ModifyEnergyConsumption(amount: EnergyConsumption) };
        }

        return base.GetTooltipStatsForButton(button);
    }

    public override bool CanDoAction(string actionName)
    {
        switch (actionName)
        {
            case "STATIONERY":
                return InventoryManager.Instance.CheckItem(ItemType.STATIONERY);

            case "TEACH":
                Player player = GameManager.Instance.Player;
                return !player.EnergyDepleted() && DuringOpenHours();
        }

        return base.CanDoAction(actionName);
    }

    protected override void OnEventExecuted(CustomEventData e)
    {
        switch (e.Id)
        {
            case CustomEventType.SCHOOL_CLOSED:
                ClosingTime = 0;
                break;
        }
    }

    public override void ResetActionProgress()
    {
        TeachCountdown = 0;
        base.ResetActionProgress();
    }

    public override bool HasResetActionProgress()
    {
        return TeachCountdown == 0 && base.HasResetActionProgress();
    }


    protected override void AutoDeliver(ItemType item)
    {
        if (item == ItemType.STATIONERY)
        {
            UpdateCharityPoints(ItemDeliveryPoints * DeadlineDeliveryBonus, 0);
            base.DeliverItem(this, true);
        }
    }

    public override void OnDisable()
    {
        base.OnDisable();
    }
}
