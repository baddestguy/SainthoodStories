using UnityEngine;

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

        if (DuringOpenHours())
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

    public override void DeliverItem(InteractableHouse house)
    {
        if (house != this) return;
        if (!DuringOpenHours())
        {
            UI.Instance.DisplayMessage("SCHOOL CLOSED!");
            return;
        }

        Player player = GameManager.Instance.Player;
        PlayerItem item = player.GetItem(ItemType.STATIONERY);

        if (item != null)
        {
            UI.Instance.DisplayMessage("DELIVERED STATIONERY!");
            UpdateCharityPoints(ItemDeliveryPoints * DeadlineDeliveryBonus);
            base.DeliverItem(house);
        }
        else
        {
            UI.Instance.DisplayMessage("YOU HAVE NO STATIONERY TO GIVE!");
        }
    }

    public override void SetDeadlineTime(double time, int day)
    {
        if (time < OpenTime || time >= ClosingTime) return;
        if ((DeadlineTime.Time != -1)) return;

        double futureTime = time + RandomFutureTimeByDifficulty();
        if (futureTime > ClosingTime) return;

        switch (MissionDifficulty)
        {
            case MissionDifficulty.EASY:
                if (DeadlineCounter < 1)
                {
                    if (Random.Range(0, 100) < 1)
                    {
                        DeadlineCounter++;
                        DeadlineTime.SetClock(futureTime, day);
                        DeadlineDeliveryBonus = 4;
                        RequiredItems = 1;
                        DeadlineSet = true;
                        Debug.LogWarning($"{name}: DEADLINE SET FOR {DeadlineTime.Time} : {DeadlineTime.Day}!");
                    }
                }
                break;

            case MissionDifficulty.NORMAL:
                if (DeadlineCounter < 2)
                {
                    if (Random.Range(0, 100) < 3)
                    {
                        DeadlineCounter++;
                        DeadlineTime.SetClock(futureTime, day);
                        DeadlineDeliveryBonus = 3;
                        RequiredItems = Random.Range(1, 3);
                        DeadlineSet = true;
                        Debug.LogWarning($"{name}: DEADLINE SET FOR {DeadlineTime.Time} : {DeadlineTime.Day}!");
                    }
                }
                break;

            case MissionDifficulty.HARD:
                if (DeadlineCounter < 3)
                {
                    if (Random.Range(0, 100) < 5)
                    {
                        DeadlineCounter++;
                        DeadlineTime.SetClock(futureTime, day);
                        DeadlineDeliveryBonus = 2;
                        RequiredItems = Random.Range(1,4);
                        DeadlineSet = true;
                        Debug.LogWarning($"{name}: DEADLINE SET FOR {DeadlineTime.Time} : {DeadlineTime.Day}!");
                    }
                }
                break;
        }
    }

    public override void OnDisable()
    {
        UI.Taught -= Teach;
        base.OnDisable();
    }
}
