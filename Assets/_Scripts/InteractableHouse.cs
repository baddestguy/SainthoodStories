using UnityEngine;

public class InteractableHouse : InteractableObject
{
    public GameClock DeadlineTime;
    public int EnergyConsumption;
    public int OpenTime;
    public int ClosingTime;
    public int ItemDeliveryPoints;
    public int VolunteerPoints;
    public int DeadlineDeliveryBonus;
    public bool DeadlineSet;
    public int RequiredItems;    

    public MissionDifficulty MissionDifficulty;
    public static int DeadlineCounter;

    protected int CurrentCharityPoints;
    protected int CurrentFaithPoints;
    protected const int MeditationPoints = 1;
    public int NeglectedPoints;
    protected int NeglectedMultiplier = 1;

    protected virtual void Start()
    {
        UI.Meditate += Meditated;
        UI.DeliverItem += DeliverItem;
        UI.Volunteer += VolunteerWork;
        MissionManager.EndOfDay += ReportScores;
        MissionManager.EndOfDay += EndofDay;
    }

    public void Init(int deadline, MapTile groundTile, TileData tileData, Sprite[] sprites, int sortingOrder = 0)
    {
        Init(groundTile, tileData, sprites, sortingOrder);
    }

    public override void Tick(double time, int day)
    {
        if (DeadlineTime.Time != -1) Debug.LogWarning($"{name}: Deadline: {DeadlineTime.Time} : DAY {DeadlineTime.Day} : {RequiredItems} Items!!");
        SetDeadlineTime(time, day);

        if ((DeadlineTime.Time != -1) && (time >= DeadlineTime.Time && day >= DeadlineTime.Day))
        {
            Debug.LogError($"{name}: TIME UP!");
            NeglectedMultiplier++;
            DeadlineCounter--;
            DeadlineTime.SetClock(-1, day);
            DeadlineDeliveryBonus = 1;
            DeadlineSet = false;
        }
    }

    public virtual void SetDeadlineTime(double time, int day)
    {
        if (time < 6 || time >= 18) return;
        if ((DeadlineTime.Time != -1)) return;

        switch (MissionDifficulty)
        {
            case MissionDifficulty.EASY:
                if (DeadlineCounter < 1)
                {
                    if(Random.Range(0, 100) < 1)
                    {
                        DeadlineCounter++;
                        DeadlineTime.SetClock(time + RandomFutureTimeByDifficulty(), day);
                        RequiredItems = 1;
                        DeadlineDeliveryBonus = 4;
                        DeadlineSet = true;
                        Debug.LogWarning($"{name}: DEADLINE SET FOR {DeadlineTime.Time} : DAY  {DeadlineTime.Day} : {RequiredItems} Items!");
                    }
                }
                break;

            case MissionDifficulty.NORMAL:
                if (DeadlineCounter < 2)
                {
                    if (Random.Range(0, 100) < 1)
                    {
                        DeadlineCounter++;
                        DeadlineTime.SetClock(time + RandomFutureTimeByDifficulty(), day);
                        RequiredItems = Random.Range(1,3);
                        DeadlineDeliveryBonus = 3;
                        DeadlineSet = true;
                        Debug.LogWarning($"{name}: DEADLINE SET FOR {DeadlineTime.Time} : DAY  {DeadlineTime.Day} : {RequiredItems} Items!");
                    }
                }
                break;

            case MissionDifficulty.HARD:
                if (DeadlineCounter < 3)
                {
                    if (Random.Range(0, 100) < 2)
                    {
                        DeadlineCounter++;
                        DeadlineTime.SetClock(time + RandomFutureTimeByDifficulty(), day);
                        RequiredItems = Random.Range(1,4);
                        DeadlineDeliveryBonus = 2;
                        DeadlineSet = true;
                        Debug.LogWarning($"{name}: DEADLINE SET FOR {DeadlineTime.Time} : DAY  {DeadlineTime.Day} : {RequiredItems} Items!");
                    }
                }
                break;
        }
    }

    public double RandomFutureTimeByDifficulty()
    {
        switch (MissionDifficulty)
        {
            case MissionDifficulty.EASY: return Random.Range(6, 9);
            case MissionDifficulty.NORMAL: return Random.Range(5, 8);
            case MissionDifficulty.HARD: return Random.Range(4, 7);
        }

        return -1;
    }

    public virtual void EndofDay()
    {        
    }

    public override void MissionBegin(Mission mission)
    {
        MissionDifficulty = GameManager.MissionDifficulty;
        DeadlineTime = new GameClock(-1);
        DeadlineDeliveryBonus = 1;
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

    public virtual void DeliverItem(InteractableHouse house)
    {
        if (house != this) return;

        RequiredItems--;
        if (RequiredItems <= 0)
        {
            DeadlineCounter = Mathf.Max(0, DeadlineCounter - 1);
            DeadlineTime.SetClock(-1, DeadlineTime.Day);
            DeadlineDeliveryBonus = 1;
            DeadlineSet = false;
            RequiredItems = 0;
        }
    }

    public virtual void VolunteerWork(InteractableHouse house)
    {
    }

    public virtual void UpdateCharityPoints(int amount)
    {
        CurrentCharityPoints += amount;
    }

    public virtual void UpdateFaithPoints(int amount)
    {
        CurrentFaithPoints += amount;
        Debug.LogWarning("FAITH: " + CurrentFaithPoints);
    }

    public virtual void ReportScores()
    {
        GameManager.Instance.MissionManager.UpdateCharityPoints(CurrentCharityPoints > 0 ? CurrentCharityPoints : (NeglectedPoints * NeglectedMultiplier), this);
        GameManager.Instance.MissionManager.UpdateFaithPoints(CurrentFaithPoints);

        if (CurrentCharityPoints <= 0)
        {
            NeglectedMultiplier++;
        }
        else
        {
            NeglectedMultiplier = 1;
        }

        CurrentCharityPoints = 0;
        CurrentFaithPoints = 0;
    }

    public bool DuringOpenHours(GameClock newClock = null)
    {
        GameClock clock = newClock ?? GameManager.Instance.GameClock;
        return clock.Time >= OpenTime && clock.Time < ClosingTime;
    }

    public override void OnDisable()
    {
        UI.Meditate -= Meditated;
        UI.DeliverItem -= DeliverItem;
        UI.Volunteer -= VolunteerWork;
        MissionManager.EndOfDay -= EndofDay;
        MissionManager.EndOfDay -= ReportScores;
        base.OnDisable();
    }
}
