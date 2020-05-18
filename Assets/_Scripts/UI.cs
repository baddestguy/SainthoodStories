using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class UI : MonoBehaviour
{
    public static UI Instance { get; private set; }

    public TextMeshProUGUI EnergyDisplay;
    public TextMeshProUGUI TimeAndDayDisplay;
    public TextMeshProUGUI MessageDisplay;
    public TextMeshProUGUI ReportDisplay;
    public TextMeshProUGUI CPDisplay;
    public TextMeshProUGUI FPDisplay;

    public GameObject ShopUI;
    public GameObject SchoolUI;
    public GameObject HospitalUI;
    public GameObject ChurchUI;
    public GameObject OrphanageUI;
    public GameObject FoodShelterUI;
    public GameObject ClothesBankUI;
    public GameObject EndGameUI;
    public GameObject KitchenUI;

    public static UnityAction<ItemType> BoughtItem;
    public static UnityAction Taught;
    public static UnityAction DeliverBaby;
    public static UnityAction Prayed;
    public static UnityAction Slept;
    public static UnityAction Cooked;
    public static UnityAction<InteractableHouse> Meditate;
    public static UnityAction<InteractableHouse> DeliverItem;
    public static UnityAction<InteractableHouse> Volunteer;

    private bool ClearDisplay;
    private InteractableHouse CurrentHouse;
    private GameObject CurrentActiveUIGameObject;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        Player.OnMoveSuccessEvent += OnPlayerMoved;
        Energy.EnergyConsumed += OnEnergyConsumed;
        MissionManager.MissionComplete += MissionComplete;
        GameClock.Ticked += OnTick;
    }

    public void InitTimeEnergy(GameClock clock, Energy energy)
    {
        EnergyDisplay.text = $"Energy: {energy.Amount}";
        TimeAndDayDisplay.text = $"Day: {clock.Day}, Time: {clock.Time}:00";
    }

    private void OnPlayerMoved(Energy energy, MapTile tile)
    {

    }

    private void OnEnergyConsumed(Energy energy)
    {
        EnergyDisplay.text = $"Energy: {energy.Amount}";
        if(energy.Amount <= 5)
        {
            EnergyDisplay.color = Color.red;
        }
        else
        {
            EnergyDisplay.color = Color.white;
        }
    }

    private void MissionComplete(bool complete)
    {
        if (complete)
        {
            MessageDisplay.text = "Mission Copmlete!!";
        }
        else
        {
            MessageDisplay.text = "ENERGY DEPLETED! RESETTING TO CHURCH";
        }
    }

    private void OnTick(double time, int day)
    {
        if (ClearDisplay)
        {
            ClearDisplay = false;
            DisplayMessage("");
        }
        if (time > 0)
        {
            ReportDisplay.text = "";
        }

        TimeAndDayDisplay.text = $"Day: {day}, Time: {(int)time}:{(time % 1 == 0 ? "00" : "30")}";
    }

    public void EnableCurrentUI(bool enable)
    {
        CurrentActiveUIGameObject.SetActive(enable);
    }

    public void EnableShop(bool enable, InteractableHouse house)
    {
        ShopUI.SetActive(enable);
        if (ShopUI.activeSelf)
        {
            CurrentActiveUIGameObject = ShopUI;
            CurrentHouse = house;
        }
    }

    public void EnableSchool(bool enable, InteractableHouse house)
    {
        SchoolUI.SetActive(enable);
        if (SchoolUI.activeSelf)
        {
            CurrentHouse = house;
            CurrentActiveUIGameObject = SchoolUI;
        }
    }

    public void EnableHospital(bool enable, InteractableHouse house)
    {
        HospitalUI.SetActive(enable);
        if (HospitalUI.activeSelf)
        {
            CurrentHouse = house;
            CurrentActiveUIGameObject = HospitalUI;
        }
    }

    public void EnableChurch(bool enable)
    {
        ChurchUI.SetActive(enable);
        if(ChurchUI.activeSelf) CurrentActiveUIGameObject = ChurchUI;
    }

    public void EnableFood(bool enable, InteractableHouse house)
    {
        FoodShelterUI.SetActive(enable);
        if (FoodShelterUI.activeSelf)
        {
            CurrentActiveUIGameObject = FoodShelterUI;
            CurrentHouse = house;
        }    
    }

    public void EnableClothes(bool enable, InteractableHouse house)
    {
        ClothesBankUI.SetActive(enable);
        if (ClothesBankUI.activeSelf)
        {
            CurrentActiveUIGameObject = ClothesBankUI;
            CurrentHouse = house;
        }
    }

    public void EnableOrphanage(bool enable, InteractableHouse house)
    {
        OrphanageUI.SetActive(enable);
        if (OrphanageUI.activeSelf)
        {
            CurrentActiveUIGameObject = OrphanageUI;
            CurrentHouse = house;
        }
    }

    public void EnableKitchen(bool enable, InteractableHouse house)
    {
        KitchenUI.SetActive(enable);
        if (KitchenUI.activeSelf)
        {
            CurrentActiveUIGameObject = KitchenUI;
            CurrentHouse = house;
        }        
    }

    public void EnableEndGame(bool enable)
    {
        EnableCurrentUI(false);
        EndGameUI.SetActive(enable);
    }

    public void Teach()
    {
        Taught?.Invoke();
    }

    public void Deliver()
    {
        DeliverBaby?.Invoke();
    }

    public void Pray()
    {
        Prayed?.Invoke();
    }

    public void Sleep()
    {
        Slept?.Invoke();
    }

    public void Cook()
    {
        Cooked?.Invoke();
    }

    public void Meditated()
    {
        Meditate?.Invoke(CurrentHouse);
    }

    public void BuyItem(string item)
    {
        ItemType itemType = (ItemType)Enum.Parse(typeof(ItemType), item);
        BoughtItem?.Invoke(itemType);
    }

    public void DeliverItems()
    {
        DeliverItem?.Invoke(CurrentHouse);
    }

    public void VolunteerWork()
    {
        Volunteer?.Invoke(CurrentHouse);
    }

    public void RefreshPoints(int cp, int fp)
    {
        CPDisplay.text = $"CP: {cp}";
        FPDisplay.text = $"FP: {fp}";
    }

    public void DisplayReport(string report)
    {
        ReportDisplay.text += report + '\n';
    }

    public void DisplayMessage(string message)
    {
        ClearDisplay = true;
        MessageDisplay.text = message;
    }

    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
    }

    public void EasyRun()
    {
        GameManager.Instance.SetMissionParameters(MissionDifficulty.EASY);
    }

    public void NormalRun()
    {
        GameManager.Instance.SetMissionParameters(MissionDifficulty.NORMAL);
    }

    public void HardRun()
    {
        GameManager.Instance.SetMissionParameters(MissionDifficulty.HARD);
    }

    private void OnDisable()
    {
        Player.OnMoveSuccessEvent -= OnPlayerMoved;
        Energy.EnergyConsumed -= OnEnergyConsumed;
        MissionManager.MissionComplete -= MissionComplete;
        GameClock.Ticked -= OnTick;
    }
}
