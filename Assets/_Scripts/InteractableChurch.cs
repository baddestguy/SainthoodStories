public class InteractableChurch : InteractableHouse
{
    void Start()
    {
        UI.Prayed += Pray;
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

        if (clock.Time >= OpenTime && clock.Time <= ClosingTime)
        {
            player.ConsumeEnergy(-RestEnergy);
            clock.Tick();
            UI.Instance.DisplayMessage("ATTENDED SERVICE!!");
        }
        else
        {
            player.ConsumeEnergy(-1);
            clock.Tick();
            UI.Instance.DisplayMessage("PRAYED");
        }
    }

    public void EndDay()
    {
        //Fast Forward to Midnight
    }

    private void OnDisable()
    {
        UI.Prayed -= Pray;
    }
}
