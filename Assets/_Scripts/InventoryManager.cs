using System.Collections;
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
    public int MaxProvisionsSlots = 1;

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

        switch (provision)
        {
            case Provision.EXTRA_INVENTORY:
                MaxInventorySlots = 4;
                break;

            case Provision.ENERGY_DRINK:
                Player player = GameManager.Instance.Player;
                player.ConsumeEnergy(-30);
                break;
        }

        RefreshInventoryUI?.Invoke();
    }

    public void ClearProvisions()
    {
        Provisions.Clear();
        MaxInventorySlots = 2;
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

    public void GenerateProvisionsForNewDay()
    {
        StartCoroutine(WaitAndEnableProvisionPopupAsync());
    }

    IEnumerator WaitAndEnableProvisionPopupAsync()
    {
        while (EventsManager.Instance.HasEventsInQueue())
        {
            yield return null;
        }

        ClearProvisions();

        if (Random.Range(0, 100) < 50) yield break;

        var prov1 = GameDataManager.Instance.ProvisionData[(Provision)Random.Range(0, 7)];
        var prov2 = GameDataManager.Instance.ProvisionData[(Provision)Random.Range(0, 7)];
        while (prov2.Id == prov1.Id)
        {
            prov2 = GameDataManager.Instance.ProvisionData[(Provision)Random.Range(0, 7)];
        }

        UI.Instance.EnableProvisionPopup(prov1, prov2);
    }

    public bool HasProvision(Provision provision)
    {
        return Provisions.Contains(provision);
    }
}
