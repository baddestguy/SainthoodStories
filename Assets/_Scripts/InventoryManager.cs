using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets._Scripts.Xbox;
using UnityEngine;
using UnityEngine.Events;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }
    public static UnityAction RefreshInventoryUI;

    public List<ItemType> Items = new List<ItemType>();
    public List<string> Collectibles = new List<string>();
    public List<ProvisionData> Provisions = new List<ProvisionData>();
    public Dictionary<ItemType, List<GameClock>> AutoDeliveryItems = new Dictionary<ItemType, List<GameClock>>();

    public int MaxInventorySlots = 2;
    public int MaxProvisionsSlots = 5;

    public List<ProvisionData> GeneratedProvisions = new List<ProvisionData>();
    public static bool HasChosenProvision;

    public List<WorldTriviaData> WorldTrivia = new List<WorldTriviaData>();
    public List<SaintWritingData> SaintWritings = new List<SaintWritingData>();
    public Dictionary<SaintID, List<SaintFragmentData>> SaintFragments = new Dictionary<SaintID, List<SaintFragmentData>>();
    public List<LetterData> Letters = new List<LetterData>();

    private void Awake()
    {
        Instance = this;
    //    GameClock.StartNewDay += GenerateProvisionsForNewDay;
    }

    public void LoadInventory(SaveObject save)
    {
        Items = save.InventoryItems?.ToList() ?? new List<ItemType>();
        Provisions = save.Provisions?.ToList() ?? new List<ProvisionData>();
        HasChosenProvision = save.HasChosenProvision;
        if (HasProvision(Provision.EXTRA_INVENTORY))
        {
            MaxInventorySlots = GetProvision(Provision.EXTRA_INVENTORY).Value;
        }
        Collectibles = save.Collectibles?.ToList() ?? new List<string>();
        RefreshInventoryUI?.Invoke();
        UI.Instance.RefreshWanderingSpiritsBalance(0);
    }

    public void AddCollectibleType(CollectibleType collectible)
    {
        switch (collectible)
        {
            case CollectibleType.WORLD_TRIVIA:
                var trivia = GameDataManager.Instance.GetRandomWorldTrivia();
                WorldTrivia.Add(trivia);
                break;

            case CollectibleType.SAINT_WRITING:
                var writing = GameDataManager.Instance.GetRandomSaintWriting();
                SaintWritings.Add(writing);
                break;

            case CollectibleType.SAINT_FRAGMENT:
                var fragment = GameDataManager.Instance.GetRandomSaintFragment();
                if (SaintFragments.ContainsKey(fragment.Id))
                    SaintFragments[fragment.Id].Add(fragment);
                else
                    SaintFragments.Add(fragment.Id, new List<SaintFragmentData>() { fragment });
                break;

            case CollectibleType.LETTER:
                var letter = GameDataManager.Instance.GetRandomLetter();
                Letters.Add(letter);
                break;
        }
    }

    public bool IsInventoryFull()
    {
        var autodelivery = GetProvision(Provision.AUTO_DELIVER);
        return (Items.Count >= MaxInventorySlots && autodelivery == null) || (Items.Count >= MaxInventorySlots && autodelivery != null && AutoDeliveryItems.Sum(x => x.Value.Count) == autodelivery.Value);
    }

    public bool AddToInventory(ItemType item, int amount = 1)
    {
        for(int i = 0; i < amount; i++)
        {
            if (Items.Count == MaxInventorySlots)
            {
                UI.Instance.DisplayMessage("INVENTORY FULL!");
                return false;
            }
            Items.Add(item);
            RefreshInventoryUI?.Invoke();
        }

        return true;
    }

    public void RemoveFromInventory(ItemType item)
    {
        Items.Remove(item);
    }

    public void ClearInventory()
    {
        Items.Clear();
        RefreshInventoryUI?.Invoke();
    }

    public void AddCollectible(string newCollectible)
    {
        Collectibles.Add(newCollectible);
        GameManager.Instance.WorldCollectibles.Remove(newCollectible);
        UI.Instance.SacredItemPopupEnable(newCollectible); 
        Debug.Log("COLLECTED: " + newCollectible);

        if(Collectibles.Count == 33)
        {
            SteamManager.Instance.UnlockAchievement("KEEPER");
            XboxUserHandler.Instance.UnlockAchievement("14");
        }
        else if(Collectibles.Count == 66)
        {
            SteamManager.Instance.UnlockAchievement("LIBRARY");
            XboxUserHandler.Instance.UnlockAchievement("15");
        }
    }

    public void AddGridCollectible(string newCollectible)
    {
        Collectibles.Add(newCollectible);
        Debug.Log("COLLECTED: " + newCollectible);
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
        }
        HasChosenProvision = true;
        RefreshInventoryUI?.Invoke();
        CheckProvisionAchievements();
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
        HasChosenProvision = true;
        RefreshInventoryUI?.Invoke();
        CheckProvisionAchievements();
    }

    void CheckProvisionAchievements()
    {
        if (Provisions.Count < MaxProvisionsSlots) return;

        foreach(var prov in Provisions)
        {
            ProvisionData nextLevel = GameDataManager.Instance.GetProvision(prov.Id, prov.Level + 1);
            if (nextLevel != null) return;
        }

        XboxUserHandler.Instance.UnlockAchievement("17");
        SteamManager.Instance.UnlockAchievement("CACHE");
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

    public int CountItem(ItemType itemType)
    {
        return Items.FindAll(i => i == itemType).Count();
    }

    public void GenerateProvisionsForNewDay()
    {
        if (!GameSettings.Instance.ProvisionsToggle) return;
        GameClock c = GameManager.Instance.GameClock;

        if (MissionManager.MissionOver) return;

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

        //Check to make sure that we dont already have the Provision in our Inventory
        var prov1 = GameDataManager.Instance.ProvisionData[(Provision)Random.Range(0, (int)Provision.MAX_COUNT)][0];
        
        if (GameSettings.Instance.DEMO_MODE_3)
        {
            var demoIds = new List<int>() { 2,3,5,6,7,8};
            demoIds.Shuffle();
            var demoId = demoIds.First();
            prov1 = GameDataManager.Instance.ProvisionData[(Provision)demoId][0];
        }

        prov1 = SwapProvisionBySeason(prov1);
        while (Provisions.Any(p => p.Id == prov1.Id))
        {
            prov1 = GameDataManager.Instance.ProvisionData[(Provision)Random.Range(0, (int)Provision.MAX_COUNT)][0];
            if (GameSettings.Instance.DEMO_MODE_3)
            {
                var demoIds = new List<int>() { 2, 3, 5, 6, 7, 8 };
                demoIds.Shuffle();
                var demoId = demoIds.First();
                prov1 = GameDataManager.Instance.ProvisionData[(Provision)demoId][0];
            }
            prov1 = SwapProvisionBySeason(prov1);
        }

        var prov2 = GameDataManager.Instance.ProvisionData[(Provision)Random.Range(0, (int)Provision.MAX_COUNT)][0];
  
        if (GameSettings.Instance.DEMO_MODE_3)
        {
            var demoIds = new List<int>() { 2, 3, 5, 6, 7, 8 };
            demoIds.Shuffle();
            var demoId = demoIds.First();
            prov2 = GameDataManager.Instance.ProvisionData[(Provision)demoId][0];
        }
  
        prov2 = SwapProvisionBySeason(prov2);

        while (Provisions.Any(p => p.Id == prov2.Id) || prov2.Id == prov1.Id)
        {
            prov2 = GameDataManager.Instance.ProvisionData[(Provision)Random.Range(0, (int)Provision.MAX_COUNT)][0];
            if (GameSettings.Instance.DEMO_MODE_3)
            {
                var demoIds = new List<int>() { 2, 3, 5, 6, 7, 8 };
                demoIds.Shuffle();
                var demoId = demoIds.First();
                prov2 = GameDataManager.Instance.ProvisionData[(Provision)demoId][0];
            }
            prov2 = SwapProvisionBySeason(prov2);
        }

        GeneratedProvisions = GeneratedProvisions.Any() ? GeneratedProvisions : GameManager.Instance.SaveData.GeneratedProvisions?.ToList();
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
    }

    private ProvisionData SwapProvisionBySeason(ProvisionData prov)
    {
        switch (MissionManager.Instance.CurrentMission.Season)
        {
                //if (prov.Id == Provision.WINTER_CLOAK || prov.Id == Provision.UMBRELLA)
                //{
                //    return GameDataManager.Instance.ProvisionData[Provision.SHADES][0];
                //}
                //break;

            case Season.SUMMER:
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
    //    GameClock.StartNewDay -= GenerateProvisionsForNewDay;
    }
}
