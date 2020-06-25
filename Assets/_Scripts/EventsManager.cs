using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EventsManager : MonoBehaviour
{
    public static EventsManager Instance { get; private set; }
    public static UnityAction<bool> EventTriggered;

    public List<CustomEventData> EventList = new List<CustomEventData>();
    public bool EventInProgress;

    private void Awake()
    {
        Instance = this;
        GameClock.ExecuteEvents += ExecuteEvents;
        GameClock.Ticked += TryEventTrigger;
    }

    public void AddEventToList(EventType newEvent)
    {
        var e = GameDataManager.Instance.CustomEventData[newEvent][0]; //Grab based on weight
        EventList.Add(e); 
        //Sort Events?
    }

    public void ExecuteEvents()
    {
        StartCoroutine(ExecuteEventsAsync());
    }

    private IEnumerator ExecuteEventsAsync()
    {
        Player.LockMovement = true;
        EventTriggered?.Invoke(true);

        //Execute events one by one
        foreach (var e in EventList)
        {
            EventInProgress = true;
            UI.Instance.EventAlert(e);
            while (EventInProgress)
            {                
                yield return null;
            }
        }
        EventList.Clear();
        Player.LockMovement = false;
        EventTriggered?.Invoke(false);
    }

    public void TryEventTrigger(double time, int day)
    {
        //Check Time of day
        //Decide which type of Event to Trigger
        //Randomly pick an event from list

        if(time > 60)
        {
            AddEventToList(EventType.MARKET_HOURS);
        }
    }

    private void OnDisable()
    {
        GameClock.ExecuteEvents -= ExecuteEvents;
        GameClock.Ticked -= TryEventTrigger;
    }
}
