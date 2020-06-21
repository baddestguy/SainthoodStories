using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

public class InteractableHouse : InteractableObject
{
    public GameClock DeadlineTime;
    public int EnergyConsumption;
    public int OpenTime;
    public int ClosingTime;
    public int ItemDeliveryPoints;
    public int VolunteerPoints;
    public int DeadlineDeliveryBonus;
    public bool DeadlineSet;
    public int RequiredItems;    

    public MissionDifficulty MissionDifficulty;
    public static int DeadlineCounter;

    protected int CurrentCharityPoints;
    protected int CurrentFaithPoints;
    protected const int MeditationPoints = 1;
    public int NeglectedPoints;
    protected int NeglectedMultiplier = 1;

    protected PopIcon PopIcon;
    protected PopUI PopUI;
    protected string PopUILocation = "";
    protected string OriginalPopUILocation = "";

    public UnityEvent ButtonCallback;
    private bool CameraLockOnMe;
    private PopUIFX PopUIFX;
    public bool HouseUIActive;

    public BuildingState BuildingState;
    public int BuildPoints = 3;
    public GameObject RubbleGo;
    public GameObject BuildingGo;

    protected virtual void Start()
    {
        UI.Meditate += Meditated;
        UI.DeliverItem += DeliverItem;
        UI.Volunteer += VolunteerWork;
        MissionManager.EndOfDay += ReportScores;
        MissionManager.EndOfDay += EndofDay;
        EventsManager.EventTriggered += OnEventTriggered;

        Initialize();
    }

    public void Initialize()
    {
        if (BuildingState == BuildingState.NORMAL)
        {
            PopIcon = Instantiate(Resources.Load<GameObject>("UI/PopIcon")).GetComponent<PopIcon>();
        }
        else if (BuildingState == BuildingState.RUBBLE)
        {
            PopIcon = Instantiate(Resources.Load<GameObject>("UI/PopConstructIcon")).GetComponent<PopIcon>();
            OriginalPopUILocation = PopUILocation;
            PopUILocation = "UI/ConstructUI";
        }

        RubbleGo.SetActive(BuildingState == BuildingState.RUBBLE);
        BuildingGo.SetActive(BuildingState == BuildingState.NORMAL);

        PopIcon.transform.SetParent(transform);
        PopIcon.transform.localPosition = new Vector3(0, 1, 0);
        PopIcon.gameObject.SetActive(false);

        if (!string.IsNullOrEmpty(PopUILocation))
        {
            PopUI = Instantiate(Resources.Load<GameObject>(PopUILocation)).GetComponent<PopUI>();
            PopUI.transform.SetParent(transform);
            PopUI.transform.localPosition = new Vector3(0, 1, 0);
            PopUI.gameObject.SetActive(false);
        }

        if(PopUIFX == null)
        {
            PopUIFX = Instantiate(Resources.Load("UI/PopUIFX") as GameObject).GetComponent<PopUIFX>();
            PopUIFX.gameObject.SetActive(false);
        }
    }

    public void Init(int deadline, MapTile groundTile, TileData tileData, Sprite[] sprites, int sortingOrder = 0)
    {
        Init(groundTile, tileData, sprites, sortingOrder);
    }

    public override void Tick(double time, int day)
    {
        if (DeadlineTime.Time != -1)
        {
            PopMyIcon();
            Debug.LogWarning($"{name}: Deadline: {DeadlineTime.Time} : DAY {DeadlineTime.Day} : {RequiredItems} Items!!");
        }
        SetDeadlineTime(time, day);

        if ((DeadlineTime.Time != -1) && (time >= DeadlineTime.Time && day >= DeadlineTime.Day))
        {
            Debug.LogError($"{name}: TIME UP!");
            NeglectedMultiplier++;
            DeadlineCounter--;
            DeadlineTime.SetClock(-1, day);
            DeadlineDeliveryBonus = 1;
            DeadlineSet = false;
            RequiredItems = 0;
            PopIcon.gameObject.SetActive(false);
            UI.Instance.SideNotificationPop(GetType().Name);
        }
        PopUI.Init(PopUICallback, GetType().Name, RequiredItems, DeadlineTime);
    }

