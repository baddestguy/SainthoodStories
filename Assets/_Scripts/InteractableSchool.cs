public class InteractableSchool : InteractableHouse
{
    public int TeachPoints;

    protected override void Start()
    {
        UI.Taught += Teach;
        base.Start();
    }

    public override void OnPlayerMoved(Energy energy, MapTile tile)
    {
        GameClock clock = GameManager.Instance.GameClock;
        if (tile.GetInstanceID() == GetInstanceID())
        {
            if (clock.Time >= OpenTime && clock.Time < ClosingTime)
            {
            }
            else
            {
                UI.Instance.DisplayMessage("SCHOOL CLOSED!");
            }
            UI.Instance.EnableSchool(true, this);
        }
        else
        {
            UI.Instance.EnableSchool(false, this);
        }
    }

    public void Teach()
    {
        GameClock clock = GameManager.Instance.GameClock;
        Player player = GameManager.Instance.Player;
        if (player.EnergyDepleted()) return;

        if (clock.Time >= OpenTime && clock.Time < ClosingTime)
        {
            player.ConsumeEnergy(EnergyConsumption);
            clock.Tick();
            UI.Instance.DisplayMessage("Taught a Class!!");
            UpdateCharityPoints(TeachPoints);
        }
        else
        {
            UI.Instance.DisplayMessage("SCHOOL CLOSED!");
        }
    }

    public override void OnDisable()
    {
        UI.Taught -= Teach;
        base.OnDisable();
    }
}
