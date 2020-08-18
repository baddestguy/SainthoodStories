using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameDataManager : MonoBehaviour
{
    public static GameDataManager Instance { get; private set; }

    public Dictionary<string, ConstantsData> Constants = new Dictionary<string, ConstantsData>();
    public Dictionary<EventType, List<CustomEventData>> CustomEventData = new Dictionary<EventType, List<CustomEventData>>();
    public Dictionary<string, List<LocalizationData>> LocalizationData = new Dictionary<string, List<LocalizationData>>();
    public Dictionary<Provision, ProvisionData> ProvisionData = new Dictionary<Provision, ProvisionData>();
    public Dictionary<ItemType, ShopItemData> ShopItemData = new Dictionary<ItemType, ShopItemData>();

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        LoadData();        
    }

    public void LoadData()
    {
        StartCoroutine(LoadDataAsync());
    }

    private IEnumerator LoadDataAsync()
    {
        //Custom Events
        TextAsset csvFile = Resources.Load<TextAsset>("GameData/CustomEvents");
        var customEvents = CSVSerializer.Deserialize<CustomEventData>(csvFile.text);
        foreach (var ev in customEvents)
        {
            if (CustomEventData.ContainsKey(ev.Id))
            {
                CustomEventData[ev.Id].Add(ev);
            }
            else
            {
                CustomEventData.Add(ev.Id, new List<CustomEventData>() { ev });
            }
        }

        yield return null;

        //Constants
        csvFile = Resources.Load<TextAsset>("GameData/Constants");
        var constData = CSVSerializer.Deserialize<ConstantsData>(csvFile.text);
        foreach (var con in constData)
        {
            Constants.Add(con.Id, con);
        }

        yield return null;

        //Localization
        csvFile = Resources.Load<TextAsset>("GameData/Localization");
        var locData = CSVSerializer.Deserialize<LocalizationData>(csvFile.text);
        foreach (var loc in locData)
        {
            if (LocalizationData.ContainsKey(loc.Key))
            {
                LocalizationData[loc.Key].Add(loc);
            }
            else
            {
                LocalizationData.Add(loc.Key, new List<LocalizationData>() { loc });
            }
        }

        yield return null;

        //Provisions
        csvFile = Resources.Load<TextAsset>("GameData/Provisions");
        var provData = CSVSerializer.Deserialize<ProvisionData>(csvFile.text);
        foreach (var prov in provData)
        {
            ProvisionData.Add(prov.Id, prov);
        }

        yield return null;

        //ShopItems
        csvFile = Resources.Load<TextAsset>("GameData/ShopItems");
        var shopData = CSVSerializer.Deserialize<ShopItemData>(csvFile.text);
        foreach (var item in shopData)
        {
            ShopItemData.Add(item.Id, item);
        }

        yield return null;
    }

    public CustomEventData GetRandomEvent(EventPopupType eGroup)
    {
        List<CustomEventData> list = new List<CustomEventData>();
        foreach(var ePair in CustomEventData)
        {
            var e = ePair.Value[Random.Range(0,ePair.Value.Count)]; //Based on weight
            list.Add(e);
        }

        var groupList = list.Where(e => e.EventPopupType == eGroup).ToList();
        return groupList[Random.Range(0, groupList.Count)];
    }

    public bool IsSpritualEvent(EventType e)
    {
        return e == EventType.SPIRITUAL_RETREAT ||
            e == EventType.PRAYER_REQUEST;
    }
}