    public virtual void SetDeadlineTime(double time, int day)
    {
        if (BuildingState != BuildingState.NORMAL) return;
        if (time < 6 || time >= 18) return;
        if ((DeadlineTime.Time != -1)) return;

        switch (MissionDifficulty)
        {
            case MissionDifficulty.EASY:
                if (DeadlineCounter < 1)
                {
                    if(Random.Range(0, 100) < 1)
                    {
                        DeadlineCounter++;
                        DeadlineTime.SetClock(time + RandomFutureTimeByDifficulty(), day);
                        RequiredItems = 1;
                        DeadlineDeliveryBonus = 4;
                        DeadlineSet = true;
                        PopMyIcon();
                        Debug.LogWarning($"{name}: DEADLINE SET FOR {DeadlineTime.Time} : DAY  {DeadlineTime.Day} : {RequiredItems} Items!");
                    }
                }
                break;

            case MissionDifficulty.NORMAL:
                if (DeadlineCounter < 2)
                {
                    if (Random.Range(0, 100) < 1)
                    {
                        DeadlineCounter++;
                        DeadlineTime.SetClock(time + RandomFutureTimeByDifficulty(), day);
                        RequiredItems = Random.Range(1,3);
                        DeadlineDeliveryBonus = 3;
                        DeadlineSet = true;
                        PopMyIcon();
                        Debug.LogWarning($"{name}: DEADLINE SET FOR {DeadlineTime.Time} : DAY  {DeadlineTime.Day} : {RequiredItems} Items!");
                    }
                }
                break;

            case MissionDifficulty.HARD:
                if (DeadlineCounter < 3)
                {
                    if (Random.Range(0, 100) < 2)
                    {
                        DeadlineCounter++;
                        DeadlineTime.SetClock(time + RandomFutureTimeByDifficulty(), day);
                        RequiredItems = Random.Range(1,4);
                        DeadlineDeliveryBonus = 2;
                        DeadlineSet = true;
                        PopMyIcon();
                        Debug.LogWarning($"{name}: DEADLINE SET FOR {DeadlineTime.Time} : DAY  {DeadlineTime.Day} : {RequiredItems} Items!");
                    }
                }
                break;
        }
    }

    public double RandomFutureTimeByDifficulty()
    {
        switch (MissionDifficulty)
        {
            case MissionDifficulty.EASY: return Random.Range(6, 9);
            case MissionDifficulty.NORMAL: return Random.Range(5, 8);
            case MissionDifficulty.HARD: return Random.Range(5, 8);
        }

        return -1;
    }

    public virtual void EndofDay()
    {        
    }

    public override void MissionBegin(Mission mission)
    {
        MissionDifficulty = GameManager.MissionDifficulty;
        DeadlineTime = new GameClock(-1);
        DeadlineDeliveryBonus = 1;
    }

    public virtual void Meditated(InteractableHouse house)
    {
        if (house != this) return;

        GameClock clock = GameManager.Instance.GameClock;
        Player player = GameManager.Instance.Player;

        player.ConsumeEnergy(-1);
        clock.Tick();
        UI.Instance.DisplayMessage("MEDITATED!!");
        UpdateFaithPoints(MeditationPoints);
    }

    public virtual void DeliverItem(InteractableHouse house)
    {
        if (house != this) return;

        RequiredItems--;
        PopMyIcon();
        PopUI.Init(PopUICallback, GetType().Name, RequiredItems, DeadlineTime);
        if (RequiredItems <= 0)
        {
            DeadlineCounter = Mathf.Max(0, DeadlineCounter - 1);
            DeadlineTime.SetClock(-1, DeadlineTime.Day);
            DeadlineDeliveryBonus = 1;
            DeadlineSet = false;
            RequiredItems = 0;
            PopIcon.gameObject.SetActive(false);
            UI.Instance.SideNotificationPop(GetType().Name);
        }
    }

    protected void PopUIFXIcons(string icon, int amount)
    {
        PopUIFX.transform.position = transform.position + new Vector3(0, 0, 0);
        PopUIFX.gameObject.SetActive(true);
        PopUIFX.Init(icon, amount, 0);
    }

    protected IEnumerator PopUIFXIconsAsync(Stack<Tuple<string, int>> stack)
    {
        while(stack.Count > 0)
        {
            var item = stack.Pop();
            PopUIFXIcons(item.Item1, item.Item2);
            yield return new WaitForSeconds(0.5f);
        }
    }

    public virtual void Build()
    {
        BuildPoints--;
        UI.Instance.DisplayMessage("BUILDING!");
        if(BuildPoints <= 0)
        {
            //Play Cool Construction Vfx and Animation!
            BuildingState = BuildingState.NORMAL;
            PopUILocation = OriginalPopUILocation;
            Destroy(PopUI.gameObject);
            Initialize();
            PopUI.gameObject.SetActive(true);
        }
        UpdateCharityPoints(VolunteerPoints);
        GameClock clock = GameManager.Instance.GameClock;
        Player player = GameManager.Instance.Player;
        player.ConsumeEnergy(EnergyConsumption); //Umm...?
        clock.Tick();
    }

