using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static UnityEngine.UI.CanvasScaler;

public class UI : MonoBehaviour
{
    public static UI Instance { get; private set; }

    public TextMeshProUGUI EnergyDisplay;
    public Image EnFillBar;
    public TextMeshProUGUI TimeHrDisplay;
    public TextMeshProUGUI TimeMinDisplay;
    public TextMeshProUGUI DayDisplay;
    public TextMeshProUGUI MessageDisplay;
    public TextMeshProUGUI ReportDisplay;
    public TextMeshProUGUI CPDisplay;
    public Image CPFillBar;
    public TextMeshProUGUI FPDisplay;
    public Image FPFillBar;
    public TextMeshProUGUI WeatherDisplay;
    public Image CPDisplayGlow;
    public Image FPDisplayGlow;
    public Image EnergyDisplayGlow;
    public TextMeshProUGUI FPAdditionDisplay;
    public TextMeshProUGUI CPAdditionDisplay;
    public TextMeshProUGUI EnergyAdditionDisplay;

    public GameObject WeatherGO;
    public Image WeatherIcon;
    public Image DayNightIcon;
    public CustomEventPopup CustomEventPopup;

    public static UnityAction<InteractableHouse> Meditate;

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
    public GameObject PackageSelectorUI;
    public GameObject TreasuryUI;
    public TextMeshProUGUI TreasuryAmount;
    public Image TreasuryDisplayGlow;
    public TextMeshProUGUI TreasuryAdditionDisplay;
    public TextMeshProUGUI WanderingSpiritsAmount;
    public Image WanderingSpiritsDisplayGlow;
    public TextMeshProUGUI WanderingSpiritsAdditionDisplay;

    public ProvisionsPopup ProvisionPopup;
    public Image Black;

    public Image CurrentWeekIntroGraphic;
    public Image WeekIntroBGGraphic;
    public TextMeshProUGUI CurrentWeekDisplay;
    public TextMeshProUGUI TooltipDisplay;
    public TextSizer TooltipSizer;

    public TutorialPopup TutorialPopup;

    public GameObject SaintCard;

    public GameObject PanelToActivateOnLoadUiEvent;
    public static UnityAction<bool> UIHidden;
    public bool FullUIVisible = true;

    [Header("UI Elements")]
    public GameObject LeftItems;
    public GameObject RightItems;
    public GameObject CenterItems;
    public GameObject SideNotifItems;

    [Header("Ui Clicked Reporters")]
    public GraphicRaycaster[] graphicRaycaster;
    public EventSystem m_EventSystem;
    private PointerEventData m_PointerEventData;

    public StatusEffectDisplay StatusEffectDisplay;
    public TextMeshProUGUI RunAttemptsDisplay;
    public Button ContinueBtn;

    public MinigamePlayer MinigamePlayer;
    public GameObject SaintsCollectionUI;

