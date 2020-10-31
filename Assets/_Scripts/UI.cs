using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
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
    public Image CPDisplayGlow;
    public Image FPDisplayGlow;
    public Image EnergyDisplayGlow;
    public TextMeshProUGUI FPAdditionDisplay;
    public TextMeshProUGUI CPAdditionDisplay;
    public TextMeshProUGUI EnergyAdditionDisplay;

    public GameObject ShopUI;
    public GameObject SchoolUI;
    public GameObject HospitalUI;
    public GameObject ChurchUI;
    public GameObject OrphanageUI;
    public GameObject FoodShelterUI;
    public GameObject ClothesBankUI;
    public GameObject EndGameUI;
    public GameObject KitchenUI;
    public GameObject WeatherGO;
    public Image WeatherIcon;
    public Image DayNightIcon;
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

    private GameObject BuildingAlertResource;
    public ScrollRect BuildingAlertScroller;
    private Dictionary<string, GameObject> InstantiatedBuildingAlerts = new Dictionary<string, GameObject>();

    public GameObject InventoryUI;
    public GameObject TreasuryUI;
    public TextMeshProUGUI TreasuryAmount;
    public Image TreasuryDisplayGlow;
    public TextMeshProUGUI TreasuryAdditionDisplay;
    public ProvisionsPopup ProvisionPopup;
    public Image Black;

    public TextMeshProUGUI CurrentWeekDisplay;

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
        TreasuryManager.DonatedMoney += RefreshTreasuryBalance;

        SideNotificationResource = Resources.Load("UI/SideNotification") as GameObject;
        BuildingAlertResource = Resources.Load("UI/BuildingIcon") as GameObject;
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
        if (DOTween.IsTweening(EnergyDisplay, true))
        {
            ResetAdditionPoints();
        }

        int oldEnergy = int.Parse(EnergyDisplay.text);
        EnergyDisplay.DOCounter(oldEnergy, energy.Amount, 0.5f).SetDelay(2f);
        if (energy.Amount <= 5)
        {
            EnergyDisplay.color = Color.red;
        }
        else
        {
            EnergyDisplay.color = Color.white;
        }

        StartCoroutine(AdditionPointsAsync(EnergyAdditionDisplay, EnergyDisplayGlow, energy.Amount - oldEnergy, 2f));
    }

    private void MissionComplete(bool complete)
    {
        StartCoroutine(MissionCompleteAsync(complete));
    }

    private IEnumerator MissionCompleteAsync(bool complete)
    {
        while (EventsManager.Instance.HasEventsInQueue())
        {
            yield return null;
        }

        GameManager.Instance.SetMissionParameters(MissionDifficulty.HARD); //Load Next Mission/Week
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

    public void EnableProvisionPopup(ProvisionData prov1, ProvisionData prov2)
    {
        ProvisionPopup.gameObject.SetActive(true);
        ProvisionPopup.Init(prov1, prov2);
    }

    public void BuildingAlertPush(string sprite)
    {
        GameClock c = GameManager.Instance.GameClock;
        if (GameManager.Instance.MissionManager.CurrentMission.CurrentWeek == 1 && c.Day < 4) return;

        if (IsDuplicateBuildingAlert(sprite)) return;

        GameObject BuildAlertGO = Instantiate(BuildingAlertResource);
        BuildAlertGO.transform.SetParent(BuildingAlertScroller.content, false);
        BuildAlertGO.GetComponent<Image>().sprite = Resources.Load<Sprite>($"Icons/{sprite}");
        BuildAlertGO.GetComponent<Image>().color = Color.red;

        InstantiatedBuildingAlerts.Add(sprite, BuildAlertGO);
    }

    public void BuildingAlertPop(string sprite)
    {
        if (BuildingAlertScroller.content.childCount <= 0 || !InstantiatedBuildingAlerts.ContainsKey(sprite)) return;
   
        Destroy(InstantiatedBuildingAlerts[sprite]);
        InstantiatedBuildingAlerts.Remove(sprite);
    }

    private bool IsDuplicateBuildingAlert(string owner)
    {
        if (InstantiatedBuildingAlerts.ContainsKey(owner))
        {
            BuildingAlertPop(owner);
            return false;
        }

        return InstantiatedBuildingAlerts.ContainsKey(owner);
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
        if(weather == WeatherType.PRERAIN || weather == WeatherType.RAIN)
        {
            WeatherGO.SetActive(true);
            WeatherDisplay.text = clock >= start ? "" : $"{(int)start.Time}:{(start.Time % 1 == 0 ? "00" : "30")}";
            WeatherIcon.sprite = Resources.Load<Sprite>($"Icons/{weather}");
        }
        else
        {
            WeatherGO.SetActive(false);
            DayNightIcon.sprite = Resources.Load<Sprite>($"Icons/{weather}");
        }
    }

    public void EventAlert(CustomEventData customEvent)
    {
        CustomEventPopup.gameObject.SetActive(true);
        CustomEventPopup.Setup(customEvent);
    }

    public void EnableTreasuryUI(bool enable)
    {
        if (GameSettings.Instance.FTUE)
        {
            GameClock c = GameManager.Instance.GameClock;
            if (GameManager.Instance.MissionManager.CurrentMission.CurrentWeek == 1 && c.Day == 1 && c.Time < 13.5) return;
        }
        TreasuryUI.SetActive(enable);
        TreasuryAdditionDisplay.text = "";
    }

    public void RefreshTreasuryBalance(double delta)
    {
        if (DOTween.IsTweening(TreasuryAmount, true))
        {
            TreasuryAmount.DOComplete();
            StopAllCoroutines();
            TreasuryAdditionDisplay.text = "";
        }

        int oldAmount = int.Parse(TreasuryAmount.text);
        TreasuryAmount.DOCounter(oldAmount, (int)TreasuryManager.Instance.Money, 0.5f).SetDelay(2f);

        StartCoroutine(AdditionPointsAsync(TreasuryAdditionDisplay, TreasuryDisplayGlow, (int)delta, 2f));
    }

    public void EnableInventoryUI(bool enable)
    {
        if (GameSettings.Instance.FTUE)
        {
            GameClock c = GameManager.Instance.GameClock;
            if (GameManager.Instance.MissionManager.CurrentMission.CurrentWeek == 1 && c.Day == 1 && c.Time < 13.5) return;
        }
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
        if (DOTween.IsTweening(CPDisplay, true) || DOTween.IsTweening(FPDisplay, true))
        {
            ResetAdditionPoints();
        }

        int oldFP = int.Parse(FPDisplay.text);
        int oldCP = int.Parse(CPDisplay.text);
        CPDisplay.DOCounter(oldCP, cp, 0.5f).SetDelay(2f);
        FPDisplay.DOCounter(oldFP, fp, 0.5f).SetDelay(2f);
        
        StartCoroutine(AdditionPointsAsync(CPAdditionDisplay, CPDisplayGlow, cp-oldCP, 2f));
        StartCoroutine(AdditionPointsAsync(FPAdditionDisplay, FPDisplayGlow, fp-oldFP, 2f));

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

    public IEnumerator AdditionPointsAsync(TextMeshProUGUI display, Image glow, int amount, float delay)
    {
        if (amount == 0) yield break;

        glow.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);
        glow.color = Color.white;
        glow.transform.DOScale(Vector3.one, 0.5f);
        glow.DOFade(0, 1f);

        display.text = amount > 0 ? $"+{amount}" : $"{amount}";
        display.color = amount > 0 ? Color.green : Color.red;
        display.DOFade(1, 0.5f);
        yield return new WaitForSeconds(delay);
        display.DOFade(0, 0.5f);
    }

    public void ResetAdditionPoints()
    {
        CPDisplay.DOComplete();
        FPDisplay.DOComplete();
        EnergyDisplay.DOComplete();
        StopAllCoroutines();
        FPAdditionDisplay.text = "";
        CPAdditionDisplay.text = "";
        EnergyAdditionDisplay.text = "";
    }

    public void DisplayReport(string report)
    {
        ReportDisplay.text += report + '\n';
    }

    public void DisplayMessage(string message)
    {
        ClearDisplay = true;
    //    MessageDisplay.text = message;
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

    public void HardRun(bool newGame)
    {
        GameManager.Instance.SetMissionParameters(MissionDifficulty.HARD, newGame);
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

    public void ShowWeekBeginText()
    {
        StartCoroutine(ShowWeekBeginTextAsync());
    }

    private IEnumerator ShowWeekBeginTextAsync()
    {
        yield return StartCoroutine(CrossFadeAsync(1, 10));

        CrossFading = true;
        CurrentWeekDisplay.gameObject.SetActive(true);
        CurrentWeekDisplay.text = "WEEK " + MissionManager.Instance.CurrentMission.CurrentWeek; //TODO: Localize

        yield return new WaitForSeconds(3.5f);
        CurrentWeekDisplay.text = "";
        CurrentWeekDisplay.gameObject.SetActive(false);
        yield return new WaitForSeconds(2f);
        
        CrossFade(0);
    }

    public void CrossFade(float fade, float speed = 5f)
    {
        StartCoroutine(CrossFadeAsync(fade, speed));
    }

    public bool CrossFading;
    private IEnumerator CrossFadeAsync(float fade, float speed)
    {
        Black.gameObject.SetActive(true);
        Color c = Black.color;

        while(Math.Abs(c.a - fade) > 0.01f)
        {
            c.a = Mathf.Lerp(c.a, fade, Time.deltaTime * speed);
            Black.color = c;
            yield return null;
        }

        c.a = fade;
        Black.color = c;
        Black.gameObject.SetActive(fade != 0);
        CrossFading = false;
    }

    private void OnDisable()
    {
        Player.OnMoveSuccessEvent -= OnPlayerMoved;
        Energy.EnergyConsumed -= OnEnergyConsumed;
        MissionManager.MissionComplete -= MissionComplete;
        WeatherManager.WeatherForecastActive -= WeatherAlert;
        GameClock.Ticked -= OnTick;
        TreasuryManager.DonatedMoney -= RefreshTreasuryBalance;
    }
}
