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

    private void Awake()
    {
        Instance = this;
        GameClock.ExecuteEvents += ExecuteEvents;
        GameClock.Ticked += TryEventTrigger;
        GameClock.StartNewDay += StartNewDay;
    }

    public void AddEventToList(EventType newEvent)
    {
        var e = GameDataManager.Instance.CustomEventData[newEvent][0]; //Grab based on weight
        EventList.Add(e);
        CurrentEvents.Add(e);
        //Sort Events?
    }

    public void ExecuteEvents()
    {
        if (EventInProgress || EventList.Count == 0) return;

        StartCoroutine(ExecuteEventsAsync());
    }

    private IEnumerator ExecuteEventsAsync()
    {
        EventDialogTriggered?.Invoke(true);
        Player.LockMovement = true;

        //Execute events one by one
        foreach (var e in EventList)
        {
            EventInProgress = true;
            UI.Instance.EventAlert(e);
            ExecuteEvent(e);
            while (EventInProgress)
            {                
                yield return null;
            }
        }

        yield return null;

        EventList.Clear();
        Player.LockMovement = false;
        EventDialogTriggered?.Invoke(false);
    }

    public void TryEventTrigger(double time, int day)
    {
        //Try Trigger Story Events First
        if (ExecuteStoryEvent()) return;

        if(time < 21 && !EventInProgress)
        {
            if (Random.Range(0, 100) < 2 && CurrentEvents.Count < 3)
            {
                AddEventToList(GameDataManager.Instance.GetRandomEvent(EventGroup.IMMEDIATE).Id);
            }
        }
    }

    public void StartNewDay()
    {
        CurrentEvents.Clear();
        if(Random.Range(0, 100) < 50)
        {
            AddEventToList(GameDataManager.Instance.GetRandomEvent(EventGroup.DAILY).Id);
        }
    }

    private void ExecuteEvent(CustomEventData e)
    {
        switch (e.Id)
        {
            case EventType.RAIN:
                WeatherManager.Instance.OverrideWeatherActivation(Random.Range(3, 5), (int)e.Cost);
                break;
        }
        EventExecuted?.Invoke(e);
    }

    private bool ExecuteStoryEvent()
    {
        int CurrentWeek = MissionManager.Instance.CurrentMission.CurrentWeek;
        GameClock currentClock = GameManager.Instance.GameClock;
        var storyEvent = GameDataManager.Instance.StoryEventData.Where(s => s.Value.Week == CurrentWeek && s.Value.Day == currentClock.Day && s.Value.Time == currentClock.Time).FirstOrDefault().Value;
        if (storyEvent == null) return false;

        StoryEvents.Add(storyEvent);
        StartCoroutine(ExecuteStoryEventsAsync());

        return true;
    }

    private IEnumerator ExecuteStoryEventsAsync()
    {
        EventDialogTriggered?.Invoke(true);
        Player.LockMovement = true;

        //Execute events one by one
        foreach (var e in StoryEvents)
        {
            EventInProgress = true;
            UI.Instance.EventAlert(new CustomEventData() { LocalizationKey = e.Id, EventPopupType = EventPopupType.OK, IsOrderedSequence = e.IsOrderedSequence });
            while (EventInProgress)
            {
                yield return null;
            }
        }

        yield return null;

        StoryEvents.Clear();
        Player.LockMovement = false;
        EventDialogTriggered?.Invoke(false);
    }

    public bool HasEventsInQueue()
    {
        return EventList.Count > 0;
    }

    private void OnDisable()
    {
        GameClock.ExecuteEvents -= ExecuteEvents;
        GameClock.Ticked -= TryEventTrigger;
        GameClock.StartNewDay -= StartNewDay;
    }
}
