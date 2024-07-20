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
    public Dictionary<PlayerStatusEffect, List<StatusEffectData>> StatusEffectData = new Dictionary<PlayerStatusEffect, List<StatusEffectData>>();
    public Dictionary<ItemType, ShopItemData> ShopItemData = new Dictionary<ItemType, ShopItemData>();
    public Dictionary<string, ConstructionAvailabilityData> ConstructionAvailability = new Dictionary<string, ConstructionAvailabilityData>();
    public Dictionary<SaintID, SaintData> Saints = new Dictionary<SaintID, SaintData>();
    public Dictionary<string, StoryEventData> StoryEventData = new Dictionary<string, StoryEventData>();
    public Dictionary<string, List<BuildingMissionData>> BuildingMissionData = new Dictionary<string, List<BuildingMissionData>>();
    public Dictionary<int, List<ObjectivesData>> ObjectivesData = new Dictionary<int, List<ObjectivesData>>();
    public Dictionary<string, List<HouseObjectivesData>> HouseObjectivesData = new Dictionary<string, List<HouseObjectivesData>>();
    public Dictionary<int, CollectibleObjectivesData> CollectibleObjectivesData = new Dictionary<int, CollectibleObjectivesData>();
    public Dictionary<string, List<CollectibleData>> CollectibleData = new Dictionary<string, List<CollectibleData>>();
    public List<WeatherData> WeatherData = new List<WeatherData>();
    public Dictionary<TooltipStatId, TooltipStats> ToolTips = new Dictionary<TooltipStatId, TooltipStats>();
    public Dictionary<MinigameType, MinigameData> MinigameData = new Dictionary<MinigameType, MinigameData>();

    public int MidnightEventChosenIndex;
    public List<CustomEventType> TriggeredDailyEvents;

    public const int TOTAL_UNLOCKABLE_SAINTS = 25;
    public const int MAX_RP_THRESHOLD = 65;
    public const int MED_RP_THRESHOLD = 30;
    public const int MIN_RP_THRESHOLD = 5;
    public const int MAX_HOUSE_MISSION_ID = 14;
    public const int MAX_MISSION_ID = 33;

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

        //Objectives
        csvFile = Resources.Load<TextAsset>("GameData/Objectives");
        var objData = CSVSerializer.Deserialize<ObjectivesData>(csvFile.text);
        foreach (var item in objData)
        {
            if (ObjectivesData.ContainsKey(item.Id))
            {
                ObjectivesData[item.Id].Add(item);
            }
            else
            {
                ObjectivesData.Add(item.Id, new List<ObjectivesData>() { item });
            }
        }

        //House Objectives
        csvFile = Resources.Load<TextAsset>("GameData/HouseObjectives");
        var houseObjData = CSVSerializer.Deserialize<HouseObjectivesData>(csvFile.text);
        foreach (var item in houseObjData)
        {
            if (HouseObjectivesData.ContainsKey(item.House))
            {
                HouseObjectivesData[item.House].Add(item);
            }
            else
            {
                HouseObjectivesData.Add(item.House, new List<HouseObjectivesData>() { item });
            }
        }

        yield return null;

        //Collectible Objectives
        csvFile = Resources.Load<TextAsset>("GameData/CollectibleObjectives");
        var colObjData = CSVSerializer.Deserialize<CollectibleObjectivesData>(csvFile.text);
        foreach (var item in colObjData)
        {
            if (CollectibleObjectivesData.ContainsKey(item.Id))
            {
                CollectibleObjectivesData[item.Id] = item;
            }
            else
            {
                CollectibleObjectivesData.Add(item.Id, item);
            }
        }

        yield return null;

        //Collectibles
        csvFile = Resources.Load<TextAsset>("GameData/Collectibles");
        var colData = CSVSerializer.Deserialize<CollectibleData>(csvFile.text);
        foreach (var item in colData)
        {
            if (CollectibleData.ContainsKey(item.Id))
            {
                CollectibleData[item.Id].Add(item);
            }
            else
            {
                CollectibleData.Add(item.Id, new List<CollectibleData>() { item });
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

        //Status Effects
        csvFile = Resources.Load<TextAsset>("GameData/StatusEffects");
        var statusData = CSVSerializer.Deserialize<StatusEffectData>(csvFile.text);
        foreach (var status in statusData)
        {
            if (StatusEffectData.ContainsKey(status.Id))
            {
                StatusEffectData[status.Id].Add(status);
            }
            else
            {
                StatusEffectData.Add(status.Id, new List<StatusEffectData>() { status });
            }
        }

        yield return null;

        //Tooltips
        csvFile = Resources.Load<TextAsset>("GameData/Tooltips");
        var tips = CSVSerializer.Deserialize<TooltipStats>(csvFile.text);
        foreach (var item in tips)
        {
            ToolTips.Add(item.Id, item);
        }

        yield return null;

        //Minigames
        csvFile = Resources.Load<TextAsset>("GameData/Minigames");
        var games = CSVSerializer.Deserialize<MinigameData>(csvFile.text);
        foreach (var item in games)
        {
            MinigameData.Add(item.Id, item);
        }

        yield return null;

    }

    public StatusEffectData GetStatusEffectData(PlayerStatusEffect id)
    {
        return StatusEffectData[id].FirstOrDefault();
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
            var currentDay = GameManager.Instance.GameClock.Day;
            var currentWeek = MissionManager.Instance.CurrentMission.CurrentWeek;

            var eventWeek = int.Parse(returnData.TriggerWeekDay.Split(',')[0]);
            var eventDay = int.Parse(returnData.TriggerWeekDay.Split(',')[1]);

            while (TriggeredDailyEvents.Contains(returnData.Id) || currentWeek < eventWeek || currentDay < eventDay)
            {
                returnData = groupList[Random.Range(0, groupList.Count)];
                eventWeek = int.Parse(returnData.TriggerWeekDay.Split(',')[0]);
                eventDay = int.Parse(returnData.TriggerWeekDay.Split(',')[1]);
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

    public CollectibleData GetCollectibleData(string name)
    {
        foreach(var kvp in CollectibleData)
        {
            foreach(var c in kvp.Value)
            {
                if(c.Name == name)
                {
                    return c;
                }
            }
        }

        return null;
    }

    public int GetNextSaintUnlockThreshold()
    {
        var unlockedSaintsCount = GameManager.Instance.SaveData.Saints.Length;

        if(unlockedSaintsCount == TOTAL_UNLOCKABLE_SAINTS) return Constants[$"SAINTS_UNLOCK_THRESHOLD_25"].IntValue;

        if (GameSettings.Instance.DEMO_MODE_2)
        {
            return 15;
        }
        return Constants[$"SAINTS_UNLOCK_THRESHOLD_{ unlockedSaintsCount + 1 }"].IntValue;
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

    public IEnumerable<ObjectivesData> GetObjectivesData(int id)
    {
        if (ObjectivesData.ContainsKey(id))
            return ObjectivesData[id];
        else
        {
            Debug.Log("COULD NOT FIND OBJECTIVE WITH ID: " + id);
            return Enumerable.Empty<ObjectivesData>();
        }
    }

    public ObjectivesData GetSingleObjective(int id)
    {
        return GetObjectivesData(id).FirstOrDefault();
    }

    public TooltipStats GetToolTip(TooltipStatId id, double ticksModifier = 0, double ticksOverride = 0, int fpModifier = 0, int fpOverride = 0, int cpModifier = 0, int cpOverride = 0, int energyModifier = 0)
    {
        var tooltip = new TooltipStats() { Id = ToolTips[id].Id, Ticks = ToolTips[id].Ticks, CP = ToolTips[id].CP, FP = ToolTips[id].FP, Energy = ToolTips[id].Energy };

        if (ticksOverride != 0)
        {
            tooltip.Ticks = ticksOverride;
        }
        if (ticksModifier != 0)
        {
            tooltip.Ticks += ticksModifier;
        }

        if (fpOverride != 0)
        {
            tooltip.FP = fpOverride;
        }
        if (fpModifier != 0)
        {
            tooltip.FP += fpModifier;
        }

        if (cpOverride != 0)
        {
            tooltip.CP = cpOverride;
        }
        if (cpModifier != 0)
        {
            tooltip.CP += cpModifier;
        }

        if (energyModifier != 0)
        {
            tooltip.Energy += energyModifier;
        }

        return tooltip;
    }
}