    public virtual void PopUICallback(string button)
    {
        switch (button)
        {
            case "BUILD":
                Build();
                break;
        }
    }

    public virtual void VolunteerWork(InteractableHouse house)
    {
    }

    public virtual void UpdateCharityPoints(int amount)
    {
        CurrentCharityPoints += amount;
        Stack<Tuple<string, int>> stack = new Stack<Tuple<string, int>>();
        stack.Push(new Tuple<string, int>("CPHappy", amount));
        if (EnergyConsumption != 0) stack.Push(new Tuple<string, int>("Energy", -EnergyConsumption));
        StartCoroutine(PopUIFXIconsAsync(stack));
    }

    public virtual void UpdateFaithPoints(int amount)
    {
        int faithMultiplier = InventoryManager.Instance.HasProvision(Provision.ROSARY) ? 2 : 1;
        CurrentFaithPoints += amount * faithMultiplier;
        Stack<Tuple<string, int>> stack = new Stack<Tuple<string, int>>();
        stack.Push(new Tuple<string, int>("InteractableChurch", amount * faithMultiplier));
        stack.Push(new Tuple<string, int>("Energy", 1)); //Prayer Energy should be a variable
        StartCoroutine(PopUIFXIconsAsync(stack));
  
        Debug.LogWarning("FAITH: " + CurrentFaithPoints);
    }

    public virtual void ReportScores()
    {
        GameManager.Instance.MissionManager.UpdateCharityPoints(CurrentCharityPoints > 0 ? CurrentCharityPoints : (NeglectedPoints * NeglectedMultiplier), this);

        if (CurrentCharityPoints <= 0)
        {
            NeglectedMultiplier++;
        }
        else
        {
            NeglectedMultiplier = 1;
        }

        CurrentCharityPoints = 0;
        CurrentFaithPoints = 0;
    }

    public bool DuringOpenHours(GameClock newClock = null)
    {
        GameClock clock = newClock ?? GameManager.Instance.GameClock;
        return clock.Time >= OpenTime && clock.Time < ClosingTime;
    }

    public virtual void PopMyIcon(string name = "", int items = -1, GameClock time = null)
    {
        if (string.IsNullOrEmpty(name)) name = GetType().Name;
        if (items < 0) items = RequiredItems;
        if (time == null) time = DeadlineTime;

        if (HouseUIActive)
        {
            UI.Instance.SideNotificationPush(name, items, time, GetType().Name);
            PopIcon.gameObject.SetActive(false);
            return;
        }
        else
        {
            UI.Instance.SideNotificationPop(GetType().Name);
        }

        PopIcon.gameObject.SetActive(true);
        PopIcon.Init(name, items, time);
    }

    public override void OnPlayerMoved(Energy energy, MapTile tile)
    {
        base.OnPlayerMoved(energy, tile);
        if (tile.GetInstanceID() == GetInstanceID())
        {
            Camera.main.GetComponent<CameraControls>().SetCameraTarget(transform.TransformPoint(-7.53f, 11.6f, -5.78f));
            CameraLockOnMe = true;
            HouseUIActive = true;
            PopIcon.gameObject.SetActive(false);
            UI.Instance.EnableInventoryUI(true);

        }
        else if(CameraLockOnMe)
        {
            Camera.main.GetComponent<CameraControls>().SetCameraTarget(Vector3.zero);
            CameraLockOnMe = false;
            HouseUIActive = false;
            UI.Instance.EnableInventoryUI(false);
        }
    }

    private void OnEventTriggered(bool started)
    {
        if (started && HouseUIActive)
        {
            PopUI.gameObject.SetActive(false);
        }
        else if (!started && HouseUIActive)
        {
            PopUI.gameObject.SetActive(true);
        }
    }


    public override void OnDisable()
    {
        UI.Meditate -= Meditated;
        UI.DeliverItem -= DeliverItem;
        UI.Volunteer -= VolunteerWork;
        MissionManager.EndOfDay -= EndofDay;
        MissionManager.EndOfDay -= ReportScores;
        EventsManager.EventTriggered -= OnEventTriggered;
        HouseUIActive = false;
        base.OnDisable();
    }
}
