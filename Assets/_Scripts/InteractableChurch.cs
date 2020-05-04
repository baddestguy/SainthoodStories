public class InteractableChurch : InteractableHouse
{
    public int SleepEnergy;
    public int PrayEnergy;
    public int ServiceEnergy;

    public int PrayerPoints;

    protected override void Start()
    {
        UI.Prayed += Pray;
        UI.Slept += Sleep;
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

    public void Pray()
    {
        GameClock clock = GameManager.Instance.GameClock;
        Player player = GameManager.Instance.Player;

        if (clock.Time >= OpenTime && clock.Time < ClosingTime)
        {
            player.ConsumeEnergy(ServiceEnergy);
            clock.Tick();
            UI.Instance.DisplayMessage("ATTENDED LITURGY OF HOURS!!");
            UpdateFaithPoints(PrayerPoints*2);
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

    private void OnDisable()
    {
        UI.Prayed -= Pray;
        UI.Slept -= Sleep;
        UI.Meditate -= Meditated;
    }
}
