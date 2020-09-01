using UnityEngine;

public class InteractableChurch : InteractableHouse
{
    public int SleepEnergy;
    public int PrayEnergy;
    public int ServiceEnergy;

    public int PrayerPoints;

    private int LiturgyStartTime;
    private int LiturgyEndTime;
    public double ConfessionTime;
    public double MassStartTime;
    public double MassEndTime;

    protected override void Start()
    {
        PopUILocation = "UI/ChurchUI";
        base.Start();
        UI.Prayed += Pray;
        UI.Slept += Sleep;
        UpdateLiturgyTimes();
        PopIcon.transform.localPosition += new Vector3 (0, 0.5f, 0);
        PopUI.transform.localPosition += new Vector3(0, 1, 0);
        EnergyConsumption = ServiceEnergy;
    }

    public override void OnPlayerMoved(Energy energy, MapTile tile)
    {
        base.OnPlayerMoved(energy, tile);
        if (tile.GetInstanceID() == GetInstanceID())
        {
            PopUI.gameObject.SetActive(true);
            PopUI.Init(PopUICallback, GetType().Name, RequiredItems, DeadlineTime);
            PopIcon.UIPopped(true);
        }
        else
        {
            PopUI.gameObject.SetActive(false);
            PopIcon.UIPopped(false);
        }
    }

    public override void Tick(double time, int day)
    {
        UpdateLiturgyTimes();
        base.Tick(time, day);
    }

    private void UpdateLiturgyTimes()
    {
        GameClock clock = GameManager.Instance.GameClock;
        CustomEventData e = EventsManager.Instance.CurrentEvents.Find(i => i.Id == EventType.WEEKDAY_MASS);

        if (clock.Time > 21.5 || clock.Time <= 6.5)
        {
            LiturgyStartTime = 6;
            LiturgyEndTime = 7;
        }
        else if(clock.Time > 18.5)
        {
            LiturgyStartTime = 21;
            LiturgyEndTime = 22;
        }
        else if(clock.Time > 12.5)
        {
            if(clock.Day % 7 != 0 || e != null) //No 6pm liturgy of the hours on Sundays or Weekday Mass
            {
                LiturgyStartTime = 18;
                LiturgyEndTime = 19;
            }
        }
        else if(clock.Time > 6.5)
        {
            LiturgyStartTime = 12;
            LiturgyEndTime = 13;
        }
        if (clock.Time == LiturgyStartTime)
            SoundManager.Instance.PlayOneShotSfx("ChurchBells", 0.3f, 10f);

        PopMyIcon();
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
        }
    }

    public override void PopMyIcon(string name = "", int items = -1, GameClock time = null)
    {
        base.PopMyIcon(GetType().Name, RequiredItems, new GameClock(LiturgyStartTime, GameManager.Instance.GameClock.Day));

        GameClock clock = GameManager.Instance.GameClock;
        if (clock.Day % 7 == 0)
        {
            if (clock.Time > ConfessionTime && clock.Time < MassEndTime)
            {
                base.PopMyIcon(GetType().Name, RequiredItems, new GameClock(LiturgyStartTime, GameManager.Instance.GameClock.Day));
            }
            else if (clock.Time > 12.5 && clock.Time <= ConfessionTime)
            {
                base.PopMyIcon(GetType().Name, RequiredItems, new GameClock(LiturgyStartTime, GameManager.Instance.GameClock.Day));
            }
        }
    }

    public void Pray()
    {
        GameClock clock = GameManager.Instance.GameClock;
        Player player = GameManager.Instance.Player;
        CustomEventData e = EventsManager.Instance.CurrentEvents.Find(i => i.Id == EventType.WEEKDAY_MASS);

        if (clock.Day % 7 == 0 || e != null)
        {
            if (clock.Time == ConfessionTime)
            {
                player.ConsumeEnergy(ServiceEnergy);
                clock.Tick();
                UI.Instance.DisplayMessage("ATTENDED CONFESSION!!");
                UpdateFaithPoints(PrayerPoints * 4);
            }
            else if (clock.Time >= MassStartTime && clock.Time < MassEndTime)
            {
                player.ConsumeEnergy(ServiceEnergy);
                clock.Tick();
                UI.Instance.DisplayMessage("ATTENDED MASS!!");
                UpdateFaithPoints(PrayerPoints * 4);
            }
            else if (clock.Time >= LiturgyStartTime && clock.Time < LiturgyEndTime)
            {
                player.ConsumeEnergy(ServiceEnergy);
                clock.Tick();
                UI.Instance.DisplayMessage("ATTENDED LITURGY OF HOURS!!");
                UpdateFaithPoints(PrayerPoints * 2);
                SoundManager.Instance.PlayOneShotSfx("MassBells", 0.3f, 10f);
                player.ConsumeEnergy(ServiceEnergy);
            }
            else
            {
                player.ConsumeEnergy(PrayEnergy);
                clock.Tick();
                UI.Instance.DisplayMessage("PRAYED");
                UpdateFaithPoints(PrayerPoints);
            }
        }
        else if (clock.Time >= LiturgyStartTime && clock.Time < LiturgyEndTime)
        {
            clock.Tick();
            UI.Instance.DisplayMessage("ATTENDED LITURGY OF HOURS!!");
            SoundManager.Instance.PlayOneShotSfx("MassBells", 0.3f, 10f);
            UpdateFaithPoints(PrayerPoints * 2);
            player.ConsumeEnergy(ServiceEnergy);
        }
        else
        {
            player.ConsumeEnergy(PrayEnergy);
            clock.Tick();
            UI.Instance.DisplayMessage("PRAYED");
            UpdateFaithPoints(PrayerPoints);
        }
    }

    public override void ReportScores()
    {
        GameManager.Instance.MissionManager.UpdateFaithPoints(-NeglectedPoints);
        CurrentFaithPoints = 0;
    }

    public void Sleep()
    {
        GameClock clock = GameManager.Instance.GameClock;
        Player player = GameManager.Instance.Player;

        player.ConsumeEnergy(SleepEnergy);
        player.ModifyStatusEffect(PlayerStatusEffect.NONE);
        PopUIFXIcons("Energy", -SleepEnergy);
        clock.Tick();
        UI.Instance.DisplayMessage("SLEPT!");
    }

    public override void SetDeadlineTime(double time, int day)
    {

    }

    public override void OnDisable()
    {
        UI.Prayed -= Pray;
        UI.Slept -= Sleep;
        base.OnDisable();
    }
}
