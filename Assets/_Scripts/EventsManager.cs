using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EventsManager : MonoBehaviour
{
    public static EventsManager Instance { get; private set; }
    public static UnityAction<bool> EventDialogTriggered;
    public static UnityAction<CustomEventData> EventExecuted;

    private List<CustomEventData> EventList = new List<CustomEventData>();
    public List<CustomEventData> CurrentEvents = new List<CustomEventData>();
    public bool EventInProgress;

    private void Awake()
    {
        Instance = this;
        GameClock.ExecuteEvents += ExecuteEvents;
        GameClock.Ticked += TryEventTrigger;
        MissionManager.StartNewDay += StartNewDay;
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
        Player.LockMovement = true;
        EventDialogTriggered?.Invoke(true);

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

    private void OnDisable()
    {
        GameClock.ExecuteEvents -= ExecuteEvents;
        GameClock.Ticked -= TryEventTrigger;
        MissionManager.StartNewDay -= StartNewDay;
    }
}
