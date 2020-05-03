public class InteractableChurch : InteractableHouse
{
    public int SleepEnergy;
    public int MeditateEnergy;
    public int PrayEnergy;
    public int ServiceEnergy;

    void Start()
    {
        UI.Prayed += Pray;
        UI.Slept += Sleep;
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
            UI.Instance.DisplayMessage("ATTENDED SERVICE!!");
        }
        else
        {
            player.ConsumeEnergy(PrayEnergy);
            clock.Tick();
            UI.Instance.DisplayMessage("PRAYED");
        }
    }

    public void Sleep()
    {
        GameClock clock = GameManager.Instance.GameClock;
        Player player = GameManager.Instance.Player;

        player.ConsumeEnergy(SleepEnergy);
        clock.Tick();
        UI.Instance.DisplayMessage("SLEPT!");
    }

    public void Meditate()
    {
        GameClock clock = GameManager.Instance.GameClock;
        Player player = GameManager.Instance.Player;

        player.ConsumeEnergy(MeditateEnergy);
        clock.Tick();
        UI.Instance.DisplayMessage("MEDITATE!");
    }

    private void OnDisable()
    {
        UI.Prayed -= Pray;
        UI.Slept -= Sleep;
    }
}
