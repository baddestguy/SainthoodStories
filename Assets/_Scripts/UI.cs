using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UI : MonoBehaviour
{
    public static UI Instance { get; private set; }

    public TextMeshProUGUI EnergyDisplay;
    public TextMeshProUGUI TimeDisplay;
    public TextMeshProUGUI DayDisplay;
    public TextMeshProUGUI MessageDisplay;
    public TextMeshProUGUI ReportDisplay;
    public TextMeshProUGUI CPDisplay;
    public TextMeshProUGUI FPDisplay;
    public TextMeshProUGUI WeatherDisplay;

    public GameObject ShopUI;
    public GameObject SchoolUI;
    public GameObject HospitalUI;
    public GameObject ChurchUI;
    public GameObject OrphanageUI;
    public GameObject FoodShelterUI;
    public GameObject ClothesBankUI;
    public GameObject EndGameUI;
    public GameObject KitchenUI;
    public Image WeatherIcon;
    public CustomEventPopup CustomEventPopup;

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
    private GameObject SideNotificationResource;
    public ScrollRect SideNotificationScroller;
    private Dictionary<string, Tuple<GameClock, GameObject, int>> InstantiatedSideNotifications = new Dictionary<string, Tuple<GameClock, GameObject, int>>();

    public GameObject InventoryUI;

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
        WeatherManager.WeatherForecastActive += WeatherAlert;

        SideNotificationResource = Resources.Load("UI/SideNotification") as GameObject;
    }

    public void InitTimeEnergy(GameClock clock, Energy energy)
    {
        EnergyDisplay.text = $"{energy.Amount}";
        TimeDisplay.text = $"{(int)clock.Time}:{(clock.Time % 1 == 0 ? "00" : "30")}";
        DayDisplay.text = DayofTheWeek(clock.Day);
    }

    private void OnPlayerMoved(Energy energy, MapTile tile)
    {

    }

    private void OnEnergyConsumed(Energy energy)
    {
        EnergyDisplay.text = $"{energy.Amount}";
        if (energy.Amount <= 5)
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
            MessageDisplay.text = "Mission Complete!!";
        }
        else
        {
            MessageDisplay.text = "Energy Depleted! Mission Failed!";
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

        TimeDisplay.text = $"{(int)time}:{(time % 1 == 0 ? "00" : "30")}";
        DayDisplay.text = DayofTheWeek(day);
    }

    public void SideNotificationPush(string sprite, int items, GameClock deadline, string owner)
    {
        if (IsDuplicateSideNotification(owner, items, deadline)) return;

        GameObject SideNotifGO = Instantiate(SideNotificationResource);
        SideNotifGO.transform.SetParent(SideNotificationScroller.content, false);
        SideNotifGO.GetComponent<SideNotification>().Init(sprite, items, deadline);

        InstantiatedSideNotifications.Add(owner, new Tuple<GameClock, GameObject, int>(deadline, SideNotifGO, items));
        SortSideNotifications();
    }

    public void SideNotificationPop(string owner)
    {
        if (SideNotificationScroller.content.childCount <= 0 || !InstantiatedSideNotifications.ContainsKey(owner)) return;
        
        Destroy(InstantiatedSideNotifications[owner].Item2);
        InstantiatedSideNotifications.Remove(owner);
    }

    private void SortSideNotifications()
    {
        var unsortedList = InstantiatedSideNotifications.ToList();
        for(int i = 0; i < unsortedList.Count; i++)
        {
            int min = i;
            for(int j = i+1; j < unsortedList.Count; j++)
            {
                if(unsortedList[j].Value.Item1 < unsortedList[min].Value.Item1)
                {
                    min = j;
                }
            }

            if(min != i)
            {
                var temp = unsortedList[i];
                unsortedList[i] = unsortedList[min];
                unsortedList[min] = temp;
            }
        }

        for(int i = unsortedList.Count-1; i >= 0; i--)
        {
            unsortedList[i].Value.Item2.transform.SetAsFirstSibling();
        }
    }

    private bool IsDuplicateSideNotification(string owner, int items, GameClock deadline)
    {
        if(InstantiatedSideNotifications.ContainsKey(owner) && (InstantiatedSideNotifications[owner].Item3 != items || InstantiatedSideNotifications[owner].Item1 != deadline))
        {
            SideNotificationPop(owner);
            return false;
        }

        return InstantiatedSideNotifications.ContainsKey(owner);
    }

    public void WeatherAlert(WeatherType weather, GameClock start, GameClock end)
    {
        GameClock clock = GameManager.Instance.GameClock;
        WeatherDisplay.text = clock >= start ? "" : $"{(int)start.Time}:{(start.Time % 1 == 0 ? "00" : "30")}";
        WeatherIcon.sprite = Resources.Load<Sprite>($"Icons/{weather}");
    }

    public void EventAlert(CustomEventData customEvent)
    {
        CustomEventPopup.gameObject.SetActive(true);
        CustomEventPopup.Setup(customEvent);
    }

    public void EnableInventoryUI(bool enable)
    {
        InventoryUI.SetActive(enable);
    }

    public void EnableCurrentUI(bool enable)
    {
        if (CurrentActiveUIGameObject == null) return;
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
        CPDisplay.text = $"{cp}";
        FPDisplay.text = $"{fp}";

        if (cp < 50)
        {
            CPDisplay.color = Color.red;
        }
        else if (cp < 75)
        {
            CPDisplay.color = Color.yellow;
        }
        else
        {
            CPDisplay.color = Color.green;
        }

        if (fp < 50)
        {
            FPDisplay.color = Color.red;
        }
        else if (fp < 75)
        {
            FPDisplay.color = Color.yellow;
        }
        else
        {
            FPDisplay.color = Color.green;
        }
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

    private string DayofTheWeek(int Day)
    {
        switch (Day)
        {
            case 1: return "Mon";
            case 2: return "Tues";
            case 3: return "Wed";
            case 4: return "Thurs";
            case 5: return "Fri";
            case 6: return "Sat";
            case 7: return "Sun";
        }
        return "";
    }

    private void OnDisable()
    {
        Player.OnMoveSuccessEvent -= OnPlayerMoved;
        Energy.EnergyConsumed -= OnEnergyConsumed;
        MissionManager.MissionComplete -= MissionComplete;
        WeatherManager.WeatherForecastActive -= WeatherAlert;
        GameClock.Ticked -= OnTick;
    }
}
