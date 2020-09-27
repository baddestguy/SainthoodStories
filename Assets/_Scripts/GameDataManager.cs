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
    public Dictionary<CustomEventType, List<CustomEventData>> CustomEventData = new Dictionary<CustomEventType, List<CustomEventData>>();
    public Dictionary<string, List<LocalizationData>> LocalizationData = new Dictionary<string, List<LocalizationData>>();
    public Dictionary<Provision, ProvisionData> ProvisionData = new Dictionary<Provision, ProvisionData>();
    public Dictionary<ItemType, ShopItemData> ShopItemData = new Dictionary<ItemType, ShopItemData>();
    public Dictionary<string, ConstructionAvailabilityData> ConstructionAvailability = new Dictionary<string, ConstructionAvailabilityData>();
    public Dictionary<string, StoryEventData> StoryEventData = new Dictionary<string, StoryEventData>();
    public Dictionary<string, List<BuildingMissionData>> BuildingMissionData = new Dictionary<string, List<BuildingMissionData>>();
    public List<WeatherData> WeatherData = new List<WeatherData>();

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

        //Construction Availability
        csvFile = Resources.Load<TextAsset>("GameData/ConstructionAvailability");
        var availData = CSVSerializer.Deserialize<ConstructionAvailabilityData>(csvFile.text);
        foreach (var item in availData)
        {
            ConstructionAvailability.Add(item.Id, item);
        }

        yield return null;

        //Story Events 
        csvFile = Resources.Load<TextAsset>("GameData/StoryEvents");
        var storyData = CSVSerializer.Deserialize<StoryEventData>(csvFile.text);
        foreach (var item in storyData)
        {
            StoryEventData.Add(item.Id, item);
        }

        yield return null;

        //Building Missions
        csvFile = Resources.Load<TextAsset>("GameData/BuildingMissions");
        var missionData = CSVSerializer.Deserialize<BuildingMissionData>(csvFile.text);
        foreach (var item in missionData)
        {
            if (BuildingMissionData.ContainsKey(item.InteractableHouse))
            {
                BuildingMissionData[item.InteractableHouse].Add(item);
            }
            else
            {
                BuildingMissionData.Add(item.InteractableHouse, new List<BuildingMissionData>() { item });
            }
        }

        yield return null;

        //Weather Data
        csvFile = Resources.Load<TextAsset>("GameData/WeatherData");
        var weatherData = CSVSerializer.Deserialize<WeatherData>(csvFile.text);
        foreach (var item in weatherData)
        {
            WeatherData.Add(item);
        }

        yield return null;
    }

    public CustomEventData GetRandomEvent(EventGroup eGroup)
    {
        List<CustomEventData> list = new List<CustomEventData>();
        foreach(var ePair in CustomEventData)
        {
            var e = ePair.Value[Random.Range(0,ePair.Value.Count)]; //Based on weight
            list.Add(e);
        }

        var groupList = list.Where(e => e.EventGroup == eGroup).ToList();
        return groupList[Random.Range(0, groupList.Count)];
    }

    public bool IsSpritualEvent(CustomEventType e)
    {
        return e == CustomEventType.SPIRITUAL_RETREAT ||
            e == CustomEventType.PRAYER_REQUEST;
    }

    public IEnumerable<BuildingMissionData> GetBuildingMissionData(string houseName)
    {
        return BuildingMissionData.ContainsKey(houseName) ? BuildingMissionData[houseName] : new List<BuildingMissionData>();
    }
}
