using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class EventsManager : MonoBehaviour
{
    public static EventsManager Instance { get; private set; }
    public static UnityAction<bool> EventDialogTriggered;
    public static UnityAction<CustomEventData> EventExecuted;

    private List<CustomEventData> EventList = new List<CustomEventData>();
    private List<StoryEventData> StoryEvents = new List<StoryEventData>();
    public List<CustomEventData> CurrentEvents = new List<CustomEventData>();
    public bool EventInProgress;

    private Coroutine eventRoutine;
    public CustomEventType DailyEvent;

    private void Awake()
    {
        Instance = this;
        GameClock.ExecuteEvents += ExecuteEvents;
        GameClock.Ticked += TryEventTrigger;
        GameClock.StartNewDay += StartNewDay;
    }

    public void AddEventToList(CustomEventType newEvent)
    {
        if (EventInProgress) return;

        var e = GameDataManager.Instance.CustomEventData[newEvent][0]; //Grab based on weight

        EventList.Add(e);
        CurrentEvents.Add(e);
    }

    public void ExecuteEvents()
    {
        if (!GameSettings.Instance.CustomEventsToggle)
        {
            EventList.Clear();
            return;
        }

        GameClock c = GameManager.Instance.GameClock;
        if (c.EndofWeek())
        {
            EventList.RemoveAll(e => e.EventGroup != EventGroup.ENDWEEK);
        }

        if (EventInProgress || (EventList.Count == 0 && StoryEvents.Count == 0)) return;

        StartCoroutine(ExecuteEventsAsync());
    }

    private IEnumerator ExecuteEventsAsync()
    {
        EventDialogTriggered?.Invoke(true);
        Player.LockMovement = true;

        UI.Instance.EnableAllUIElements(false);
        //Execute events one by one
        foreach (var e in EventList)
        {
            EventInProgress = true;
            while (UI.Instance.WeekBeginCrossFade) yield return null;
            UI.Instance.EventAlert(e);
            ExecuteEvent(e);
            while (EventInProgress)
            {                
                yield return null;
            }
        }

        EventList.Clear();
        UI.Instance.EnableAllUIElements(true);

        if (EventInProgress) yield break; //If something else has started an event, break out early.

        eventRoutine = StartCoroutine(ExecuteStoryEventsAsync());
    }

    public void TryEventTrigger(double time, int day)
    {
        if (!GameClock.DeltaTime) return;
        if (!UI.Instance.WeekBeginCrossFade && !GameSettings.Instance.FTUE && !GameClock.DeltaTime) return;

        if (!GameSettings.Instance.StoryToggle) return;

        int CurrentWeek = MissionManager.Instance.CurrentMission.CurrentWeek;
        GameClock currentClock = GameManager.Instance.GameClock;
        var storyEvent = GameDataManager.Instance.StoryEventData.Select(y => y.Value).Where(s => s.Week == CurrentWeek && s.Day == currentClock.Day && s.Time == currentClock.Time).OrderBy(x => x.OrderBy);
        if (storyEvent == null) return;

        var filteredEvents = storyEvent.Where(e => 
        { 
            if(e.Id.Contains("Tutorial") && !GameSettings.Instance.FTUE)
            {
                return false;
            }

            return true;
        });
        
        if(!EventList.Any(x=> x.Id == InteractableHouse.HouseTriggeredEvent) && InteractableHouse.HouseTriggeredEvent != CustomEventType.NONE)
        {
            AddEventToList(InteractableHouse.HouseTriggeredEvent);
            ExecuteEvents();
        }

        StoryEvents.AddRange(filteredEvents);
    }

    public void ForceTriggerStoryEvent(StoryEventData e)
    {
        StoryEvents.Add(e);
        ExecuteEvents();
    }

    public void StartNewDay()
    {
        CurrentEvents.Clear();
        GameClock c = GameManager.Instance.GameClock;

        if (c.EndofWeek()) return;
        if (GameSettings.Instance.FTUE && GameManager.Instance.MissionManager.CurrentMission.CurrentWeek == 1 && c.Day < 2) return;

        if (c.Day % 5 == 0)
        {
            AddEventToList(CustomEventType.SUNDAY_MASS);
            GameDataManager.Instance.TriggeredDailyEvents.Clear();
        }
        else
        {
            var randomEvent = GameManager.Instance.SaveData.DailyEvent == CustomEventType.NONE ? GameDataManager.Instance.GetRandomEvent(EventGroup.DAILY) : GameDataManager.Instance.GetEvent(GameManager.Instance.SaveData.DailyEvent);
            var security = InventoryManager.Instance.GetProvision(Provision.SECURITY_GUARDS);
            if(security != null && randomEvent.Id == CustomEventType.VANDALISM)
            {
                randomEvent = GameDataManager.Instance.GetEvent(CustomEventType.VANDALISM_STOPPED);
            }
            randomEvent = GameDataManager.Instance.RemixEventBySeason(randomEvent);
            DailyEvent = randomEvent.Id;
            AddEventToList(randomEvent.Id);
        }
    }

    private void ExecuteEvent(CustomEventData e)
    {
        switch (e.Id)
        {
            case CustomEventType.RAIN:
                WeatherManager.Instance.OverrideWeatherActivation(Random.Range(3, 5), (int)e.Cost);
                break;
        }
        EventExecuted?.Invoke(e);
    }

    private IEnumerator ExecuteStoryEventsAsync()
    {
        while (EventInProgress)
        {
            yield return null;
        }

        UI.Instance.EnableAllUIElements(false);
        if (GameSettings.Instance.FTUE && TutorialManager.Instance.CurrentTutorialStep < 1) yield return new WaitForSeconds(10f);
        //Execute events one by one
        foreach (var e in StoryEvents)
        {
            EventInProgress = true;
            while (UI.Instance.WeekBeginCrossFade) yield return null;
            UI.Instance.EventAlert(new CustomEventData() { LocalizationKey = e.Id, EventGroup = e.EventGroup, EventPopupType = EventPopupType.OK, ImagePath = e.ImagePath, IsOrderedSequence = e.IsOrderedSequence });
            while (EventInProgress)
            {
                yield return null;
            }
        }

        yield return null;

        StoryEvents.Clear();
        UI.Instance.EnableAllUIElements(true);
        if (MissionManager.MissionOver) yield break;
        Player.LockMovement = false;
        EventDialogTriggered?.Invoke(false);
    }

    public bool HasEventsInQueue()
    {
        return EventList.Count > 0 || StoryEvents.Count > 0;
    }

    public void OnOveride()
    {
        StopAllCoroutines();
        StoryEvents.Clear();
        EventInProgress = false;
        Player.LockMovement = false;
        EventDialogTriggered?.Invoke(false);
    }

    private void OnDisable()
    {
        GameClock.ExecuteEvents -= ExecuteEvents;
        GameClock.Ticked -= TryEventTrigger;
        GameClock.StartNewDay -= StartNewDay;
    }
}
