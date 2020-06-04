using System;
using UnityEngine;

public class InteractableMarket : InteractableHouse
{
    protected override void Start()
    {
        UI.BoughtItem += BoughtItem;
        PopUILocation = "UI/MarketUI";
        base.Start();
    }

    public override void OnPlayerMoved(Energy energy, MapTile tile)
    {
        base.OnPlayerMoved(energy, tile);
        GameClock clock = GameManager.Instance.GameClock;
        if (tile.GetInstanceID() == GetInstanceID())
        {
            if (clock.Time >= OpenTime && clock.Time < ClosingTime)
            {
            }
            else
            {
                UI.Instance.DisplayMessage("SHOP CLOSED!");
            }
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

    public void BoughtItem(ItemType item)
    {
        GameClock clock = GameManager.Instance.GameClock;
        if (clock.Time >= OpenTime && clock.Time < ClosingTime)
        {
            UI.Instance.DisplayMessage($"PICKED UP {item}!");
            GameManager.Instance.Player.AddToInventory(new PlayerItem(item));
        }
        else
        {
            UI.Instance.DisplayMessage("SHOP CLOSED!");
        }
    }

    public override void PopUICallback(string button)
    {
        if(button == "PRAY")
        {
            UI.Meditate?.Invoke(this);
            return;
        }

        ItemType itemType = (ItemType)Enum.Parse(typeof(ItemType), button);
        BoughtItem(itemType);
    }


    public override void ReportScores()
    {
        //Do absolutely nothing!
    }

    public override void SetDeadlineTime(double time, int day)
    {

    }

    public override void OnDisable()
    {
        UI.BoughtItem -= BoughtItem;
        base.OnDisable();
    }
}
