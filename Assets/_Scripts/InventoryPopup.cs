using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryPopup : MonoBehaviour
{
    public PackageItem[] ItemList;
    public ProvisionUIItem[] UpgradeProvisionUIItems;
    public ScrollRect Scroller;
    public HouseObjectiveUIItem[] Objs;


    // Start is called before the first frame update
    void OnEnable()
    {
        Player.LockMovement = true;
        for (int i = 0; i < InventoryManager.Instance.Items.Count; i++)
        {
            ItemList[i].PackageIcon.gameObject.SetActive(true);
            ItemList[i].PackageIcon.sprite = Resources.Load<Sprite>($"Icons/{InventoryManager.Instance.Items[i]}");
        }

        for (int i = 0; i < InventoryManager.Instance.Provisions.Count; i++)
        {
            UpgradeProvisionUIItems[i].Init(InventoryManager.Instance.Provisions[i], ProvisionUIItemType.UPGRADE);
        }

        var houses = GameManager.Instance.Houses;
        
        //Sort to always place convent first
        for (int i = 0; i < houses.Length; i++)
        {
            if (houses[i].HouseName.Contains("Church"))
            {
                var temp = houses[0];
                houses[0] = houses[i];
                houses[i] = temp;
            }
        }

        for (int i = 0; i < houses.Length; i++)
        {
            if (houses[i].MyObjective == null) continue;

            Objs[i].gameObject.SetActive(true);
            Objs[i].Text.text = $"{LocalizationManager.Instance.GetText(houses[i].MyObjective.MissionDescription)}";
            Objs[i].Image.sprite = Resources.Load<Sprite>($"Icons/{houses[i].HouseName}");

            if (houses[i].MyObjective.Event == BuildingEventType.COOK || houses[i].MyObjective.Event == BuildingEventType.COOK_URGENT
                || houses[i].MyObjective.Event == BuildingEventType.DELIVER_ITEM 
                || houses[i].MyObjective.Event == BuildingEventType.DELIVER_ITEM_URGENT
                || houses[i].MyObjective.Event == BuildingEventType.DELIVER_MEAL
                || houses[i].MyObjective.Event == BuildingEventType.DELIVER_MEAL_URGENT)
            {
                Objs[i].Text.text = $"{LocalizationManager.Instance.GetText(houses[i].MyObjective.MissionDescription)}: {houses[i].MyObjective.RequiredAmount - houses[i].RequiredItems}/{houses[i].MyObjective.RequiredAmount}";
            }

            if (houses[i].MyObjective.Event == BuildingEventType.VOLUNTEER 
                || houses[i].MyObjective.Event == BuildingEventType.VOLUNTEER_URGENT
                || houses[i].MyObjective.Event == BuildingEventType.PRAY 
                || houses[i].MyObjective.Event == BuildingEventType.PRAY_URGENT)
            {
                Objs[i].Text.text = $"{LocalizationManager.Instance.GetText(houses[i].MyObjective.MissionDescription)}: {houses[i].VolunteerProgress}/{houses[i].MyObjective.RequiredAmount}";
            }
        }
    }

    private void OnDisable()
    {
        for (int i = 0; i < InventoryManager.Instance.Items.Count; i++)
        {
            ItemList[i].PackageIcon.gameObject.SetActive(false);
        }
        for (int i = 0; i < Objs.Length; i++)
        {
            Objs[i].gameObject.SetActive(false);
        }

        Player.LockMovement = false;
    }
}
