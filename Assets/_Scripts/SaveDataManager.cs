using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
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

    [HideInInspector]public SavedDataUiHandler savedDataUiHandler;

    private const string FILENAME = "Sainthood.save";
    
    string GetPath(string fileName) => Path.Combine(Application.persistentDataPath , fileName);
    private bool FileExixst(string fileName) => File.Exists(GetPath(FILENAME));


    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        
        Debug.Log("SAVE PATH: " + Application.persistentDataPath);
    }

    public void SaveGame()
    {
        if (GameSettings.Instance.FTUE) return;

        if (!FileExixst(FILENAME))
        {
           //Beter just do a direct save than looking for what do not exist...
            FirstSave();
        }
        else
        {
            DoSequentialSave();
        }

        /*
        SaveObject save = new SaveObject()
        {
            FP = MissionManager.Instance.FaithPoints,
            CP = MissionManager.Instance.CharityPoints,
            Energy = GameManager.Instance.Player.GetEnergyAmount(),
            Week = MissionManager.Instance.CurrentMission.CurrentWeek,
            Day = GameManager.Instance.GameClock.Day,
            Time = GameManager.Instance.GameClock.Time,
            TutorialSteps = TutorialManager.Instance.CurrentTutorialStep,
            Money = TreasuryManager.Instance.Money,
            HospitalRelationshipPoints = FindObjectOfType<InteractableHospital>().RelationshipPoints,
            SchoolRelationshipPoints = FindObjectOfType<InteractableSchool>().RelationshipPoints,
            OrphanageRelationshipPoints = FindObjectOfType<InteractableOrphanage>().RelationshipPoints,
            ShelterRelationshipPoints = FindObjectOfType<InteractableShelter>().RelationshipPoints,
            ClothesRelationshipPoints = FindObjectOfType<InteractableClothesBank>().RelationshipPoints,
            HospitalBuildingState = FindObjectOfType<InteractableHospital>().BuildingState,
            SchoolBuildingState = FindObjectOfType<InteractableSchool>().BuildingState,
            OrphanageBuildingState = FindObjectOfType<InteractableOrphanage>().BuildingState,
            ShelterBuildingState = FindObjectOfType<InteractableShelter>().BuildingState,
            ClothesBuildingState = FindObjectOfType<InteractableClothesBank>().BuildingState,
            KitchenBuildingState = FindObjectOfType<InteractableKitchen>().BuildingState,
            Saints = SaintsManager.Instance.UnlockedSaints.Select(s => s.Id).ToArray(),
            InventoryItems = InventoryManager.Instance.Items.ToArray(),
            Provisions = InventoryManager.Instance.Provisions.ToArray()
        };

        


        BinaryFormatter bf = new BinaryFormatter();
        //File.Open("", FileMode.OpenOrCreate);
        FileStream file = File.Create(Application.persistentDataPath + $"/Sainthood.save");
        bf.Serialize(file, save);
        file.Close();
        */
        Debug.Log("SAVED!");
    }

    public SaveObject CurrentSaveData()
    {
        return new SaveObject()
        {
            FP = MissionManager.Instance.FaithPoints,
            CP = MissionManager.Instance.CharityPoints,
            Energy = GameManager.Instance.Player.GetEnergyAmount(),
            Week = MissionManager.Instance.CurrentMission.CurrentWeek,
            Day = GameManager.Instance.GameClock.Day,
            Time = GameManager.Instance.GameClock.Time,
            TutorialSteps = TutorialManager.Instance.CurrentTutorialStep,
            Money = TreasuryManager.Instance.Money,
            HospitalRelationshipPoints = FindObjectOfType<InteractableHospital>().RelationshipPoints,
            SchoolRelationshipPoints = FindObjectOfType<InteractableSchool>().RelationshipPoints,
            OrphanageRelationshipPoints = FindObjectOfType<InteractableOrphanage>().RelationshipPoints,
            ShelterRelationshipPoints = FindObjectOfType<InteractableShelter>().RelationshipPoints,
            ClothesRelationshipPoints = FindObjectOfType<InteractableClothesBank>().RelationshipPoints,
            HospitalBuildingState = FindObjectOfType<InteractableHospital>().BuildingState,
            SchoolBuildingState = FindObjectOfType<InteractableSchool>().BuildingState,
            OrphanageBuildingState = FindObjectOfType<InteractableOrphanage>().BuildingState,
            ShelterBuildingState = FindObjectOfType<InteractableShelter>().BuildingState,
            ClothesBuildingState = FindObjectOfType<InteractableClothesBank>().BuildingState,
            KitchenBuildingState = FindObjectOfType<InteractableKitchen>().BuildingState,
            Saints = SaintsManager.Instance.UnlockedSaints.Select(s => s.Id).ToArray(),
            InventoryItems = InventoryManager.Instance.Items.ToArray(),
            Provisions = InventoryManager.Instance.Provisions.ToArray()
        };
    }
    

    public SaveObject NewGameData()
    {
        return new SaveObject()
        {
            FP = 15,
            CP = 15,
            Energy = 20,
            Week = 1,
            Day = 1,
            Time = 6,
            TutorialSteps = 0,
            Money = 100
        };
    }


    public void FirstSave()
    {
        //InitialDataSaving
        SaveObject save = CurrentSaveData();
        SaveObject[] saveObjects = new SaveObject[1] { save };

        Save(saveObjects);

        //BinaryFormatter bf = new BinaryFormatter();
        //FileStream file = File.Create(Application.persistentDataPath + $"/Sainthood.save");
        //bf.Serialize(file, saveObjects);
        //file.Close();
    }


    private bool IsNewWeek(SaveObject[] saveObjects , SaveObject newData)
    {
        return saveObjects[saveObjects.Length - 1].Week != newData.Week;
    }

    private void DoSequentialSave()
    {
        SaveObject saveObject = CurrentSaveData();

        Dictionary<Days, SaveObject> keyVal = GetSavedDataSet();

        if(keyVal == null)
        {
            FirstSave();
            return;
        }

        //Compare the saved week with the current
        
        if (IsNewWeek(keyVal.Values.ToArray(), saveObject))
        {
            FirstSave();
            return;
        }


        if (keyVal.ContainsKey((Days)saveObject.Day))
        {
            keyVal[(Days)saveObject.Day] = saveObject;
        }
        else
        {
            keyVal.Add((Days)saveObject.Day, saveObject);
        }

        SaveObject[] saveObjects = keyVal.Values.ToArray().OrderBy(x => x.Day).ToArray();


        Save(saveObjects);

        
    }

    private void Save(SaveObject[] data)
    {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(GetPath(FILENAME));
        bf.Serialize(file, data);
        file.Close();
    }


    private Dictionary<Days, SaveObject> GetSavedDataSet()
    {

        
        try
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(GetPath(FILENAME), FileMode.Open);
            SaveObject[] saveObjects = (SaveObject[])bf.Deserialize(file);
            file.Close();
            Dictionary<Days, SaveObject> keyVal = saveObjects.ToDictionary(x => (Days)x.Day, x => x);
            return keyVal;
        }
        catch(Exception e)
        {
            Debug.LogError("Error " + e.Message);
            return null;

        }
        

        
        

    }


    [Obsolete("use LoadGame(Action<SaveObject> callback)")]
    public SaveObject LoadGame()
    {
        SaveObject save;
        if(File.Exists(Application.persistentDataPath + "/Sainthood.save"))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath + "/Sainthood.save", FileMode.Open);

            save = (SaveObject)bf.Deserialize(file);
            save.Day = OverrideClock ? DayOverride : save.Day;
            save.Time = OverrideClock ? TimeOverride : save.Time;
            file.Close();
            Debug.Log("LOADED!");
        }
        else
        {
            //New Game
            save = new SaveObject()
            {
                FP = 15,
                CP = 15,
                Energy = 20,
                Week = 1,
                Day = 1,
                Time = 6,
                TutorialSteps = 0,
                Money = 100
            };
            Debug.Log("NEW GAME!");
        }
        return save;
    }


    
    
    /// <summary>
    /// Load the game data 
    /// </summary>
    /// <param name="callback"></param>
    public void LoadGame(Action<SaveObject> callback , bool lastDay = false)
    {
        if (Directory.Exists(Application.persistentDataPath))
        {
            Dictionary<Days, SaveObject> keyVal = GetSavedDataSet();

            if(keyVal == null)
            {
                callback?.Invoke(NewGameData());
                return;
            }

            SaveObject[] saveObjects = keyVal.Values.ToArray();
            if(saveObjects == null || saveObjects.Length <= 0)
            {
                callback?.Invoke(NewGameData());
                return;
            }

            if (lastDay)
            {
                SaveObject data = saveObjects.OrderBy(x => x.Day).LastOrDefault();
                callback?.Invoke(data);
                return;
            }

            savedDataUiHandler.Pupulate(saveObjects, (data) => {

                Dictionary<Days, SaveObject> set = GetSavedDataSet();
                SaveObject[] newData = set.Where(x => ((int)x.Key) <= data.Day).Select(x => x.Value).ToArray();
                Save(newData);

                callback?.Invoke(data);
            });
        }
        else
        {
            callback?.Invoke(NewGameData());
        }
    }




    public void DeleteSave()
    {
        if (File.Exists(Application.persistentDataPath + "/Sainthood.save"))
        {
            File.Delete(Application.persistentDataPath + "/Sainthood.save");
        }
    }
}


