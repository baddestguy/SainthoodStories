using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class UI : MonoBehaviour
{
    public static UI Instance { get; private set; }

    public TextMeshProUGUI EnergyDisplay;
    public TextMeshProUGUI TimeAndDayDisplay;
    public TextMeshProUGUI MessageDisplay;
    public TextMeshProUGUI ReportDisplay;
    public TextMeshProUGUI TPDisplay;
    public TextMeshProUGUI FPDisplay;

    public GameObject ShopUI;
    public GameObject SchoolUI;
    public GameObject HospitalUI;
    public GameObject ChurchUI;
    public GameObject OrphanageUI;
    public GameObject FoodShelterUI;
    public GameObject ClothesBankUI;

    public static UnityAction BoughtFood;
    public static UnityAction BoughtClothes;
    public static UnityAction BoughtToys;
    public static UnityAction Taught;
    public static UnityAction DeliverBaby;
    public static UnityAction Prayed;
    public static UnityAction Slept;
    public static UnityAction DonatedToys;
    public static UnityAction DonatedFood;
    public static UnityAction DonatedClothes;
    public static UnityAction<InteractableHouse> Meditate;

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
        CurrentHouse = house;
        ShopUI.SetActive(enable);
        CurrentActiveUIGameObject = ShopUI;
    }

    public void EnableSchool(bool enable, InteractableHouse house)
    {
        CurrentHouse = house;
        SchoolUI.SetActive(enable);
        CurrentActiveUIGameObject = SchoolUI;
    }

    public void EnableHospital(bool enable, InteractableHouse house)
    {
        CurrentHouse = house;
        HospitalUI.SetActive(enable);
        CurrentActiveUIGameObject = HospitalUI;
    }

    public void EnableChurch(bool enable)
    {
        ChurchUI.SetActive(enable);
        CurrentActiveUIGameObject = ChurchUI;
    }

    public void EnableFood(bool enable, InteractableHouse house)
    {
        CurrentHouse = house;
        FoodShelterUI.SetActive(enable);
        CurrentActiveUIGameObject = FoodShelterUI;
    }

    public void EnableClothes(bool enable, InteractableHouse house)
    {
        CurrentHouse = house;
        ClothesBankUI.SetActive(enable);
        CurrentActiveUIGameObject = ClothesBankUI;
    }

    public void EnableOrphanage(bool enable, InteractableHouse house)
    {
        CurrentHouse = house;
        OrphanageUI.SetActive(enable);
        CurrentActiveUIGameObject = OrphanageUI;
    }

    public void BuyFood()
    {
        BoughtFood?.Invoke();
    }

    public void BuyClothes()
    {
        BoughtClothes?.Invoke();
    }

    public void BuyToys()
    {
        BoughtToys?.Invoke();
    }

    public void Teach()
    {
        Taught?.Invoke();
    }

    public void Deliver()
    {
        DeliverBaby?.Invoke();
    }

    public void DonateToys()
    {
        DonatedToys?.Invoke();
    }
    
    public void DonateFood()
    {
        DonatedFood?.Invoke();
    }

    public void DonateClothes()
    {
        DonatedClothes?.Invoke();
    }

    public void Pray()
    {
        Prayed?.Invoke();
    }

    public void Sleep()
    {
        Slept?.Invoke();
    }

    public void Meditated()
    {
        Meditate?.Invoke(CurrentHouse);
    }

    public void RefreshPoints(int tp, int fp)
    {
        TPDisplay.text = $"TP: {tp}";
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
}