    public GameObject LoadingScreen;
    public GameObject GameOverPopup;
    public GameObject InventoryPopup;
    public GameObject SacredItemPopup;
    public GameObject TutorialPopupQuestion;
    public bool WasUiHit
    {
        get
        {
            m_PointerEventData = new PointerEventData(m_EventSystem);
            m_PointerEventData.position = Input.mousePosition;
            List<RaycastResult> results = new List<RaycastResult>();
            if (graphicRaycaster != null)
            {
                for (int i = 0; i < graphicRaycaster.Length; i++)
                {
                    graphicRaycaster[i].Raycast(m_PointerEventData, results);
                }
                foreach (RaycastResult result in results)
                {
                    if (result.gameObject.layer == LayerMask.NameToLayer("UI") || result.gameObject.CompareTag("UI"))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

    }


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
        if (SceneManager.GetActiveScene().name.Contains("Level"))
        {
            GetComponent<Canvas>().worldCamera = GameObject.Find("2DUICam").GetComponent<Camera>();
        }
    }

    private void Update()
    {
        if (Gamepad.current != null && Gamepad.current.buttonEast.wasPressedThisFrame)
        {
            if (StatusEffectDisplay != null && StatusEffectDisplay.gameObject.activeSelf)
            {
                StatusEffectDisplay.gameObject.SetActive(false);
            }
        }
    }

    public void InventoryPopupEnable()
    {
        if (!FullUIVisible && !InventoryPopup.activeSelf) return;
        InventoryPopup.SetActive(!InventoryPopup.activeSelf);
    }

    public void SacredItemPopupEnable(string item)
    {
        if (!FullUIVisible && !SacredItemPopup.activeSelf) return;
        EnableAllUIElements(false);
        SacredItemPopup.SetActive(!SacredItemPopup.activeSelf);
        SacredItemPopup.GetComponent<SacredItemPopup>().Init(item);
    }

    public void LoadingScreenEnable(bool enable)
    {
        LoadingScreen.SetActive(enable);
    }

    public void ToggleSaintsCollection(bool enable)
    {
        BroadcastMessage("HideInfoPanel", SendMessageOptions.DontRequireReceiver);
        SaintsCollectionUI.SetActive(enable);
    }

    public void InitTimeEnergy(GameClock clock, Energy energy)
    {
        EnergyDisplay.text = $"{energy.Amount}";
        TimeDisplay(clock.Time);
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
        var bonusEnergy = InventoryManager.Instance.GetProvision(Provision.ENERGY_DRINK)?.Energy ?? 0;
        EnFillBar.DOFillAmount(energy.Amount / (3f + bonusEnergy), 1f).SetDelay(2f);

        if (energy.Amount < 1)
        {
            EnergyDisplay.color = Color.red;
            EnergyDisplayGlow.color = Color.red;
            SoundManager.Instance.PlayOneShotSfx("LowEnergy_SFX");
        }
        else
        {
            EnergyDisplay.color = new Color32(0xFF, 0xFB, 0xE7, 0xFF);
            EnergyDisplayGlow.color = Color.white;
        }

        AdditionPoints(EnergyAdditionDisplay, EnergyDisplayGlow, energy.Amount - oldEnergy, 2f);
    }

    private IEnumerator UnlockSaintSequence()
    {
        SoundManager.Instance.PlayOneShotSfx("SaintUnlock_SFX", timeToDie: 10f);
        SaintCard.SetActive(true);
        SaintCard.GetComponent<SaintCard>().Init(SaintsManager.Instance.NewSaint);
        yield return new WaitForSeconds(10);
        SaintCard.SetActive(false);
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
        EventsManager.Instance.CurrentEvents.Clear();
        GameManager.Instance.SetMissionParameters(MissionDifficulty.HARD, showUI: complete);
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

        TimeDisplay(time);
        DayDisplay.text = DayofTheWeek(day);

        //Tutorial Checks
        TutorialHideUI();
    }

    public void TimeDisplay(double time)
    {
        var currentMinute = int.Parse(TimeMinDisplay.text);
        var currentHour = int.Parse(TimeHrDisplay.text);

        var newMinute = 0;
        var newHour = (int)time;
        if (time - (int)time == 0) newMinute = 0;
        if (time - (int)time == 0.25) newMinute = 15;
        if (time - (int)time == 0.5) newMinute = 30;
        if (time - (int)time == 0.75) newMinute = 45;

        TimeHrDisplay.DOCounter(currentHour, newHour, 1f, "{0:D2}", false);
        TimeMinDisplay.DOCounter(currentMinute, newMinute, 1f, "{0:D2}", false);

        if(newHour > 21)
        {
            TimeHrDisplay.color = Color.red;
            TimeMinDisplay.color = Color.red;
            DayDisplay.color = Color.red;
        }
    }

    public void EnableProvisionPopup(ProvisionData prov1, ProvisionData prov2)
    {
        ProvisionPopup.gameObject.SetActive(true);
        ProvisionPopup.Init(prov1, prov2);
        ProvisionPopup.House = GameManager.Instance.CurrentHouse;
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
        return;
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
        if(weather != WeatherType.NONE)
        {
        //    WeatherGO.SetActive(true);
            WeatherDisplay.text = clock >= start ? "" : $"{(int)start.Time}:{(start.Time % 1 == 0 ? "00" : "30")}";
            
            switch (MissionManager.Instance.CurrentMission.Season) {
                case Season.SPRING:
                    WeatherIcon.sprite = Resources.Load<Sprite>($"Icons/Hail");
                    break;
                    //WeatherIcon.sprite = Resources.Load<Sprite>($"Icons/Heatwave");
                    //break;
                case Season.SUMMER:
                case Season.FALL:
                    WeatherIcon.sprite = Resources.Load<Sprite>($"Icons/Rain");
                    break;
                case Season.WINTER:
                    WeatherIcon.sprite = Resources.Load<Sprite>($"Icons/Snow");
                    break;
            }
        }
        else
        {
            WeatherGO.SetActive(false);
        }
    }

    public void UpdateDayNightIcon(DayNight dayNight)
    {
        if (WeatherManager.Instance.IsStormy()) return;
        if(DayNightIcon != null)
        {
            DayNightIcon.sprite = Resources.Load<Sprite>($"Icons/{dayNight}");
        }
    }

    public void HideUIItemsExcept(List<string> items)
    {
        LeftItems.SetActive(false);
        foreach (Transform g in LeftItems.transform)
        {
            g.gameObject.SetActive(false);
        }
        RightItems.SetActive(false);
        foreach (Transform g in RightItems.transform)
        {
            g.gameObject.SetActive(false);
        }
        CenterItems.SetActive(false);

        foreach (var item in items)
        {
            foreach (Transform t in RightItems.transform)
            {
                if (t.name == item)
                {
                    RightItems.SetActive(true);
                    if(item != "TreasuryBalance" && item != "Spirits")
                        RightItems.transform.Find("Image").gameObject.SetActive(true);
                    t.gameObject.SetActive(true);
                }
            }
            foreach (Transform t in LeftItems.transform)
            {
                if (t.name == item)
                {
                    LeftItems.SetActive(true);
                    t.gameObject.SetActive(true);
                }
            }
        }
    }

    public void EventAlert(CustomEventData customEvent)
    {
        CustomEventPopup.gameObject.SetActive(true);
        CustomEventPopup.Setup(customEvent);

        //Hide/Show relevant UI
        switch (customEvent.EventGroup)
        {
            case EventGroup.THANKYOU:
                RightItems.SetActive(true);
                foreach(Transform t in RightItems.transform)
                {
                    if(customEvent.RewardType == CustomEventRewardType.COIN && t.name == "TreasuryBalance")
                    {
                        t.gameObject.SetActive(true);
                        TreasuryManager.DonatedMoney?.Invoke(TreasuryManager.Instance.TemporaryMoneyToDonate);
                    }
                    if (customEvent.RewardType == CustomEventRewardType.CP && t.name == "CP")
                    {
                        t.gameObject.SetActive(true);
                    }
                    if (customEvent.RewardType == CustomEventRewardType.FP && t.name == "FP")
                    {
                        t.gameObject.SetActive(true);
                    }
                }
                break;

            case EventGroup.CHURCH:
                CenterItems.SetActive(true);
                RightItems.SetActive(true);
                foreach (Transform t in RightItems.transform)
                {
                    if (t.name == "FP")
                    {
                        t.gameObject.SetActive(true);
                    }
                }
                LeftItems.SetActive(true);
                foreach (Transform t in LeftItems.transform)
                {
                    if (t.name == "Time")
                    {
                        t.gameObject.SetActive(true);
                    }
                }
                break;

            case EventGroup.KITCHEN:
            case EventGroup.SHELTER:
            case EventGroup.HOSPITAL:
            case EventGroup.ORPHANAGE:
            case EventGroup.SCHOOL:
            case EventGroup.CLOTHES:
            case EventGroup.SAVE_HOSPITAL:
            case EventGroup.SAVE_KITCHEN:
            case EventGroup.SAVE_ORPHANAGE:
            case EventGroup.SAVE_SCHOOL:
            case EventGroup.SAVE_SHELTER:

                CenterItems.SetActive(true);
                RightItems.SetActive(true);
                foreach (Transform t in RightItems.GetComponentsInChildren<Transform>(true))
                {
                    if (customEvent.RewardType == CustomEventRewardType.COIN && t.name == "TreasuryBalance")
                    {
                        t.gameObject.SetActive(true);
                    }
                    else if (customEvent.RewardType != CustomEventRewardType.COIN && t.name == "CP")
                    {
                        t.gameObject.SetActive(true);
                    }
                }
                LeftItems.SetActive(true);
                foreach (Transform t in LeftItems.transform)
                {
                    if (t.name == "Time")
                    {
                        t.gameObject.SetActive(true);
                    }
                }
                break;

            case EventGroup.DAILY:
                SoundManager.Instance.PlayOneShotSfx("TownCrier_SFX", timeToDie: 5f);
                break;
        }
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

        int oldAmount = int.Parse(TreasuryAmount.text, System.Globalization.NumberStyles.AllowLeadingSign | System.Globalization.NumberStyles.AllowThousands);
        TreasuryAmount.DOCounter(oldAmount, (int)TreasuryManager.Instance.Money, 0.5f).SetDelay(2f);

        AdditionPoints(TreasuryAdditionDisplay, TreasuryDisplayGlow, (int)delta, 2f);
    }

    public void RefreshWanderingSpiritsBalance(double delta)
    {
        if (DOTween.IsTweening(WanderingSpiritsAmount, true))
        {
            WanderingSpiritsAmount.DOComplete();
            StopAllCoroutines();
            WanderingSpiritsAdditionDisplay.text = "";
        }

        int oldAmount = int.Parse(WanderingSpiritsAmount.text, System.Globalization.NumberStyles.AllowLeadingSign | System.Globalization.NumberStyles.AllowThousands);
        WanderingSpiritsAmount.DOCounter(oldAmount, (int)InventoryManager.Instance.WanderingSpirits, 0.5f).SetDelay(2f);

        AdditionPoints(WanderingSpiritsAdditionDisplay, WanderingSpiritsDisplayGlow, (int)delta, 2f);

    }

    public void EnablePackageSelector(bool enable, InteractableHouse house = null)
    {
        EnableAllUIElements(!enable);

        PackageSelectorUI.SetActive(enable);
        if(house != null)
        {
            PackageSelectorUI.GetComponent<PackageSelector>().House = house;
        }
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

    public void Meditated()
    {
        Meditate?.Invoke(CurrentHouse);
    }

    int CachedCpAddition = 0;
    public void RefreshCP(int delta, int newCp)
    {
        int oldCP = int.Parse(CPDisplay.text);
        CachedCpAddition += delta;
        if (DOTween.IsTweening(CPDisplay, true))
        {
            ResetAdditionPoints();
        }

        CPDisplay.DOCounter(oldCP, newCp, 0.5f).SetDelay(2f);
        CPFillBar.DOFillAmount(newCp / 5f, 1f).SetDelay(2f);

        if (Mathf.Abs(newCp - oldCP) > 0)
        {
            if (newCp < 1)

            {
                CPDisplayGlow.color = Color.red;
                SoundManager.Instance.PlayOneShotSfx("LowEnergy_SFX");
            }
            else
            {
                CPDisplayGlow.color = Color.white;
            }
        }
        
        if (Mathf.Abs(newCp - oldCP) > 0) AdditionPoints(CPAdditionDisplay, CPDisplayGlow, CachedCpAddition, 2f);
        
        if (newCp < 1)
        {
            CPDisplay.color = Color.red;
        }
        else
        {
            CPDisplay.color = new Color32(0xFF, 0xFB, 0xE7, 0xFF);
        }
        //else if (newCp < 4)
        //{
        //    CPDisplay.color = Color.yellow;
        //}
        //else
        //{
        //    CPDisplay.color = Color.green;
        //}

        StartCoroutine(ResetCachedCP());
    }

    IEnumerator ResetCachedCP()
    {
        yield return new WaitForSeconds(2f);
        CachedCpAddition = 0;
    }

    public void RefreshFP(int fp)
    {
        int oldFP = int.Parse(FPDisplay.text);
        int fpAmount = fp - oldFP;

        if (DOTween.IsTweening(FPDisplay, true))
        {
            fpAmount = int.Parse(FPAdditionDisplay.text);
            ResetAdditionPoints();
        }

        FPDisplay.DOCounter(oldFP, fp, 0.5f).SetDelay(2f);
        FPFillBar.DOFillAmount(fp/5f, 1f).SetDelay(2f);
        if (Mathf.Abs(fp - oldFP) > 0)
        {
            if (fp < 1)
            {
                FPDisplayGlow.color = Color.red;
                SoundManager.Instance.PlayOneShotSfx("LowEnergy_SFX");
            }
            else
            {
                FPDisplayGlow.color = Color.white;
            }
        }

        if (Mathf.Abs(fp - oldFP) > 0) AdditionPoints(FPAdditionDisplay, FPDisplayGlow, fpAmount, 2f);

        if (fp < 1)
        {
            FPDisplay.color = Color.red;
        }
        else
        {
            FPDisplay.color = new Color32(0xFF, 0xFB, 0xE7, 0xFF);
        }
        //else if (fp < 4)
        //{
        //    FPDisplay.color = Color.yellow;
        //}
        //else
        //{
        //    FPDisplay.color = Color.green;
        //}
    }

    public void AdditionPoints(TextMeshProUGUI display, Image glow, int amount, float delay)
    {
        glow.transform.localScale = Vector3.one;
        glow.transform.DOScale(new Vector3(2f, 2f, 2f), 0.5f);
        glow.DOFade(0, 1f);

        if (amount == 0) return;

        display.text = amount > 0 ? $"+{amount}" : $"{amount}";
        display.color = amount > 0 ? Color.green : Color.red;
        display.DOFade(1, 0.5f);
        display.transform.GetChild(0).gameObject.SetActive(true);
        display.DOFade(0, 0.5f).SetDelay(delay);
    }

    public void ErrorFlash(string displayGlow)
    {
        SoundManager.Instance.PlayOneShotSfx("LowEnergy_SFX");
        switch (displayGlow) {
            case "Energy":
                ErrorFlash(EnergyDisplayGlow);
                break;
        }

    }

    private void ErrorFlash(Image glow)
    {
        glow.color = Color.red;
        glow.transform.localScale = Vector3.one;
        glow.transform.DOScale(new Vector3(2f, 2f, 2f), 0.5f);
        glow.DOFade(0, 1f);
    }

    public void ResetAdditionPoints()
    {
        CPDisplay.DOComplete();
        FPDisplay.DOComplete();
        EnergyDisplay.DOComplete();
        CPDisplayGlow.transform.localScale = Vector3.one;
        FPDisplayGlow.transform.localScale = Vector3.one;
        EnergyDisplayGlow.transform.localScale = Vector3.one;
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

    public void TutorialPopupOn(string locKey)
    {
        UI.Instance.EnableAllUIElements(false);
        Black.gameObject.SetActive(true);
        Color c = Black.color;
        c.a = 0.5f;
        Black.color = c;
        TutorialPopup.gameObject.SetActive(true);
        TutorialPopup.Display(locKey);
    }

    public void TutorialPopupOff()
    {
        UI.Instance.EnableAllUIElements(true);
        Color c = Black.color;
        c.a = 0f;
        Black.color = c;
        Black.gameObject.SetActive(false);
    }

    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
    }

    public void EasyRun()
    {
    }

    public void NormalRun()
    {
    }

    public void TutorialConfirmation(string response)
    {
        switch (response)
        {
            case "YES":
                TutorialPopupQuestion.SetActive(false);
                GameManager.Instance.SetMissionParameters(MissionDifficulty.HARD, true);
                GameSettings.Instance.TUTORIAL_MODE = true;
                break;

            case "NO":
                TutorialPopupQuestion.SetActive(false);
                GameManager.Instance.SetMissionParameters(MissionDifficulty.HARD, true);
                break;

            case "CLOSE":
                TutorialPopupQuestion.SetActive(false);
                break;
        }
    }

    public void HardRun(bool newGame)
    {
        if (newGame)
        {
            TutorialPopupQuestion.SetActive(true);
        }
        else
        {
            GameManager.Instance.SetMissionParameters(MissionDifficulty.HARD, newGame);
        }
    }

    public void DisableMainMenuContinueBtn()
    {
        ContinueBtn.interactable = false;
    }

    public GameObject SettingsMenuGO;
    public void SettingsMenu()
    {
        PauseMenu.Instance.Activate();
        SoundManager.Instance.PlayOneShotSfx("Button_SFX");
    }
    public void CloseSettingsMenu()
    {
        SoundManager.Instance.PlayOneShotSfx("Button_SFX");
    }

    public TMP_Dropdown Dropdown;
    public void PopulateLanguageDropdown()
    {
        Dropdown.AddOptions(new List<string>() { "English", "Français", "Português do Brasil", "Tagalog", "Español latinoamericano", "Italiano", "Deutsche" });
        Dropdown.value = PlayerPrefs.GetInt("Language");
        Dropdown.onValueChanged.AddListener(delegate {
            ChooseLanguage(Dropdown);
        });
    }

    public void ChooseLanguage(TMP_Dropdown dropdown)
    {
        Language language = Language.ENGLISH;
        switch (dropdown.value)
        {
            case 0: language = Language.ENGLISH; break;
            case 1: language = Language.FRENCH; break;
            case 2: language = Language.BRPT; break;
            case 3: language = Language.FILIPINO; break;
            case 4: language = Language.LATAMSPANISH; break;
            case 5: language = Language.ITALIAN; break;
            case 6: language = Language.GERMAN; break;

        }
        LocalizationManager.Instance.ChangeLanguage(language);
    }

    public void CreditsScene()
    {
        SceneManager.LoadScene("Credits", LoadSceneMode.Single);
    }

    public void ExitCredits()
    {
        SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
    }

    public void Discord()
    {
        Application.OpenURL("https://discord.com/invite/3NZdTqDVda");
    }

    public void SteamWishlist()
    {
        Application.OpenURL("https://store.steampowered.com/app/1748600/Sainthood/");
    }

    private string DayofTheWeek(int Day)
    {
        return "Day " + MissionManager.Instance.CurrentMissionId;

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

    public void DisplayStatusEffect()
    {
        if (MissionManager.MissionOver)
        {
            return;
        }

        StatusEffectDisplay.gameObject.SetActive(!StatusEffectDisplay.gameObject.activeSelf);
        if (!StatusEffectDisplay.gameObject.activeSelf) return;

        StatusEffectDisplay.Init();
    }

    public void DisplayToolTip(string text)
    {
        return;
        if (MissionManager.MissionOver)
        {
            TooltipDisplay.transform.parent.gameObject.SetActive(false);
            return;
        }

        if (Black.color.a > 0) return;
        if (TooltipDisplay == null) return;
        if (text == TooltipDisplay.text) return;

        TooltipDisplay.text = text;
        TooltipDisplay.transform.parent.gameObject.SetActive(false);

        if (text != "")
        {
            TooltipDisplay.transform.parent.gameObject.SetActive(true);
        }
    }

    public void DisplayRunAttempts()
    {
        if(RunAttemptsDisplay != null)
        {
            RunAttemptsDisplay.text = "Run Attempts: " + GameManager.Instance.RunAttempts;
        }
    }

    public void ShowWeekBeginText(string text)
    {
        StartCoroutine(ShowWeekBeginTextAsync(text));
    }

    private IEnumerator ShowWeekBeginTextAsync(string text)
    {
        WeekBeginCrossFade = true;
        yield return StartCoroutine(CrossFadeAsync(1, 10));
        if (GameSettings.Instance.SkipSplashScreens)
        {
            WeekBeginCrossFade = false;
            yield break;
        }
        GameManager.Instance.InGameSession = false;
        WeekIntroBGGraphic.gameObject.SetActive(true);
        WeekIntroBGGraphic.DOFade(1, 1f);

        yield return new WaitForSeconds(1.5f);
        CurrentWeekIntroGraphic.sprite = Resources.Load<Sprite>($"Icons/{MissionManager.Instance.CurrentMission.Season}");
        CurrentWeekIntroGraphic.DOFade(1f, 3f); 

        if(!string.IsNullOrEmpty(text)) yield return new WaitForSeconds(3f);

        WeekIntroBGGraphic.DOFade(0, 3f);
        CurrentWeekIntroGraphic.DOFade(0f, 3f);
        yield return StartCoroutine(CrossFadeAsync(0, 5f));
        yield return new WaitForSeconds(3f);
        
        WeekIntroBGGraphic.gameObject.SetActive(false);
        WeekBeginCrossFade = false;
        GameManager.Instance.InGameSession = true;
    }

    public void ShowDayBeginText(string text)
    {
        StartCoroutine(ShowDayBeginTextAsync(text));
    }

    private IEnumerator ShowDayBeginTextAsync(string text)
    {
        WeekBeginCrossFade = true;
        yield return StartCoroutine(CrossFadeAsync(1, 10));
        if (GameSettings.Instance.SkipSplashScreens)
        {
            WeekBeginCrossFade = false;
            yield break;
        }
        GameManager.Instance.InGameSession = false;
        WeekIntroBGGraphic.gameObject.SetActive(true);
        WeekIntroBGGraphic.DOFade(1, 1f);

        yield return new WaitForSeconds(1f);

        WeekIntroBGGraphic.DOFade(0, 1f);
        CurrentWeekIntroGraphic.DOFade(0f, 1f);
        yield return StartCoroutine(CrossFadeAsync(0, 5f));
        yield return new WaitForSeconds(2f);

        WeekIntroBGGraphic.gameObject.SetActive(false);
        WeekBeginCrossFade = false;
        GameManager.Instance.InGameSession = true;
    }

    public void CrossFade(float fade, float speed = 5f)
    {
        StartCoroutine(CrossFadeAsync(fade, speed));
    }

    public bool WeekBeginCrossFade;
    public bool CrossFading;
    private IEnumerator CrossFadeAsync(float fade, float speed)
    {
        CrossFading = true;
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


    public void EnableAllUIElements(bool enable)
    {
        if (LeftItems == null) return;

        LeftItems.SetActive(enable);
        foreach (Transform g in LeftItems.transform)
        {
            g.gameObject.SetActive(enable);
        }
        RightItems.SetActive(enable);
        foreach (Transform g in RightItems.transform)
        {
            g.gameObject.SetActive(enable);
        }
        CenterItems.SetActive(enable);
        SideNotifItems.SetActive(enable);
        UIHidden?.Invoke(enable);
        WeatherManager.Instance.BroadcastWeather();
        EnergyAdditionDisplay.transform.GetChild(0).gameObject.SetActive(false);
        TreasuryAdditionDisplay.transform.GetChild(0).gameObject.SetActive(false);
        CPAdditionDisplay.transform.GetChild(0).gameObject.SetActive(false);
        FPAdditionDisplay.transform.GetChild(0).gameObject.SetActive(false);
        FullUIVisible = enable;

        TutorialHideUI();
    }

    private void TutorialHideUI()
    {
        if (!GameSettings.Instance.TUTORIAL_MODE) return;
        if (!TutorialManager.Instance.Steps.Contains(CustomEventType.NEW_TUTORIAL_35))
        {
            HideUIItemsExcept(new List<string>() { "Time" });
        }
        else if (!TutorialManager.Instance.Steps.Contains(CustomEventType.NEW_TUTORIAL_4))
        {
            HideUIItemsExcept(new List<string>() { "Time", "Spirits" });
        }
        else if (!TutorialManager.Instance.Steps.Contains(CustomEventType.NEW_TUTORIAL_5))
        {
            HideUIItemsExcept(new List<string>() { "Time", "Spirits", "Energy" });
        }
        else if (!TutorialManager.Instance.Steps.Contains(CustomEventType.NEW_TUTORIAL_6))
        {
            HideUIItemsExcept(new List<string>() { "Time", "Spirits", "Energy", "FP", "CP" });
        }
        else
        {
            HideUIItemsExcept(new List<string>() { "Time", "Spirits", "Energy", "FP", "CP", "TreasuryBalance" });
        }
    }

    public void StartMinigame(MinigameType minigame, Action<string> callback)
    {
        MinigamePlayer.gameObject.SetActive(true);
        MinigamePlayer.Init(minigame, callback);
    }

    public void GameOver()
    {
        StatusEffectDisplay.gameObject.SetActive(false);
    }

    public void TriggerGameOver()
    {
        GameOverPopup.gameObject.SetActive(!GameOverPopup.gameObject.activeSelf);
    }

    public void QuitGame()
    {
        Application.Quit();
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
