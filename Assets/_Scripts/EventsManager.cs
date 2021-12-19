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

    private void Awake()
    {
        Instance = this;
        GameClock.ExecuteEvents += ExecuteEvents;
        GameClock.Ticked += TryEventTrigger;
        GameClock.StartNewDay += StartNewDay;
    }

    public void AddEventToList(CustomEventType newEvent)
    {
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
        if (!UI.Instance.WeekBeginCrossFade && !GameSettings.Instance.FTUE && !GameClock.DeltaTime) return;

        if (!GameSettings.Instance.StoryToggle) return;

        int CurrentWeek = MissionManager.Instance.CurrentMission.CurrentWeek;
        GameClock currentClock = GameManager.Instance.GameClock;
        var storyEvent = GameDataManager.Instance.StoryEventData.Where(s => s.Value.Week == CurrentWeek && s.Value.Day == currentClock.Day && s.Value.Time == currentClock.Time).FirstOrDefault().Value;
        if (storyEvent == null) return;

        StoryEvents.Add(storyEvent);
    }

    public void StartNewDay()
    {
        CurrentEvents.Clear();
        GameClock c = GameManager.Instance.GameClock;

        if (MissionManager.Instance.CurrentMission.CurrentWeek == 1 || c.EndofWeek()) return;

        if(c.Day % 7 == 0)
        {
            AddEventToList(CustomEventType.SUNDAY_MASS);
            GameDataManager.Instance.TriggeredDailyEvents.Clear();
        }
        else
        {
            var randomEvent = GameDataManager.Instance.GetRandomEvent(EventGroup.DAILY);
            randomEvent = GameDataManager.Instance.RemixEventBySeason(randomEvent);
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
            UI.Instance.EventAlert(new CustomEventData() { LocalizationKey = e.Id, EventPopupType = EventPopupType.OK, IsOrderedSequence = e.IsOrderedSequence });
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
