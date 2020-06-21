using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CustomEventPopup : MonoBehaviour
{
    public GameObject YesNoGO;
    public GameObject OKGO;
    public GameObject NextGO;
    public CustomEventData EventData;
    public TextMeshProUGUI EventText;

    private int CurrentSequenceNumber;

    public void Setup(CustomEventData customEvent)
    {
        EventData = customEvent;
        YesNoGO.SetActive(customEvent.EventGroup == EventGroup.IMMEDIATE);
        OKGO.SetActive(customEvent.EventGroup == EventGroup.DAILY);

        if (customEvent.IsOrderedSequence)
        {
            CurrentSequenceNumber = 0;
            EventText.text = $"{LocalizationManager.Instance.GetText(customEvent.LocalizationKey, CurrentSequenceNumber)}";
            YesNoGO.SetActive(false);
            OKGO.SetActive(false);
            NextGO.SetActive(true);
        }
        else
        {
            EventText.text = $"{LocalizationManager.Instance.GetText(customEvent.LocalizationKey)}";
        }
    }

    public void Yes()
    {
        EventsManager.Instance.EventInProgress = false;
        gameObject.SetActive(false);
    }

    public void No()
    {
        EventsManager.Instance.EventInProgress = false;
        gameObject.SetActive(false);
    }

    public void OK()
    {
        EventsManager.Instance.EventInProgress = false;
        gameObject.SetActive(false);
    }

    public void Continue()
    {
        int sequences = LocalizationManager.Instance.GetTotalSequences(EventData.LocalizationKey);
        CurrentSequenceNumber++;

        if (CurrentSequenceNumber >= sequences) return;

        if(sequences - CurrentSequenceNumber == 1)
        {
            YesNoGO.SetActive(EventData.EventGroup == EventGroup.IMMEDIATE);
            OKGO.SetActive(EventData.EventGroup == EventGroup.DAILY);
            NextGO.SetActive(false);
        }

        EventText.text = $"{LocalizationManager.Instance.GetText(EventData.LocalizationKey, CurrentSequenceNumber)}";
    }
}