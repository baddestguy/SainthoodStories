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

    private double LiturgyStartTime;
    private double LiturgyEndTime;
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
        BuildPoints = MaxBuildPoints;
        InventoryManager.RefreshInventoryUI += CheckProvisions;
    }

    public override void OnPlayerMoved(Energy energy, MapTile tile)
    {
        base.OnPlayerMoved(energy, tile);
        if (tile.GetInstanceID() == GetInstanceID())
        {
            StartCoroutine(FadeAndSwitchCamerasAsync(InteriorLightsOn));
            SoundManager.Instance.SwitchMusicChannel(true);
        }
        else
        {
            ExteriorPopUI.gameObject.SetActive(false);
            PopIcon.UIPopped(false);
            SoundManager.Instance.SwitchMusicChannel(false);
        }
    }

    public void CheckProvisions()
    {
        var rosary = InventoryManager.Instance.GetProvision(Provision.ROSARY);
        MaxPrayerProgress = rosary != null ? 5f : 4f;

        var ac = InventoryManager.Instance.GetProvision(Provision.REDUCE_SLEEP_TIME);
        MaxSleepProgress = ac?.Value ?? 2f;

        var mattress = InventoryManager.Instance.GetProvision(Provision.SOFT_MATTRESS);
        MaxSleepProgress += mattress?.Value ?? 0;
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

        if (clock.Time > 23 || clock.Time <= 6.5)
        {
            LiturgyStartTime = 6;
            LiturgyEndTime = 7;
        }
        else if(clock.Time > 18.5)
        {
            LiturgyStartTime = 22.5;
            LiturgyEndTime = 23.5;
        }
        else if(clock.Time > 12.5)
        {
            LiturgyStartTime = 18;
            LiturgyEndTime = 19;
        }
        else if(clock.Time > 6.5)
        {
            LiturgyStartTime = 12;
            LiturgyEndTime = 13;
        }

        PopMyIcon();
    }

    public void CheckParticipation(GameClock clock)
    {
        if (!GameClock.DeltaTime) return;
        CustomEventData e = EventsManager.Instance.CurrentEvents.Find(i => i.Id == CustomEventType.WEEKDAY_MASS);
        if (clock.Day % 5 == 0 || e != null)
        {
            if (clock.Time == MassStartTime)
            {
                if(ConfessionProgress == 0) //Did not participate at all
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
                }
                StartCoroutine(ResetActionProgressAsync());
            }
            else if (clock.Time == LiturgyEndTime)
            {
                if (LotHProgress == 0) 
                {
                    SoundManager.Instance.PlayOneShotSfx("FailedDeadline_SFX");
                    UpdateFaithPoints(-2, 0);
                }
                StartCoroutine(ResetActionProgressAsync());
            }
        }
        else
        {
            if (clock.Time == LiturgyEndTime)
            {
                if (LotHProgress == 0) //Did not participate at all
                {
                    SoundManager.Instance.PlayOneShotSfx("FailedDeadline_SFX");
                    UpdateFaithPoints(-2, 0);
                }
                StartCoroutine(ResetActionProgressAsync());
            }
        }
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

        GameClock c = GameManager.Instance.GameClock;
        if (c.Time > 23)
            base.PopMyIcon(GetType().Name, RequiredItems, new GameClock(LiturgyStartTime, c.Day + 1));
        else
            base.PopMyIcon(GetType().Name, RequiredItems, new GameClock(LiturgyStartTime, c.Day));

        GameClock clock = GameManager.Instance.GameClock;
        CustomEventData e = EventsManager.Instance.CurrentEvents.Find(i => i.Id == CustomEventType.WEEKDAY_MASS);
        if (clock.Day % 5 == 0 || e!= null)
        {
            if (clock.Time > ConfessionTime && clock.Time < MassEndTime)
            {
                base.PopMyIcon(GetType().Name, RequiredItems, new GameClock(MassStartTime, GameManager.Instance.GameClock.Day));
                mouseOverBtn.Loc_Key = "Tooltip_Mass";
            }
            else if (clock.Time > 12.5 && clock.Time <= ConfessionTime)
            {
                base.PopMyIcon(GetType().Name, RequiredItems, new GameClock(ConfessionTime, GameManager.Instance.GameClock.Day));
                LiturgyStartTime = ConfessionTime;
                if(clock.Time == ConfessionTime)
                    mouseOverBtn.Loc_Key = "Tooltip_Confession";

                if (clock.Time == ConfessionTime && GameClock.DeltaTime)
                    SoundManager.Instance.PlayOneShotSfx("ChurchBells_SFX", timeToDie: 10f);
            }
            else
            {
                if (clock.Time >= LiturgyStartTime && clock.Time < LiturgyEndTime)
                    mouseOverBtn.Loc_Key = "Tooltip_Liturgy";
                else
                    mouseOverBtn.Loc_Key = "Tooltip_Pray";

                if (clock.Time == LiturgyStartTime && GameClock.DeltaTime)
                    SoundManager.Instance.PlayOneShotSfx("ChurchBells_SFX", timeToDie: 10f);
            }
        }
        else
        {
            if (clock.Time >= LiturgyStartTime && clock.Time < LiturgyEndTime)
                mouseOverBtn.Loc_Key = "Tooltip_Liturgy";
            else
                mouseOverBtn.Loc_Key = "Tooltip_Pray";

            if (clock.Time == LiturgyStartTime && GameClock.DeltaTime)
                SoundManager.Instance.PlayOneShotSfx("ChurchBells_SFX", timeToDie: 10f);
        }
    }

    public void Pray()
    {
        GameClock clock = GameManager.Instance.GameClock;
        Player player = GameManager.Instance.Player;
        CustomEventData e = EventsManager.Instance.CurrentEvents.Find(i => i.Id == CustomEventType.WEEKDAY_MASS);

        if (clock.Day % 5 == 0 || e != null)
        {
            if (clock.Time == ConfessionTime)
            {
                ConfessionProgress++;
                OnActionProgress?.Invoke(ConfessionProgress/1f, this, 0);
            //    player.ConsumeEnergy(ServiceEnergy);
                UI.Instance.DisplayMessage("ATTENDED CONFESSION!!");
                UpdateFaithPoints(PrayerPoints, 0);
                InteriorPopUI.PlayVFX("Halo");
                clock.Tick();
            }
            else if (clock.Time >= MassStartTime && clock.Time < MassEndTime)
            {
                MassProgress++;
                OnActionProgress?.Invoke(MassProgress/2f, this, 0);
            //    player.ConsumeEnergy(ServiceEnergy);
                UI.Instance.DisplayMessage("ATTENDED MASS!!");
                SoundManager.Instance.PlayOneShotSfx("MassBells_SFX", timeToDie: 10f);
                InteriorPopUI.PlayVFX("Halo2");
                if(clock.Time == MassStartTime)
                {
                    SoundManager.Instance.PlayOneShotSfx("MassBegin_SFX", timeToDie: 4);
                }
                else
                {
                    SoundManager.Instance.PlayOneShotSfx("MassEnd_SFX", timeToDie: 6);
                }
                if (MassProgress == 2)
                {
                    var incense = InventoryManager.Instance.GetProvision(Provision.INCENSE);
                    if (incense != null)
                    {
                        if (Random.Range(0, 100) <= incense.Value)
                        {
                            FPBonus += 2;
                        }
                    }
                    UpdateFaithPoints(PrayerPoints + FPBonus, 0);
                }

                clock.Tick();
            }
            else if (clock.Time >= LiturgyStartTime && clock.Time < LiturgyEndTime)
            {
                LotHProgress++;
                OnActionProgress?.Invoke(LotHProgress/2f, this, 0);
            //    player.ConsumeEnergy(ServiceEnergy);
                UI.Instance.DisplayMessage("ATTENDED LITURGY OF HOURS!!");
                SoundManager.Instance.PlayOneShotSfx("MassBells_SFX", timeToDie: 10f);
                InteriorPopUI.PlayVFX("Halo");
                if (LotHProgress == 2)
                {
                    var incense = InventoryManager.Instance.GetProvision(Provision.INCENSE);
                    if (incense != null)
                    {
                        if (Random.Range(0, 100) <= incense.Value)
                        {
                            FPBonus += 2;
                        }
                    }
                    UpdateFaithPoints(PrayerPoints + FPBonus, 0);
                }

                clock.Tick();
            }
            else
            {
            //    player.ConsumeEnergy(PrayEnergy);
                UI.Instance.DisplayMessage("PRAYED");
                PrayerProgress++;
                OnActionProgress?.Invoke(PrayerProgress / MaxPrayerProgress, this, 0);
                if (PrayerProgress == MaxPrayerProgress)
                {
                    var provData = InventoryManager.Instance.GetProvision(Provision.ROSARY);
                    FPBonus += provData?.Value ?? 0;
                    var koboko = InventoryManager.Instance.GetProvision(Provision.KOBOKO);

                    if (koboko != null)
                    {
                        FPBonus += koboko?.Value ?? 0;
                        player.ConsumeEnergy(koboko.Value);
                    }
                    UpdateFaithPoints(PrayerPoints + FPBonus, 0);
                    PrayerProgress = 0;
                }
                clock.Tick();
            }
        }
        else if (clock.Time >= LiturgyStartTime && clock.Time < LiturgyEndTime)
        {
            LotHProgress++;
            OnActionProgress?.Invoke(LotHProgress/2f, this, 0);
            UI.Instance.DisplayMessage("ATTENDED LITURGY OF HOURS!!");
            SoundManager.Instance.PlayOneShotSfx("MassBells_SFX", timeToDie: 10f);
        //    player.ConsumeEnergy(ServiceEnergy);
            InteriorPopUI.PlayVFX("Halo");
            if (LotHProgress == 2)
            {
                var incense = InventoryManager.Instance.GetProvision(Provision.INCENSE);
                if(incense != null)
                {
                    if (Random.Range(0, 100) <= incense.Value)
                    {
                        FPBonus += 2;
                    }
                }
                UpdateFaithPoints(PrayerPoints +FPBonus, 0);
            }

            clock.Tick();
        }
        else
        {
        //    player.ConsumeEnergy(PrayEnergy);
            UI.Instance.DisplayMessage("PRAYED");
            PrayerProgress++;
            OnActionProgress?.Invoke(PrayerProgress / MaxPrayerProgress, this, 0);
            if(PrayerProgress == MaxPrayerProgress)
            {
                var provData = InventoryManager.Instance.GetProvision(Provision.ROSARY);
                FPBonus += provData?.Value ?? 0;
                var koboko = InventoryManager.Instance.GetProvision(Provision.KOBOKO);

                if (koboko != null)
                {
                    FPBonus += koboko?.Value ?? 0;
                    player.ConsumeEnergy(koboko.Value);
                }
                UpdateFaithPoints(PrayerPoints + FPBonus, 0);
                PrayerProgress = 0;
            }
            clock.Tick();
        }
    }

    public override void TriggerCustomEvent()
    {
        GameClock clock = GameManager.Instance.GameClock;
        if ((clock.Time+1) >= LiturgyStartTime && clock.Time < LiturgyEndTime) return;
        if (clock.Day % 5 == 0 && (clock.Time+0.5 == ConfessionTime || (clock.Time+0.5 >= MassStartTime && clock.Time < MassEndTime))) return;
        base.TriggerCustomEvent();
    }

    public override void ReportScores()
    {
        CurrentFaithPoints = 0;
    }

    public void Sleep()
    {
        GameClock clock = GameManager.Instance.GameClock;
        Player player = GameManager.Instance.Player;

        SleepProgress++;
        OnActionProgress?.Invoke(SleepProgress / MaxSleepProgress, this, 0);
        if (SleepProgress == MaxSleepProgress)
        {
            var mattress = InventoryManager.Instance.GetProvision(Provision.SOFT_MATTRESS);

            player.ConsumeEnergy(SleepEnergy - (mattress?.Value ?? 0));
            player.RemoveRandomStatusEffect();
            PopUIFXIcons("Energy", -SleepEnergy);
            SleepProgress = 0;
        }

        if(MaxSleepProgress > 0)
        {
            clock.Tick();
        }
        UI.Instance.DisplayMessage("SLEPT!");
    }

    public void OpenUnlockedSaints()
    {
        SaveDataManager.Instance.SaveGame();

        GameManager.Instance.FadeAndLoadScene("SaintsShowcase_Day");
    }

    private IEnumerator ResetActionProgressAsync()
    {
        yield return new WaitForSeconds(1.5f);
        ResetActionProgress();
        OnActionProgress?.Invoke(0, this, 0);
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
        return MassProgress == 0 && LotHProgress == 0 && ConfessionProgress == 0 && PrayerProgress == 0;
    }

    public override TooltipStats GetTooltipStatsForButton(string button)
    {
        switch (button)
        {
            case "PRAY":
                var fp = 0;
                GameClock clock = GameManager.Instance.GameClock;
                CustomEventData e = EventsManager.Instance.CurrentEvents.Find(i => i.Id == CustomEventType.WEEKDAY_MASS);

                if (clock.Day % 5 == 0 || e != null)
                {
                    if (clock.Time == ConfessionTime)
                    {
                       fp = PrayerPoints * 4;
                    }
                    else if (clock.Time >= MassStartTime && clock.Time < MassEndTime)
                    {
                        fp = PrayerPoints * 4;
                    }
                    else if (clock.Time >= LiturgyStartTime && clock.Time < LiturgyEndTime)
                    {
                        fp = PrayerPoints * 2;
                    }
                    else
                    {
                        fp = PrayerPoints;
                    }
                }
                else if (clock.Time >= LiturgyStartTime && clock.Time < LiturgyEndTime)
                {
                    fp = PrayerPoints * 2;
                }
                else
                {
                    fp = PrayerPoints;
                }

                return new TooltipStats() { Ticks = 1, FP = fp, CP = 0, Energy = 1 };
        }

        return base.GetTooltipStatsForButton(button);
    }

    public override float SetButtonTimer(string actionName)
    {
        switch (actionName)
        {
            case "PRAY":
                GameClock clock = GameManager.Instance.GameClock;
                CustomEventData e = EventsManager.Instance.CurrentEvents.Find(i => i.Id == CustomEventType.WEEKDAY_MASS);

                if (clock.Day % 5 == 0 || e != null)
                {
                    if (clock.Time == ConfessionTime)
                    {
                        return 2f;
                    }
                    else if (clock.Time >= MassStartTime && clock.Time < MassEndTime)
                    {
                        return 4f;
                    }
                    else if (clock.Time >= LiturgyStartTime && clock.Time < LiturgyEndTime)
                    {
                        return 2f;
                    }
                }
                else if (clock.Time >= LiturgyStartTime && clock.Time < LiturgyEndTime)
                {
                    return 2f;
                }
                break;
        }

        return base.SetButtonTimer(actionName);
    }

    public override void SetDeadlineTime(double time, int day)
    {

    }

    public override void OnDisable()
    {
        InventoryManager.RefreshInventoryUI -= CheckProvisions;
        base.OnDisable();
    }
}
