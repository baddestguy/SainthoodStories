using System;
using System.Linq;
using UnityEngine;

public class InteractableMarket : InteractableHouse
{
    protected override void Start()
    {
        UI.BoughtItem += BoughtItem;
        PopUILocation = "UI/MarketUI";
        base.Start();
        BuildPoints = 4;
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
            PopUI.Init(PopUICallback, GetType().Name, RequiredItems, DeadlineTime, this);
            PopIcon.UIPopped(true);
            UI.Instance.EnableTreasuryUI(true);
            UI.Instance.RefreshTreasuryBalance();
        }
        else
        {
            PopUI.gameObject.SetActive(false);
            PopIcon.UIPopped(false);
            UI.Instance.EnableTreasuryUI(false);
        }
    }

    public void BoughtItem(ItemType item)
    {
        GameClock clock = GameManager.Instance.GameClock;
        if (clock.Time >= OpenTime && clock.Time < ClosingTime)
        {
            UI.Instance.DisplayMessage($"PICKED UP {item}!");
            var itemData = GameDataManager.Instance.ShopItemData[item];
            var moddedPrice = ApplyDiscount(itemData.Price);

            if (!TreasuryManager.Instance.CanAfford(moddedPrice))
            {
                UI.Instance.DisplayMessage("Not Enough Money!");
                return;
            }
            TreasuryManager.Instance.SpendMoney(moddedPrice);
            InventoryManager.Instance.AddToInventory(item);
            PopUI.Init(PopUICallback, GetType().Name, RequiredItems, DeadlineTime, this);
            UI.Instance.RefreshTreasuryBalance();
        }
        else
        {
            UI.Instance.DisplayMessage("SHOP CLOSED!");
        }
    }

    public override void PopUICallback(string button)
    {
        base.PopUICallback(button);

        if (button == "PRAY")
        {
            UI.Meditate?.Invoke(this);
            return;
        }

        ItemType itemType = (ItemType)Enum.Parse(typeof(ItemType), button);
        BoughtItem(itemType);
    }

    public static double ApplyDiscount(double price)
    {
        double newPrice = price;
        var e = EventsManager.Instance.CurrentEvents.Find(i => i.Id == CustomEventType.MARKET_DISCOUNT || i.Id == CustomEventType.MARKET_MARKUP);

        if(e != null)
        {
            if(e.Id == CustomEventType.MARKET_DISCOUNT)
            {
                newPrice = newPrice - (e.Cost / 100 * newPrice);
            }
            else if(e.Id == CustomEventType.MARKET_MARKUP)
            {
                newPrice = newPrice + (e.Cost / 100 * newPrice);
            }
        }

        if(InventoryManager.Instance.HasProvision(Provision.DISCOUNT_CARD))
        {
            newPrice = newPrice - (20d / 100d * newPrice);
        }

        return (int)newPrice;
    }

    protected override void OnEventExecuted(CustomEventData e)
    {
        switch (e.Id)
        {
            case CustomEventType.MARKET_HOURS:
                if(e.Cost == 18)
                {
                    ClosingTime = 23;
                }
                else if(e.Cost == 24)
                {
                    OpenTime = 0;
                    ClosingTime = 23;
                }
                break;
        }
    }

    public override bool CanDoAction(string actionName)
    {
        switch (actionName)
        {
            case "PRAY":
                return base.CanDoAction(actionName);
        }

        ItemType itemType = (ItemType)Enum.Parse(typeof(ItemType), actionName);
        var itemData = GameDataManager.Instance.ShopItemData[itemType];
        var moddedPrice = ApplyDiscount(itemData.Price);
        bool isHouseAvailable = false;

        switch (itemType)
        {
            case ItemType.CLOTHES:
                isHouseAvailable = FindObjectOfType<InteractableClothesBank>()?.BuildingState != BuildingState.RUBBLE;
                break;
            case ItemType.GROCERIES:
                isHouseAvailable = FindObjectOfType<InteractableShelter>()?.BuildingState != BuildingState.RUBBLE;
                break;
            case ItemType.MEDS:
                isHouseAvailable = FindObjectOfType<InteractableHospital>()?.BuildingState != BuildingState.RUBBLE;
                break;
            case ItemType.STATIONERY:
                isHouseAvailable = FindObjectOfType<InteractableSchool>()?.BuildingState != BuildingState.RUBBLE;
                break;
            case ItemType.TOYS:
                isHouseAvailable = FindObjectOfType<InteractableOrphanage>()?.BuildingState != BuildingState.RUBBLE;
                break;
        }

        return DuringOpenHours() && TreasuryManager.Instance.CanAfford(moddedPrice) && isHouseAvailable;
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
