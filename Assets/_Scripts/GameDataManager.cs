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
    public Dictionary<Provision, List<ProvisionData>> ProvisionData = new Dictionary<Provision, List<ProvisionData>>();
    public Dictionary<ItemType, ShopItemData> ShopItemData = new Dictionary<ItemType, ShopItemData>();
    public Dictionary<string, ConstructionAvailabilityData> ConstructionAvailability = new Dictionary<string, ConstructionAvailabilityData>();
    public Dictionary<SaintID, SaintData> Saints = new Dictionary<SaintID, SaintData>();
    public Dictionary<string, StoryEventData> StoryEventData = new Dictionary<string, StoryEventData>();
    public Dictionary<string, List<BuildingMissionData>> BuildingMissionData = new Dictionary<string, List<BuildingMissionData>>();
    public List<WeatherData> WeatherData = new List<WeatherData>();

    public int MidnightEventChosenIndex;
    public List<CustomEventType> TriggeredDailyEvents;

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
            if (ProvisionData.ContainsKey(prov.Id))
            {
                ProvisionData[prov.Id].Add(prov);
            }
            else
            {
                ProvisionData.Add(prov.Id, new List<ProvisionData>() { prov });
            }
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

        //Saints
        csvFile = Resources.Load<TextAsset>("GameData/Saints");
        var saintData = CSVSerializer.Deserialize<SaintData>(csvFile.text);
        foreach (var item in saintData)
        {
            Saints.Add(item.Id, item);
        }

        yield return null;
    }

    public ProvisionData GetProvision(Provision provision, int level = 1)
    {
        return ProvisionData[provision].Where(p => p.Level == level).FirstOrDefault();
    }

    public CustomEventData GetRandomEvent(EventGroup eGroup)
    {
        List<CustomEventData> list = new List<CustomEventData>();
        foreach(var ePair in CustomEventData)
        {
            MidnightEventChosenIndex = Random.Range(0, ePair.Value.Count);
            var e = ePair.Value[MidnightEventChosenIndex]; //Based on weight
            list.Add(e);
        }

        var groupList = list.Where(e => e.EventGroup == eGroup).ToList();
        CustomEventData returnData = groupList[Random.Range(0, groupList.Count)];

        if(eGroup == EventGroup.DAILY)
        {
            while (TriggeredDailyEvents.Contains(returnData.Id))
            {
                returnData = groupList[Random.Range(0, groupList.Count)];
            }
            TriggeredDailyEvents.Add(returnData.Id);
        }

        return returnData;
    }

    public CustomEventData GetEvent(CustomEventType id)
    {
        var groupList = CustomEventData[id];
        CustomEventData returnData = groupList.FirstOrDefault();

        return returnData;
    }


    public CustomEventData RemixEventBySeason(CustomEventData data)
    {
        switch (data.Id)
        {
            case CustomEventType.RAIN:
                if (MissionManager.Instance.CurrentMission.Season == Season.SUMMER)
                {
                    return CustomEventData[CustomEventType.HEATWAVE][MidnightEventChosenIndex];
                }
                else if (MissionManager.Instance.CurrentMission.Season == Season.WINTER)
                {
                    return CustomEventData[CustomEventType.BLIZZARD][MidnightEventChosenIndex];
                }
                break;
            case CustomEventType.HEATWAVE:
                if (MissionManager.Instance.CurrentMission.Season == Season.FALL)
                {
                    return CustomEventData[CustomEventType.RAIN][MidnightEventChosenIndex];
                }
                else if (MissionManager.Instance.CurrentMission.Season == Season.WINTER)
                {
                    return CustomEventData[CustomEventType.BLIZZARD][MidnightEventChosenIndex];
                }
                break;
            case CustomEventType.BLIZZARD:
                if (MissionManager.Instance.CurrentMission.Season == Season.SUMMER)
                {
                    return CustomEventData[CustomEventType.HEATWAVE][MidnightEventChosenIndex];
                }
                else if (MissionManager.Instance.CurrentMission.Season == Season.FALL)
                {
                    return CustomEventData[CustomEventType.RAIN][MidnightEventChosenIndex];
                }
                break;
        }

        return data;
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
