using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class UI : MonoBehaviour
{
    public static UI Instance { get; private set; }

    public TextMeshProUGUI EnergyDisplay;
    public TextMeshProUGUI TimeAndDayDisplay;
    public TextMeshProUGUI MessageDisplay;

    public GameObject ShopUI;
    public GameObject SchoolUI;
    public GameObject HospitalUI;
    public GameObject ChurchUI;

    public static UnityAction BoughtFood;
    public static UnityAction BoughtClothes;
    public static UnityAction Taught;
    public static UnityAction DeliverBaby;
    public static UnityAction Prayed;
    public static UnityAction Slept;

    private bool ClearDisplay;

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

    private void OnPlayerMoved(Energy energy, MapTile tile)
    {

    }

    private void OnEnergyConsumed(Energy energy)
    {
        EnergyDisplay.text = $"Energy: {energy.Amount}";
    }

    private void MissionComplete(bool complete)
    {
        if (complete)
        {
            MessageDisplay.text = "Mission Copmlete!!";
        }
        else
        {
            MessageDisplay.text = "Mission Failed!";
        }
    }

    private void OnTick(int time, int day)
    {
        if (ClearDisplay)
        {
            ClearDisplay = false;
            DisplayMessage("");
        }

        TimeAndDayDisplay.text = $"Day:{day}, Time: {time}:00";
    }

    public void EnableShop(bool enable)
    {
        ShopUI.SetActive(enable);
    }

    public void EnableSchool(bool enable)
    {
        SchoolUI.SetActive(enable);
    }

    public void EnableHospital(bool enable)
    {
        HospitalUI.SetActive(enable);
    }
    public void EnableChurch(bool enable)
    {
        ChurchUI.SetActive(enable);
    }

    public void BuyFood()
    {
        BoughtFood?.Invoke();
    }

    public void BuyClothes()
    {
        BoughtClothes.Invoke();
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

    public void DisplayMessage(string message)
    {
        ClearDisplay = true;
        MessageDisplay.text = message;
    }
}
