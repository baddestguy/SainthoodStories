#if STEAM_API
using System.Collections.Generic;
using Steamworks;
using Steamworks.Data;
#endif
using UnityEngine;

public class SteamManager : MonoBehaviour
{
    public static SteamManager Instance;

#if STEAM_API
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
    
#endif
    private void Awake()
    {
        Instance = this;
#if STEAM_API
        try
        {
            SteamClient.Init(GameDataManager.APP_ID);
        }
        catch
        {
            Debug.LogError("Couldn't initialize Steam client!");
        }
#endif
    }

    private void Update()
    {
#if STEAM_API
        if (GameSettings.Instance.IsXboxMode) return;
        
        SteamClient.RunCallbacks();
#endif
    }

    public void UnlockAchievement(string id)
    {

#if STEAM_API
        if (!Achievements.ContainsKey(id) || GameSettings.Instance.IsXboxMode) return;

        try
        {
            Achievements[id].Trigger();
        }
        catch
        {
            return;
        }
#endif
    }

    public void ClearAchievement(string id)
    {
#if STEAM_API
        
        if (!Achievements.ContainsKey(id)) return;

        Achievements[id].Clear();
#endif
    }
}
