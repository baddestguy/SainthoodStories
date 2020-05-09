using UnityEngine;

public class InteractableHouse : InteractableObject
{
    private int CountdownTimer;
    public int DeadlineTime;
    public int EnergyConsumption;
    public int OpenTime;
    public int ClosingTime;

    protected int CurrentTownPoints;
    protected int CurrentFaithPoints;
    protected const int MeditationPoints = 1;
    public int NeglectedPoints;
    protected int NeglectedMultiplier = 1;

    protected virtual void Start()
    {
        UI.Meditate += Meditated;
        MissionManager.EndOfDay += ReportScores;
        Player.OnReset += OnReset;
    }

    public void Init(int deadline, MapTile groundTile, TileData tileData, Sprite[] sprites, int sortingOrder = 0)
    {
        DeadlineTime = deadline;        
        Init(groundTile, tileData, sprites, sortingOrder);
    }

    public override void Tick(double time, int day)
    {
        CountdownTimer--;
        if ((DeadlineTime != -1) && (CountdownTimer <= 0 || time >= DeadlineTime))
        {
            UI.Instance.DisplayMessage($"{name}: TIME UP!");
        }
    }

    public override void MissionBegin(Mission mission)
    {
        CountdownTimer = DeadlineTime - mission.StartingClock;
    }

    public virtual void Meditated(InteractableHouse house)
    {
        if (house != this) return;

        GameClock clock = GameManager.Instance.GameClock;
        Player player = GameManager.Instance.Player;

        player.ConsumeEnergy(-1);
        clock.Tick();
        UI.Instance.DisplayMessage("MEDITATED!!");
        UpdateFaithPoints(MeditationPoints);
    }

    public virtual void UpdateTownPoints(int amount)
    {
        CurrentTownPoints += amount;
    }

    public virtual void UpdateFaithPoints(int amount)
    {
        CurrentFaithPoints += amount;
        Debug.LogWarning("FAITH: " + CurrentFaithPoints);
    }

    public virtual void ReportScores()
    {
        GameManager.Instance.MissionManager.UpdateTownPoints(CurrentTownPoints > 0 ? CurrentTownPoints : (NeglectedPoints * NeglectedMultiplier), this);
        GameManager.Instance.MissionManager.UpdateFaithPoints(CurrentFaithPoints);

        if (CurrentTownPoints <= 0)
        {
            NeglectedMultiplier++;
        }
        else
        {
            NeglectedMultiplier = 1;
        }

        CurrentTownPoints = 0;
        CurrentFaithPoints = 0;
    }

    public virtual void OnReset()
    {

    }

    private void OnDisable()
    {
        UI.Meditate -= Meditated;
        MissionManager.EndOfDay -= ReportScores;
    }
}
