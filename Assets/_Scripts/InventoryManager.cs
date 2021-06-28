using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        GameClock.StartNewDay += GenerateProvisionsForNewDay;
    }

    public void LoadInventory(SaveObject save)
    {
        Items = save.InventoryItems?.ToList() ?? new List<ItemType>();
        Provisions = save.Provisions?.ToList() ?? new List<Provision>();
        if (Provisions.Contains(Provision.EXTRA_INVENTORY)){
            MaxInventorySlots = 4;
            RefreshInventoryUI?.Invoke();
        }
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

    public bool CheckItem(ItemType itemType)
    {
        int index = Items.FindIndex(i => i == itemType);
        return index >= 0;
    }

    public void GenerateProvisionsForNewDay()
    {
        if (!GameSettings.Instance.ProvisionsToggle) return;
        GameClock c = GameManager.Instance.GameClock;

        if (c.EndofWeek()) return;
        if (GameManager.Instance.MissionManager.CurrentMission.CurrentWeek == 1 && c.Day < 5) return;

        StartCoroutine(WaitAndEnableProvisionPopupAsync());
    }

    IEnumerator WaitAndEnableProvisionPopupAsync()
    {
        yield return null;

        while (EventsManager.Instance.HasEventsInQueue())
        {
            yield return null;
        }

        ClearProvisions();

        GameClock c = GameManager.Instance.GameClock;
        if (GameManager.Instance.MissionManager.CurrentMission.CurrentWeek == 1 && c.Day > 5 && Random.Range(0, 100) < 50) yield break;

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

    private void OnDisable()
    {
        GameClock.StartNewDay -= GenerateProvisionsForNewDay;
    }
}
