using UnityEngine;

public class InteractableOrphanage : InteractableHouse
{
    public int AdoptionPoints;

    public override void OnPlayerMoved(Energy energy, MapTile tile)
    {
        if (tile.GetInstanceID() == GetInstanceID())
        {
            UI.Instance.EnableOrphanage(true, this);
        }
        else
        {
            UI.Instance.EnableOrphanage(false, this);
        }
    }

    public override void DeliverItem(InteractableHouse house)
    {
        if (house != this) return;

        if (DuringOpenHours())
        {
            Player player = GameManager.Instance.Player;
            PlayerItem item = player.GetItem(ItemType.TOYS);

            if (item != null)
            {
                UI.Instance.DisplayMessage("GAVE TOYS TO THE KIDS!");
                UpdateCharityPoints(ItemDeliveryPoints * DeadlineDeliveryBonus);
                base.DeliverItem(house);
            }
            else
            {
                UI.Instance.DisplayMessage("YOU HAVE NO TOYS TO GIVE!");
            }
        }
        else
        {
            UI.Instance.DisplayMessage("ORPHANAGE CLOSED!");
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
                        PopMyIcon();
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
                        PopMyIcon();
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
                        PopMyIcon();
                        Debug.LogWarning($"{name}: DEADLINE SET FOR {DeadlineTime.Time} : {DeadlineTime.Day}!");
                    }
                }
                break;
        }
    }

    public override void VolunteerWork(InteractableHouse house)
    {
        if (house != this) return;

        GameClock clock = GameManager.Instance.GameClock;
        Player player = GameManager.Instance.Player;
        if (player.EnergyDepleted()) return;

        if (DuringOpenHours())
        {
            player.ConsumeEnergy(EnergyConsumption);
            clock.Tick();
            UI.Instance.DisplayMessage("VOLUNTEERED AT ORPHANAGE!");
            UpdateCharityPoints(VolunteerPoints);
        }
        else
        {
            UI.Instance.DisplayMessage("ORPHANAGE CLOSED!");
        }
    }
}
