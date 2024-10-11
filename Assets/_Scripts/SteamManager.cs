using System.Collections.Generic;
using Steamworks;
using Steamworks.Data;
using UnityEngine;

public class SteamManager : MonoBehaviour
{
    public static SteamManager Instance;

    public Dictionary<string, Achievement> Achievements = new Dictionary<string, Achievement>()
    {
        {"GETTING_STARTED", new Achievement("GETTING_STARTED")},
        {"CANONIZED", new Achievement("CANONIZED")},
        {"MIDWAY", new Achievement("MIDWAY")},
        {"HEALER", new Achievement("HEALER")},
        {"COMMUNION", new Achievement("COMMUNION")},
        {"INTERVENTION", new Achievement("INTERVENTION")},
        {"ARCHITECT", new Achievement("ARCHITECT")},
        {"ANGEL", new Achievement("ANGEL")},
        {"TEACHER", new Achievement("TEACHER")},
        {"FEAST", new Achievement("FEAST")},
        {"FEEDER", new Achievement("FEEDER")},
        {"FINISHED", new Achievement("FINISHED")},
        {"SEEKER", new Achievement("SEEKER")},
        {"KEEPER", new Achievement("KEEPER")},
        {"LIBRARY", new Achievement("LIBRARY")},
        {"KINGDOM_ARCHITECT", new Achievement("KINGDOM_ARCHITECT")},
        {"CACHE", new Achievement("CACHE")},
        {"LITANY", new Achievement("LITANY")},
        {"PIOUS", new Achievement("PIOUS")}
    };

    private void Awake()
    {
        Instance = this;
        try
        {
            SteamClient.Init(GameDataManager.APP_ID);
        }
        catch
        {
            Debug.LogError("Couldn't initialize Steam client!");
        }
    }

    private void Update()
    {
        if (GameSettings.Instance.IsXboxMode) return;
        
        SteamClient.RunCallbacks();
    }

    public void UnlockAchievement(string id)
    {
        if (!Achievements.ContainsKey(id) || GameSettings.Instance.IsXboxMode) return;

        Achievements[id].Trigger();
    }

    public void ClearAchievement(string id)
    {
        if (!Achievements.ContainsKey(id)) return;

        Achievements[id].Clear();
    }
}
