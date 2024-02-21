using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PackageSelector : MonoBehaviour
{
    public PackageItem[] ItemList;
    public ScrollRect Scroller;

    private GameObject ItemGO;
    public InteractableHouse House;

    // Start is called before the first frame update
    void Start()
    {
        ItemGO = Resources.Load<GameObject>("UI/PackageUIItem");
        foreach (var house in GameManager.Instance.Houses)
        {
            if(house.MyObjective != null && house.MyObjective.Event == BuildingEventType.DELIVER_ITEM)
            {
                var item = Instantiate(ItemGO);
                item.transform.SetParent(Scroller.content);
                item.GetComponent<PackageItem>().Init(house.MyObjective);
            }
        }

        var items = InventoryManager.Instance.Items;

        for(int i = 0; i < items.Count; i++)
        {
            ItemList[i].PackageIcon.gameObject.SetActive(true);
            ItemList[i].PackageIcon.sprite = Resources.Load<Sprite>($"Icons/{items[i]}");
        }
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
    }

    public void PackageDeselected(PackageItem item)
    {
        for (int i = 0; i < ItemList.Length; i++)
        {
            if(item == ItemList[i])
            {
                if (!ItemList[i].PackageIcon.gameObject.activeSelf) return;

                ItemList[i].PackageIcon.gameObject.SetActive(false);
                break;
            }
        }
        InventoryManager.Instance.RemoveFromInventory(item.Item);
        var returnedItem = Instantiate(ItemGO);
        returnedItem.transform.SetParent(Scroller.content);
        returnedItem.GetComponent<PackageItem>().Init(item.Data);
    }

    public void Cancel()
    {
        UI.Instance.EnableAllUIElements(true);
        gameObject.SetActive(false);
    }

    public void GoToWorld()
    {
        if (House == null) return;

        House.GoToWorldMap();
    }

}
