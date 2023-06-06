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
    public List<ProvisionData> Provisions = new List<ProvisionData>();
    public Dictionary<ItemType, List<GameClock>> AutoDeliveryItems = new Dictionary<ItemType, List<GameClock>>();

    public int MaxInventorySlots = 2;
    public int MaxProvisionsSlots = 5;

    public List<ProvisionData> GeneratedProvisions = new List<ProvisionData>();

    private void Awake()
    {
        Instance = this;
        GameClock.StartNewDay += GenerateProvisionsForNewDay;
    }

    public void LoadInventory(SaveObject save)
    {
        Items = save.InventoryItems?.ToList() ?? new List<ItemType>();
        Provisions = save.Provisions?.ToList() ?? new List<ProvisionData>();
        if (HasProvision(Provision.EXTRA_INVENTORY)){
            MaxInventorySlots = GetProvision(Provision.EXTRA_INVENTORY).Value;
        }
        RefreshInventoryUI?.Invoke();
    }

    public bool IsInventoryFull()
    {
        var autodelivery = GetProvision(Provision.AUTO_DELIVER);
        return (Items.Count >= MaxInventorySlots && autodelivery == null) || (Items.Count >= MaxInventorySlots && autodelivery != null && AutoDeliveryItems.Sum(x => x.Value.Count) == autodelivery.Value);
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
        SaveDataManager.Instance.SaveGame();
    }

    public void SwapProvision(ProvisionData provisionFrom, ProvisionData provisionTo)
    {

        
    }

    public void AddProvision(ProvisionData provision)
    {
        if (Provisions.Count == MaxProvisionsSlots)
        {
            UI.Instance.DisplayMessage("Provisions FULL!");
            return;
        }
        Provisions.Add(provision);

        switch (provision.Id)
        {
            case Provision.EXTRA_INVENTORY:
                MaxInventorySlots = provision.Value;
                break;

            case Provision.ENERGY_DRINK:
                Player player = GameManager.Instance.Player;
                player.ConsumeEnergy(-provision.Value);
                break;
        }

        RefreshInventoryUI?.Invoke();
        SaveDataManager.Instance.SaveGame();
    }

    public void UpgradeProvision(ProvisionData currProvision)
    {
        if (HasProvision(currProvision.Id))
        {
            var prov = Provisions.SingleOrDefault(p => p.Id == currProvision.Id);
            if (prov == null) return;

            var upgradedProv = GameDataManager.Instance.GetProvision(prov.Id, prov.Level + 1);
            if(upgradedProv != null)
            {
                Provisions.Remove(prov);
                AddProvision(upgradedProv);
            }
        }
        RefreshInventoryUI?.Invoke();
    }

    public void RemoveProvision(Provision Id)
    {
        var prov = Provisions.Where(p => p.Id == Id).FirstOrDefault();
        if (prov == null) return;

        Provisions.Remove(prov);
    }

    //We will never call this during a run as Provisions are now permanent
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
        if (GameSettings.Instance.FTUE && GameManager.Instance.MissionManager.CurrentMission.CurrentWeek == 1 && c.Day < 2) return;

        StartCoroutine(WaitAndEnableProvisionPopupAsync());
    }

    IEnumerator WaitAndEnableProvisionPopupAsync()
    {
        yield return null;

        while (EventsManager.Instance.HasEventsInQueue())
        {
            yield return null;
        }

        //ClearProvisions();

        GameClock c = GameManager.Instance.GameClock;
        if (GameManager.Instance.MissionManager.CurrentMission.CurrentWeek == 1 && c.Day > 5 && Random.Range(0, 100) < 50) yield break;

        //Check to make sure that we dont already have the Provision in our Inventory
        var prov1 = GameDataManager.Instance.ProvisionData[(Provision)Random.Range(0, (int)Provision.MAX_COUNT)][0];
        prov1 = SwapProvisionBySeason(prov1);
        while (Provisions.Any(p => p.Id == prov1.Id))
        {
            prov1 = GameDataManager.Instance.ProvisionData[(Provision)Random.Range(0, (int)Provision.MAX_COUNT)][0];
            prov1 = SwapProvisionBySeason(prov1);
        }

        var prov2 = GameDataManager.Instance.ProvisionData[(Provision)Random.Range(0, (int)Provision.MAX_COUNT)][0];
        prov2 = SwapProvisionBySeason(prov2);

        while (Provisions.Any(p => p.Id == prov2.Id) || prov2.Id == prov1.Id)
        {
            prov2 = GameDataManager.Instance.ProvisionData[(Provision)Random.Range(0, (int)Provision.MAX_COUNT)][0];
            prov2 = SwapProvisionBySeason(prov2);
        }

        GeneratedProvisions = GameManager.Instance.SaveData.GeneratedProvisions?.ToList();
        if (GeneratedProvisions != null && GeneratedProvisions.Any())
        {
            UI.Instance.EnableProvisionPopup(GeneratedProvisions[0], GeneratedProvisions[1]);
        }
        else
        {
            GeneratedProvisions = new List<ProvisionData>();
            GeneratedProvisions.Add(prov1);
            GeneratedProvisions.Add(prov2);
            UI.Instance.EnableProvisionPopup(prov1, prov2);
        }
        SaveDataManager.Instance.SaveGame();
    }

    private ProvisionData SwapProvisionBySeason(ProvisionData prov)
    {
        switch (MissionManager.Instance.CurrentMission.Season)
        {
            case Season.SUMMER:
                if (prov.Id == Provision.WINTER_CLOAK || prov.Id == Provision.UMBRELLA)
                {
                    return GameDataManager.Instance.ProvisionData[Provision.SHADES][0];
                }
                break;

            case Season.FALL:
                if(prov.Id == Provision.WINTER_CLOAK || prov.Id == Provision.SHADES)
                {
                    return GameDataManager.Instance.ProvisionData[Provision.UMBRELLA][0];
                }
                break;

            case Season.WINTER:
                if (prov.Id == Provision.UMBRELLA || prov.Id == Provision.SHADES)
                {
                    return GameDataManager.Instance.ProvisionData[Provision.WINTER_CLOAK][0];
                }
                break;
        }
        return prov;
    }

    public bool HasProvision(Provision provision)
    {
        return Provisions.Any(p => p.Id == provision);
    }

    public void OnInventoryOverride(bool add, ItemType itemType)
    {
        if (itemType == ItemType.NONE) return;
        if (add)
            AddToInventory(itemType);
        else
            GetItem(itemType);
        
    }

    public ProvisionData GetProvision(Provision provId)
    {
        return Provisions.Where(p => p.Id == provId).FirstOrDefault();
    }

    public void OnProvisionsOverride(Provision provision)
    {
        AddProvision(GameDataManager.Instance.GetProvision(provision, 1));
    }

    private void OnDisable()
    {
        GameClock.StartNewDay -= GenerateProvisionsForNewDay;
    }
}
