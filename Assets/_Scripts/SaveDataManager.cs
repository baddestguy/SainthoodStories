using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class SaveDataManager : MonoBehaviour
{
    public static SaveDataManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        
    }

    public void SaveGame()
    {
        if (GameSettings.Instance.FTUE) return;

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
            Saints = SaintsManager.Instance.UnlockedSaints.Select(s => s.Id).ToArray()
        };

        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(Application.persistentDataPath + "/Sainthood.save");
        bf.Serialize(file, save);
        file.Close();
        Debug.Log("SAVED!");
    }

    public SaveObject LoadGame()
    {
        SaveObject save;
        if(File.Exists(Application.persistentDataPath + "/Sainthood.save"))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath + "/Sainthood.save", FileMode.Open);

            save = (SaveObject)bf.Deserialize(file);
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

    public void DeleteSave()
    {
        if (File.Exists(Application.persistentDataPath + "/Sainthood.save"))
        {
            File.Delete(Application.persistentDataPath + "/Sainthood.save");
        }
    }
}
