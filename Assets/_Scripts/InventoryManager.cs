using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }
    public static UnityAction RefreshInventoryUI;

    public List<ItemType> Items = new List<ItemType>();
    public List<Provision> Provisions = new List<Provision>();

    public int MaxInventorySlots = 2;
    public int MaxProvisionsSlots = 2;

    private void Awake()
    {
        Instance = this;
    }

    public void AddToInventory(ItemType item)
    {
        if (Items.Count == MaxInventorySlots)
        {
            UI.Instance.DisplayMessage("INVENTORY FULL!");
            return;
        }
        Items.Add(item);
        RefreshInventoryUI?.Invoke();
    }

    public void AddProvision(Provision provision)
    {
        if (Provisions.Count == MaxProvisionsSlots)
        {
            UI.Instance.DisplayMessage("Provisions FULL!");
            return;
        }
        Provisions.Add(provision);
        RefreshInventoryUI?.Invoke();
    }

    public ItemType GetItem(ItemType itemType)
    {
        int index = Items.FindIndex(i => i == itemType);
        if (index < 0) return ItemType.NONE;

        ItemType item = Items[index];
        Items.RemoveAt(index);

        RefreshInventoryUI?.Invoke();
        return item;
    }
}
