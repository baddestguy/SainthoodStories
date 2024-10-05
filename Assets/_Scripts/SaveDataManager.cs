using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using Assets._Scripts.Xbox;
using UnityEngine;


public enum Days
{
    Monday = 1,
    Tuesday,
    Wednesday,
    Thursday,
    Friday,
    Saturday,
    Sunday
}




public class SaveDataManager : MonoBehaviour
{
    public static SaveDataManager Instance { get; private set; }

    public bool OverrideClock;
    public int DayOverride;
    public double TimeOverride;

    [HideInInspector] public SavedDataUiHandler savedDataUiHandler;

    private const string FILENAME = "Sainthood.save";
    private const string SPARE_FILENAME = "Sainthood_DayCheckpoint.save";

    string GetPath(string fileName) => Path.Combine(Application.persistentDataPath, fileName);
    private bool FileExixst() => File.Exists(GetPath(FILENAME));


    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (GameSettings.Instance.IsXboxMode)
        {

        }
        else
        {
            Debug.Log("SAVE PATH: " + Application.persistentDataPath);
        }
    }

    public void DaySave()
    {
        var save = CurrentSaveData();
        var data = new SaveObject[1] { save };

        Save(data, SPARE_FILENAME);
    }

    public void LoadDaySave()
    {
        var data = GetSavedDataSet(SPARE_FILENAME);
        var saveObjects = data.Values.ToArray();
        var save = saveObjects.OrderBy(x => x.Day).LastOrDefault();
        Save(new SaveObject[1] { save });
    }

    public void SaveGame()
    {
        if (GameSettings.Instance.TUTORIAL_MODE) return;
        DoSequentialSave();
    }

    public SaveObject CurrentSaveData()
    {
        return new SaveObject()
        {
            FP = MissionManager.Instance.FaithPoints,
            FPPool = MissionManager.Instance.FaithPointsPool,
            CP = MissionManager.Instance.CharityPoints,
            CPPool = MissionManager.Instance.CharityPointsPool,
            Energy = GameManager.Instance.Player.GetEnergyAmount(),
            Week = MissionManager.Instance.CurrentMission.CurrentWeek,
            Day = GameManager.Instance.GameClock.Day,
            Time = GameManager.Instance.GameClock.Time,
            TutorialSteps = TutorialManager.Instance.CurrentTutorialStep,
            Money = TreasuryManager.Instance.Money,
            TemporaryMoneyToDonate = TreasuryManager.Instance.TemporaryMoneyToDonate,
            Houses = GetSavedHouses(),
            Saints = SaintsManager.Instance.UnlockedSaints.Select(s => s.Id).ToArray(),
            InventoryItems = InventoryManager.Instance.Items.ToArray(),
            Provisions = InventoryManager.Instance.Provisions.ToArray(),
            GeneratedProvisions = InventoryManager.Instance.GeneratedProvisions.ToArray(),
            DailyEvent = EventsManager.Instance.DailyEvent,
            RunAttempts = GameManager.Instance.RunAttempts,
            Maptiles = GameManager.Instance.MaptileIndexes,
            CurrentHouse = GameManager.Instance.CurrentBuilding,
            StatusEffects = GameManager.Instance.Player.StatusEffects.ToArray(),
            HouseTriggeredEvent = InteractableHouse.HouseTriggeredEvent,
            CurrentDailyEvent = EventsManager.Instance.CurrentEvents.Find(e => e.EventGroup == EventGroup.DAILY),
            WeatherActivated = WeatherManager.Instance.WeatherForecastTriggered,
            WeatherStartTime = WeatherManager.Instance.WeatherStartTime.Time,
            WeatherStartDay = WeatherManager.Instance.WeatherStartTime.Day,
            WeatherEndTime = WeatherManager.Instance.WeatherEndTime.Time,
            WeatherEndDay = WeatherManager.Instance.WeatherEndTime.Day,
            CurrentMissionId = MissionManager.Instance.CurrentMissionId,
            Collectibles = InventoryManager.Instance.Collectibles?.ToArray(),
            WanderingSpirits = InventoryManager.Instance.WanderingSpirits,
            CurrentCollectibleMissionId = MissionManager.Instance.CurrentCollectibleMissionId,
            CurrentCollectibleCounter = MissionManager.Instance.CurrentCollectibleCounter,
            WorldCollectibles = GameManager.Instance.WorldCollectibles.ToArray(),
            MissionEvents = EventsManager.Instance.TriggeredMissionEvents.ToArray(),
            HasChosenProvision = InventoryManager.HasChosenProvision,
            FaithPointsPermanentlyLost = MissionManager.Instance.FaithPointsPermanentlyLost
        };
    }

    public HouseSaveData[] GetSavedHouses()
    {
        var houses = GameManager.Instance.Houses.Select(h => h.GetHouseSave());
        return houses.ToArray();
    }

    public SaveObject NewGameData()
    {
        return new SaveObject()
        {
            FP = 0,
            FPPool = 0,
            CP = 0,
            CPPool = 0,
            Energy = 3,
            Week = 1,
            Day = 1,
            Time = 5,
            TutorialSteps = 40,
            Money = 0
        };
    }


    private bool IsNewWeek(SaveObject[] saveObjects, SaveObject newData)
    {
        return saveObjects[saveObjects.Length - 1].Week != newData.Week;
    }

    private void DoSequentialSave()
    {
        SaveObject save = CurrentSaveData();
        SaveObject[] saves = new SaveObject[1] { save };
        
        Save(saves);
    }

    private void Save(SaveObject[] data, string fileName = FILENAME)
    {
        if (GameSettings.Instance.IsXboxMode)
        {
            XboxUserHandler.Instance.QueueSave(fileName, data);
        }
        else
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Create(GetPath(fileName));
            bf.Serialize(file, data);
            file.Close();
            //   Debug.Log("SAVED!");
        }
        GameManager.Instance.SaveData = data[0];
    }

    private Dictionary<Days, SaveObject> GetSavedDataSet(string fileName = FILENAME)
    {
        try
        {
            SaveObject[] saveObjects;
            if (GameSettings.Instance.IsXboxMode)
            {
                saveObjects = XboxUserHandler.Instance.LoadData<SaveObject[]>(fileName);
            }
            else
            {
                BinaryFormatter bf = new BinaryFormatter();
                FileStream file = File.Open(GetPath(fileName), FileMode.Open);
                saveObjects = (SaveObject[])bf.Deserialize(file);
                file.Close();
            }
            Dictionary<Days, SaveObject> keyVal = saveObjects.ToDictionary(x => (Days)x.Day, x => x);
            return keyVal;
        }
        catch (Exception e)
        {
            //Debug.LogError("Error " + e.Message);
            return null;

        }
    }

    /// <summary>
    /// Load the game data 
    /// </summary>
    /// <param name="callback"></param>
    public void LoadGame(Action<SaveObject, bool> callback, bool newGame, bool lastDay = false, bool ingameLoading = false, bool showUI = true)
    {
        if (GameSettings.Instance.IsXboxMode || Directory.Exists(Application.persistentDataPath))
        {
            Dictionary<Days, SaveObject> keyVal = GetSavedDataSet();

            if (keyVal == null || newGame)
            {
                callback?.Invoke(NewGameData(), true);
                return;
            }

            SaveObject[] saveObjects = keyVal.Values.ToArray();
            if (saveObjects == null || saveObjects.Length <= 0)
            {
                Debug.LogError("No save objects? ");
                callback?.Invoke(NewGameData(), true);
                return;
            }

            if (lastDay || !showUI)
            {
                SaveObject data = saveObjects.OrderBy(x => x.Day).LastOrDefault();
                callback?.Invoke(data, false);
                return;
            }

            {
                SaveObject data = saveObjects.OrderBy(x => x.Day).LastOrDefault();

                if (data == null)
                {
                    data = NewGameData();
                    Save(new[] { data });
                    newGame = true;
                }
                else
                {
                    Dictionary<Days, SaveObject> set = GetSavedDataSet();
                    SaveObject[] newData = set.Where(x => ((int)x.Key) <= data.Day).Select(x => x.Value).ToArray();
                    Save(newData);
                }
                //callback return
                //CheckOveride(ref data);
                callback?.Invoke(data, newGame);
            }
        }
        else
        {
            callback?.Invoke(NewGameData(), true);
        }
    }

    private void CheckOveride(ref SaveObject saveObject)
    {
        if (OverrideClock)
        {
            saveObject.Day = DayOverride;
            saveObject.Time = TimeOverride;
        }
    }

    public void ReloadDaySave()
    {

    }

    public void DeleteProgress()
    {
        if (File.Exists(Application.persistentDataPath + "/Sainthood.save"))
        {
            Debug.Log("RESET");
            Dictionary<Days, SaveObject> keyVal = GetSavedDataSet();

            SaveObject[] saveObjects = keyVal.Values.ToArray();
            SaveObject save = saveObjects.OrderBy(x => x.Day).LastOrDefault();
            var newSave = NewGameData();
            newSave.Saints = save.Saints;
            newSave.RunAttempts = GameManager.Instance.RunAttempts;

            File.Delete(Application.persistentDataPath + "/Sainthood.save");

            Save(new SaveObject[1] { newSave });
        }
    }
}