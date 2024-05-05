using System.Collections.Generic;
using System.Linq;
using Assets._Scripts.Extensions;
using Assets.Xbox;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PackageSelector : MonoBehaviour
{
    public PackageItem[] ItemList;
    public ScrollRect Scroller;
    public InteractableHouse House;

    public List<PackageItem> AvailableItems { get; set; }
    public GameObject ExitGameObject { get; set; }

    private GameObject ItemGO;

    // Start is called before the first frame update
    void Start()
    {
        ItemGO = Resources.Load<GameObject>("UI/PackageUIItem");
        var instantiatedGos = new List<PackageItem>();
        foreach (var house in GameManager.Instance.Houses)
        {
            if (house.MyObjective != null && (house.MyObjective.Event == BuildingEventType.DELIVER_ITEM || house.MyObjective.Event == BuildingEventType.DELIVER_ITEM_URGENT))
            {
                for (int i = 0; i < house.MyObjective.RequiredAmount; i++)
                {
                    var item = Instantiate(ItemGO);
                    item.transform.SetParent(Scroller.content);
                    var pItem = item.GetComponent<PackageItem>();
                    pItem.Init(house.MyObjective);
                    instantiatedGos.Add(pItem);
                }
            }
        }

        var items = InventoryManager.Instance.Items;

        for (int i = 0; i < items.Count; i++)
        {
            PackageSelected(instantiatedGos.FirstOrDefault(go => go.Item == items[i]));
        }

        AvailableItems = instantiatedGos.ToList();
        ExitGameObject = gameObject.FindDeepChild("Exit");
        GameplayControllerHandler.Instance.SetPackageSelector(this);
    }

    public void PackageSelected(PackageItem item)
    {
        for (int i = 0; i < ItemList.Length; i++)
        {
            if (!ItemList[i].PackageIcon.gameObject.activeSelf)
            {
                ItemList[i].PackageIcon.gameObject.SetActive(true);
                ItemList[i].Init(item.Data);
                break;
            }
        }
        InventoryManager.Instance.AddToInventory(item.Item);
        Destroy(item.gameObject);
        AvailableItems.Remove(item);
    }

    public void PackageDeselected(PackageItem item)
    {
        for (int i = 0; i < ItemList.Length; i++)
        {
            if (item == ItemList[i])
            {
                if (!ItemList[i].PackageIcon.gameObject.activeSelf) return;

                ItemList[i].PackageIcon.gameObject.SetActive(false);
                var toolTip = ItemList[i].GetComponent<TooltipMouseOver>();
                toolTip.Loc_Key = string.Empty;
                break;
            }
        }
        InventoryManager.Instance.RemoveFromInventory(item.Item);
        var returnedItem = Instantiate(ItemGO);
        returnedItem.transform.SetParent(Scroller.content);
        var packageItem = returnedItem.GetComponent<PackageItem>();
        packageItem.Init(item.Data);
        AvailableItems.Add(packageItem);
    }

    public void Cancel()
    {
        TextMeshProUGUI provisionDescription = GameObject.Find("ProvisionDescription")?.GetComponent<TextMeshProUGUI>();
        provisionDescription.text = "";
        UI.Instance.EnableAllUIElements(true);
        gameObject.SetActive(false);
    }

    public void GoToWorld()
    {
        gameObject.SetActive(false);
        if (!InventoryManager.HasChosenProvision)
            InventoryManager.Instance.GenerateProvisionsForNewDay();
        else
        {
            if (House == null) return;

            House.GoToWorldMap();
        }
    }
}
