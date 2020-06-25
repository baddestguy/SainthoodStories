using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameDataManager : MonoBehaviour
{
    public static GameDataManager Instance { get; private set; }

    public Dictionary<EventType, List<CustomEventData>> CustomEventData = new Dictionary<EventType, List<CustomEventData>>();
    public Dictionary<string, List<LocalizationData>> LocalizationData = new Dictionary<string, List<LocalizationData>>();

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

        //Localization
        csvFile = Resources.Load<TextAsset>("GameData/Localization");
        var locData = CSVSerializer.Deserialize<LocalizationData>(csvFile.text);
        foreach(var loc in locData)
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
    }
}
