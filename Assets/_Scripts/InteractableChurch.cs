using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InteractableChurch : InteractableHouse
{
    public int SleepEnergy;
    public int PrayEnergy;
    public int ServiceEnergy;

    public int PrayerPoints;

    private double LiturgyStartTime = -1;
    private double LiturgyEndTime = -1;
    public double ConfessionTime;
    public double MassStartTime;
    public double MassEndTime;

    public int LotHProgress;
    public int MassProgress;
    public int ConfessionProgress;
    public int PrayerProgress;
    public int SleepProgress;

    public float MaxSleepProgress = 2f;

    protected override void Start()
    {
        PopUILocation = "UI/ExternalUI";
        base.Start();
        UpdateLiturgyTimes();
        PopIcon.transform.localPosition += new Vector3 (0, 0.5f, 0);
        ExteriorPopUI.transform.localPosition += new Vector3(0, 1, 0);
        EnergyConsumption = ServiceEnergy;
        BuildPoints = (int)MaxBuildPoints;
        InventoryManager.RefreshInventoryUI += CheckProvisions;
    }

    public override void SetObjectiveParameters()
    {
        if (TutorialManager.Instance.CheckTutorialStepDialog(CustomEventType.NEW_TUTORIAL_1))
        {
            if (TutorialManager.Instance.Steps.Contains(CustomEventType.NEW_TUTORIAL_6))
            {
                if(MissionManager.Instance.FaithPointsPermanentlyLost > 0)
                {
                    EventsManager.Instance.AddEventToList(CustomEventType.NEW_TUTORIAL_FAILED_2);
                    MissionManager.Instance.FaithPointsPermanentlyLost = 0;
                }
                else
                {
                    EventsManager.Instance.AddEventToList(CustomEventType.NEW_TUTORIAL_FAILED_3);
                }
            }
            else if (TutorialManager.Instance.Steps.Contains(CustomEventType.NEW_TUTORIAL_3))
            {
                EventsManager.Instance.AddEventToList(CustomEventType.NEW_TUTORIAL_FAILED_1);
            }
        }
        else
        {
            var missionId = MissionManager.Instance.CurrentMissionId;
            if (missionId <= GameDataManager.MAX_MISSION_ID)
            {
                var eventId = GameDataManager.Instance.ObjectivesData[missionId].CustomEventId;
                if (eventId != CustomEventType.NONE && !(GameManager.Instance.SaveData.MissionEvents?.Contains(eventId) ?? false))
                {
                    EventsManager.Instance.AddEventToList(eventId);
                    EventsManager.Instance.TriggeredMissionEvents.Add(eventId);
                }
            }

            base.SetObjectiveParameters();
        }
    }

    public override void GetInteriorPopUI()
    {
        InteriorPopUI = UI.Instance.transform.Find("ChurchUI").GetComponent<PopUI>();
        base.GetInteriorPopUI();
    }

    public override void OnPlayerMoved(Energy energy, MapTile tile)
    {
        base.OnPlayerMoved(energy, tile);
        if (tile.GetInstanceID() == GetInstanceID())
        {
            StartCoroutine(FadeAndSwitchCamerasAsync(InteriorLightsOn));
            CheckCollectibleObjectives();
            TreasuryManager.Instance.DepositMoney();
            if(GameSettings.Instance.TUTORIAL_MODE && TutorialManager.Instance.Steps.Contains(CustomEventType.NEW_TUTORIAL_6))
            {
                TutorialManager.Instance.CheckTutorialStepDialog(CustomEventType.NEW_TUTORIAL_7);
            }
        }
        else
        {
            ExteriorPopUI.gameObject.SetActive(false);
            PopIcon.UIPopped(false);
        }
    }

    public void CheckCollectibleObjectives()
    {
        if (MissionManager.Instance.CurrentCollectibleCounter == GameDataManager.Instance.CollectibleObjectivesData[MissionManager.Instance.CurrentCollectibleMissionId].Amount)
        {
            EventsManager.Instance.AddEventToList(GameDataManager.Instance.CollectibleObjectivesData[MissionManager.Instance.CurrentCollectibleMissionId].OnComplete);
            MissionManager.Instance.CurrentCollectibleMissionId++;
            MissionManager.Instance.CurrentCollectibleCounter = 0;
            GridCollectibleManager.Instance.SacredItemSpawned = 0;
        }
    }

    public void CheckProvisions()
    {
        var rosary = InventoryManager.Instance.GetProvision(Provision.ROSARY);
        MaxPrayerProgress = 4 + (rosary?.Ticks ?? 0);
    }

    public override void Tick(double time, int day)
    {
        UpdateLiturgyTimes();
        base.Tick(time, day);
    }

    private void UpdateLiturgyTimes()
    {
        GameClock clock = GameManager.Instance.GameClock;
        CheckParticipation(clock);

    }

    public void CheckParticipation(GameClock clock)
    {
        if (!GameClock.DeltaTime) return;
        if (MyObjective == null) return;
        if (MyObjective != null && MyObjective.Event != BuildingEventType.MASS) return;

        if (clock.Time == MassStartTime)
        {
            if (ConfessionProgress == 0) //Did not participate at all
            {
                SoundManager.Instance.PlayOneShotSfx("FailedDeadline_SFX");
                UpdateFaithPoints(-1, 0);
            }
            StartCoroutine(ResetActionProgressAsync());
        }
        else if (clock.Time == MassEndTime)
        {
            if (MassProgress == 0)
            {
                SoundManager.Instance.PlayOneShotSfx("FailedDeadline_SFX");
                UpdateFaithPoints(-3, 0);
                CurrentMissionId++;
            }
            StartCoroutine(ResetActionProgressAsync());
            UI.Instance.SideNotificationPop(GetType().Name);
        }
    }

    public override bool CanDoAction(string actionName)
    {
        switch (actionName)
        {
            case "WORLD":
                return MissionManager.Instance.CurrentMissionId != 1 || GameManager.Instance.GameClock.Time != 5;
        }

        return base.CanDoAction(actionName);
    }

    public override void PopUICallback(string button)
    {
        base.PopUICallback(button);
        switch (button)
        {
            case "PRAY":
                Pray();
                break;

            case "SLEEP":
                Sleep();
                break;

            case "SAINTS":
                OpenUnlockedSaints();
                break;
        }
    }

    public override void PopMyIcon(string name = "", int items = -1, GameClock time = null)
    {
        TooltipMouseOver mouseOverBtn = InteriorPopUI.GetComponentsInChildren<TooltipMouseOver>(true).Where(b => b.name == "Pray").FirstOrDefault();

        //GameClock c = GameManager.Instance.GameClock;
        //if (c.Time < 19)
        //    base.PopMyIcon(GetType().Name, RequiredItems, new GameClock(LiturgyStartTime, c.Day));
        //else
        //{
        //    UI.Instance.SideNotificationPop(GetType().Name);
        //    PopIcon.gameObject.SetActive(false);
        //}
        GameClock clock = GameManager.Instance.GameClock;
        if (MyObjective != null && MyObjective.Event == BuildingEventType.MASS)
        {
            if (clock.Time > ConfessionTime && clock.Time < MassEndTime)
            {
                base.PopMyIcon(GetType().Name, RequiredItems, new GameClock(MassStartTime, GameManager.Instance.GameClock.Day));
                mouseOverBtn.Loc_Key = "Tooltip_Mass";
            }
            else
            {
                base.PopMyIcon(GetType().Name, RequiredItems, new GameClock(ConfessionTime, GameManager.Instance.GameClock.Day));
                if(clock.Time == ConfessionTime)
                    mouseOverBtn.Loc_Key = "Tooltip_Confession";
                else
                    mouseOverBtn.Loc_Key = "Tooltip_Pray";

                if (clock.Time == ConfessionTime && GameClock.DeltaTime)
                    SoundManager.Instance.PlayOneShotSfx("ChurchBells_SFX", timeToDie: 10f);
            }
        }
        else
        {
            mouseOverBtn.Loc_Key = "Tooltip_Pray";
        }
    }

    public void Pray()
    {
        GameClock clock = GameManager.Instance.GameClock;
        Player player = GameManager.Instance.Player;

        //if (MyObjective != null && MyObjective.Event == BuildingEventType.MASS)
        //{
        //    if (clock.Time == ConfessionTime)
        //    {
        //        ConfessionProgress++;
        //        var extraPoints = 0;
        //        if (PopUI.CriticalHitCount == 1) extraPoints = 1;
        //        OnActionProgress?.Invoke(ConfessionProgress/1f, this, 0);
        //    //    player.ConsumeEnergy(ServiceEnergy);
        //        UI.Instance.DisplayMessage("ATTENDED CONFESSION!!");

        //        UpdateFaithPoints(PrayerPoints + FPBonus + extraPoints);
        //        InteriorPopUI.PlayVFX("Halo");
        //        clock.Tick();
        //    }
        //    else if (clock.Time >= MassStartTime && clock.Time < MassEndTime)
        //    {
        //        MassProgress++;
        //        var extraPoints = 0;
        //        if (PopUI.CriticalHitCount == 2) extraPoints += 1;
        //        OnActionProgress?.Invoke(MassProgress/2f, this, 0);
        //        //    player.ConsumeEnergy(ServiceEnergy);
        //        UI.Instance.DisplayMessage("ATTENDED MASS!!");
        //        SoundManager.Instance.PlayOneShotSfx("MassBells_SFX", timeToDie: 10f);
        //        InteriorPopUI.PlayVFX("Halo2");
        //        if(clock.Time == MassStartTime)
        //        {
        //            SoundManager.Instance.PlayOneShotSfx("MassBegin_SFX", timeToDie: 4);
        //        }
        //        else
        //        {
        //            CurrentMissionId++;
        //            SoundManager.Instance.PlayOneShotSfx("MassEnd_SFX", timeToDie: 6);
        //        }
        //        if (MassProgress == 2)
        //        {
        //            var incense = InventoryManager.Instance.GetProvision(Provision.INCENSE);
        //            if (incense != null)
        //            {
        //                if (Random.Range(0, 100) <= incense.Value)
        //                {
        //                    extraPoints += 2;
        //                }
        //            }

        //            UpdateFaithPoints(PrayerPoints + FPBonus + extraPoints);
        //        }

        //        clock.Tick();
        //    }
        //    else
        //    {
        //    //    player.ConsumeEnergy(PrayEnergy);
        //        UI.Instance.DisplayMessage("PRAYED");
        //        PrayerProgress+=(int)MaxPrayerProgress;
        //        var extraPoints = 0;
        //        if (PopUI.CriticalHitCount == MaxPrayerProgress) extraPoints += 1;
        //        OnActionProgress?.Invoke(PrayerProgress / MaxPrayerProgress, this, 0);
        //        if (PrayerProgress == MaxPrayerProgress)
        //        {
        //            var provData = InventoryManager.Instance.GetProvision(Provision.ROSARY);
        //            extraPoints += provData?.Value ?? 0;
        //            var koboko = InventoryManager.Instance.GetProvision(Provision.KOBOKO);

        //            if (koboko != null)
        //            {
        //                extraPoints += koboko?.Value ?? 0;
        //                player.ConsumeEnergy(koboko.Value);
        //            }

        //            UpdateFaithPoints(PrayerPoints + FPBonus + extraPoints);
        //            PrayerProgress = 0;
        //        }
        //        for (int i = 0; i < MaxPrayerProgress; i++)
        //        {
        //            clock.Tick();
        //        }
        //    }
        //}
        //else
        {
            UI.Instance.DisplayMessage("PRAYED");
            PrayerProgress += (int)MaxPrayerProgress;
            var extraPoints = 0;
            if (PopUI.CriticalHitCount == MaxPrayerProgress) extraPoints += 1;

            var maxPP = MaxPrayerProgress;
            if (MyObjective?.Event == BuildingEventType.PRAY || MyObjective?.Event == BuildingEventType.PRAY_URGENT)
            {
                maxPP = 12; //3hrs minimum to complete prayer objective
            }

            if (PrayerProgress == MaxPrayerProgress)
            {
                var provData = InventoryManager.Instance.GetProvision(Provision.ROSARY);
                var koboko = InventoryManager.Instance.GetProvision(Provision.KOBOKO);
                var incense = InventoryManager.Instance.GetProvision(Provision.INCENSE);
                var bonusFPChance = incense?.Value / 100d ?? 0;

                extraPoints += provData?.FP ?? 0 + (Random.Range(0, 100) < bonusFPChance ? incense?.FP ?? 0 : 0);
                if (koboko != null)
                {
                    extraPoints += koboko?.FP ?? 0;
                    player.ConsumeEnergy(koboko.Energy);
                }

                PrayerProgress = 0;

                if(TutorialManager.Instance.CheckTutorialStepDialog(CustomEventType.NEW_TUTORIAL_2)) { }
                else if(MissionManager.Instance.CurrentMissionId == 1 && !(GameManager.Instance.SaveData.MissionEvents?.Contains(CustomEventType.MISSION_1) ?? false))
                {
                    EventsManager.Instance.AddEventToList(CustomEventType.MISSION_1);
                    EventsManager.Instance.TriggeredMissionEvents.Add(CustomEventType.MISSION_1);
                    foreach(var h in GameManager.Instance.Houses)
                    {
                        h.SetObjectiveParameters();
                    }
                }

                if (MyObjective?.Event == BuildingEventType.PRAY || MyObjective?.Event == BuildingEventType.PRAY_URGENT)
                {
                    VolunteerProgress++;
                    OnActionProgress?.Invoke(VolunteerProgress / (float)MyObjective.RequiredAmount, this, 0);
                    if (VolunteerProgress >= MyObjective.RequiredAmount)
                    {
                        BuildRelationship(ThankYouType.VOLUNTEER);
                        CurrentMissionId++;
                        UpdateFaithPoints(MyObjective.Reward, 0);
                        MyObjective = null;
                        VolunteerProgress = 0;
                        CurrentMissionCompleteToday = true;
                    }
                }
                else
                {
                    UpdateFaithPoints(MeditationPoints + FPBonus + extraPoints);
                }
            }
            for (int i = 0; i < maxPP; i++)
            {
                clock.Tick();
            }
        }
    }

    public override void TriggerCustomEvent()
    {
        GameClock clock = GameManager.Instance.GameClock;
        if (clock.Time+0.5 == ConfessionTime || (clock.Time+0.5 >= MassStartTime && clock.Time < MassEndTime)) return;
        base.TriggerCustomEvent();
    }

    public override void ReportScores()
    {
        CurrentFaithPoints = 0;
    }

    public void Sleep()
    {
        MissionManager.Instance.EndDay();

        //GameClock clock = GameManager.Instance.GameClock;
        //Player player = GameManager.Instance.Player;

        //SleepProgress++;
        //var extraPoints = 0;
        //if (PopUI.CriticalHitCount == MaxSleepProgress) extraPoints = 1;
        //OnActionProgress?.Invoke(SleepProgress / MaxSleepProgress, this, 1);
        //if (SleepProgress == MaxSleepProgress)
        //{
        //    var mattress = InventoryManager.Instance.GetProvision(Provision.SOFT_MATTRESS);

        //    player.ConsumeEnergy(SleepEnergy - (mattress?.Value ?? 0) - extraPoints);
        //    player.RemoveRandomStatusEffect();
        //    PopUIFXIcons("Energy", -SleepEnergy);
        //    SleepProgress = 0;
        //}

        //if(MaxSleepProgress > 0)
        //{
        //    clock.Tick();
        //}
        //UI.Instance.DisplayMessage("SLEPT!");
    }

    public void OpenUnlockedSaints()
    {
        UI.Instance.ToggleSaintsCollection(true);
    }

    private IEnumerator ResetActionProgressAsync()
    {
        yield return new WaitForSeconds(1.5f);
        ResetActionProgress();
        OnActionProgress?.Invoke(0, this, 0);
        OnActionProgress?.Invoke(0, this, 1);
    }

    public override void ResetActionProgress()
    {
        MassProgress = 0;
        LotHProgress = 0;
        ConfessionProgress = 0;
        PrayerProgress = 0;
        base.ResetActionProgress();
    }

    public override bool HasResetActionProgress()
    {
        return MassProgress == 0 && LotHProgress == 0 && ConfessionProgress == 0 && PrayerProgress == 0 && SleepProgress == 0;
    }

    public override TooltipStats GetTooltipStatsForButton(string button)
    {
        switch (button)
        {
            case "PRAY":
                GameClock clock = GameManager.Instance.GameClock;
                CustomEventData e = EventsManager.Instance.CurrentEvents.Find(i => i.Id == CustomEventType.WEEKDAY_MASS);

                var incense = InventoryManager.Instance.GetProvision(Provision.INCENSE);
                var bonusFPChance = incense?.Value / 100d ?? 0;

                var rosary = InventoryManager.Instance.GetProvision(Provision.ROSARY);
                var koboko = InventoryManager.Instance.GetProvision(Provision.KOBOKO);
                var bonusFp = (koboko?.FP ?? 0) + (rosary?.FP ?? 0) + (Random.Range(0, 100) < bonusFPChance ? incense?.FP ?? 0 : 0);
                var maxPP = 0;
                if (MyObjective?.Event == BuildingEventType.PRAY || MyObjective?.Event == BuildingEventType.PRAY_URGENT)
                {
                    maxPP = 12;
                }

                return GameDataManager.Instance.GetToolTip(TooltipStatId.PRAY, ticksOverride:maxPP, ticksModifier: rosary != null ? rosary.Ticks : 0, energyModifier: -(koboko?.Energy ?? 0), fpModifier: FPBonus + bonusFp, fpOverride:MyObjective?.Reward ?? 0);

            case "SLEEP":
                if (MaxSleepProgress - SleepProgress == 1)
                {
                    var mattress = InventoryManager.Instance.GetProvision(Provision.SOFT_MATTRESS);
                    return GameDataManager.Instance.GetToolTip(TooltipStatId.SLEEP, energyModifier: mattress?.Value ?? 0);
                }
                else
                    return GameDataManager.Instance.GetToolTip(TooltipStatId.TIME);

        }

        return base.GetTooltipStatsForButton(button);
    }

    public override float SetButtonTimer(string actionName)
    {
        switch (actionName)
        {
            case "PRAY":

                if (MyObjective != null && MyObjective.Event == BuildingEventType.MASS)
                {
                    GameClock clock = GameManager.Instance.GameClock;
                    if (clock.Time == ConfessionTime)
                    {
                        return 2f;
                    }
                    else if (clock.Time >= MassStartTime && clock.Time < MassEndTime)
                    {
                        return 4f;
                    }
                }
                break;
        }

        return base.SetButtonTimer(actionName);
    }

    public override CustomEventType GetEndGameStory()
    {
        return CustomEventType.ENDGAME_CHURCH;
    }

    public override void SetDeadlineTime(double time, int day)
    {

    }
    protected override void AutoDeliver(ItemType item)
    {

    }

    public override void OnDisable()
    {
        InventoryManager.RefreshInventoryUI -= CheckProvisions;
        base.OnDisable();
    }
}
