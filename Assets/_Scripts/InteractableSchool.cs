public class InteractableSchool : InteractableHouse
{
    void Start()
    {
        UI.Taught += Teach;
    }

    public override void OnPlayerMoved(Energy energy, MapTile tile)
    {
        GameClock clock = GameManager.Instance.GameClock;
        if (tile.GetInstanceID() == GetInstanceID())
        {
            if (clock.Time >= OpenTime && clock.Time < ClosingTime)
            {
                UI.Instance.EnableSchool(true);
            }
            else
            {
                UI.Instance.DisplayMessage("SCHOOL CLOSED!");
            }
        }
        else
        {
            UI.Instance.EnableSchool(false);
        }
    }

    public void Teach()
    {
        GameClock clock = GameManager.Instance.GameClock;
        Player player = GameManager.Instance.Player;

        if (clock.Time >= OpenTime && clock.Time < ClosingTime)
        {
            player.ConsumeEnergy(EnergyConsumption);
            clock.Tick();
            UI.Instance.DisplayMessage("Taught a Class!!");
        }

        if (clock.Time < OpenTime || clock.Time >= ClosingTime)
        {
            UI.Instance.EnableSchool(false);
        }
    }

    private void OnDisable()
    {
        UI.Taught -= Teach;
    }
}
