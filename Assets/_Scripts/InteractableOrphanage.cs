using UnityEngine;

public class InteractableOrphanage : InteractableHouse
{
    public int AdoptionPoints;

    protected override void Start()
    {
        PopUILocation = "UI/OrphanageUI";
        base.Start();
    }

    public override void OnPlayerMoved(Energy energy, MapTile tile)
    {
        base.OnPlayerMoved(energy, tile);
        if (tile.GetInstanceID() == GetInstanceID())
        {
            PopUI.gameObject.SetActive(true);
            PopUI.Init(PopUICallback, GetType().Name, RequiredItems, DeadlineTime);
            PopIcon.UIPopped(true);
        }
        else
        {
            PopUI.gameObject.SetActive(false);
            PopIcon.UIPopped(false);
        }
    }
    public override void Tick(double time, int day)
    {
        base.Tick(time, day);

        if(day > 5)
        {
            OpenTime = 9;
            ClosingTime = 22;
        }
        else
        {
            OpenTime = 16;
            ClosingTime = 22;
        }
    }

    public override void DeliverItem(InteractableHouse house)
    {
        if (house != this) return;

        if (DuringOpenHours())
        {
            ItemType item = InventoryManager.Instance.GetItem(ItemType.TOYS);

            if (item != ItemType.NONE)
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
        if (BuildingState != BuildingState.NORMAL) return;

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
            CustomEventData e = EventsManager.Instance.CurrentEvents.Find(i => i.Id == EventType.ORPHANAGE_BONUS);
            player.ConsumeEnergy(EnergyConsumption);
            clock.Tick();
            UI.Instance.DisplayMessage("VOLUNTEERED AT ORPHANAGE!");
            UpdateCharityPoints(VolunteerPoints + (e != null ? (int)e.Gain : 0));
        }
        else
        {
            UI.Instance.DisplayMessage("ORPHANAGE CLOSED!");
        }
    }

    public override void PopUICallback(string button)
    {
        base.PopUICallback(button);

        switch (button)
        {
            case "TOYS":
                DeliverItem(this);
                break;

            case "VOLUNTEER":
                VolunteerWork(this);
                break;

            case "PRAY":
                UI.Meditate?.Invoke(this);
                break;
        }
    }

}
