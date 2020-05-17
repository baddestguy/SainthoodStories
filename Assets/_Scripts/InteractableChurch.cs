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
        UI.Prayed += Pray;
        UI.Slept += Sleep;
        UpdateLiturgyTimes();
        base.Start();
    }

    public override void OnPlayerMoved(Energy energy, MapTile tile)
    {
        if (tile.GetInstanceID() == GetInstanceID())
        {
            UI.Instance.EnableChurch(true);
        }
        else
        {
            UI.Instance.EnableChurch(false);
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
        if (clock.Time > 21.5 || clock.Time < 6)
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
            LiturgyStartTime = 18;
            LiturgyEndTime = 19;
        }
        else if(clock.Time > 6.5)
        {
            LiturgyStartTime = 12;
            LiturgyEndTime = 13;
        }
    }

    public void Pray()
    {
        GameClock clock = GameManager.Instance.GameClock;
        Player player = GameManager.Instance.Player;

        if (clock.Day % 7 == 0)
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
        GameManager.Instance.MissionManager.UpdateFaithPoints(CurrentFaithPoints - NeglectedPoints);
        CurrentFaithPoints = 0;
    }

    public void Sleep()
    {
        GameClock clock = GameManager.Instance.GameClock;
        Player player = GameManager.Instance.Player;

        player.ConsumeEnergy(SleepEnergy);
        clock.Tick();
        UI.Instance.DisplayMessage("SLEPT!");
    }

    public override void OnDisable()
    {
        UI.Prayed -= Pray;
        UI.Slept -= Sleep;
        base.OnDisable();
    }
}
