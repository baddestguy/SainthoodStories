using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CustomEventPopup : MonoBehaviour
{
    public GameObject YesNoGO;
    public GameObject OKGO;
    public GameObject NextGO;
    public CustomEventData EventData;
    public TextMeshProUGUI EventText;

    public GameObject IconsGO;
    public TextMeshProUGUI TimeDisplay;
    public TextMeshProUGUI EnergyDisplay;
    public TextMeshProUGUI FPCPDisplay;
    public GameObject FPIcon;
    public GameObject CPIcon;

    public Image Hearts;
    public TextMeshProUGUI HeartsPlus;

    private int CurrentSequenceNumber;

    public void Setup(CustomEventData customEvent)
    {
        EventData = customEvent;
        YesNoGO.SetActive(customEvent.EventPopupType == EventPopupType.YESNO);
        IconsGO.SetActive(customEvent.EventPopupType == EventPopupType.YESNO);
        OKGO.SetActive(customEvent.EventPopupType == EventPopupType.OK);

        switch (customEvent.EventPopupType)
        {
            case EventPopupType.YESNO:
                GameClock clock = GameManager.Instance.GameClock;
                Player player = GameManager.Instance.Player;

                GameClock c = new GameClock(clock.Time);
                c.AddTicks((int)customEvent.Cost);
                TimeDisplay.text = $"{(int)c.Time}:{(c.Time % 1 == 0 ? "00" : "30")}";

                var moddedEnergy = player.ModifyEnergyConsumption(amount: (int)customEvent.Cost);
                EnergyDisplay.text = $"-{moddedEnergy}";

                FPIcon.SetActive(GameDataManager.Instance.IsSpritualEvent(customEvent.Id));
                CPIcon.SetActive(!GameDataManager.Instance.IsSpritualEvent(customEvent.Id));
                FPCPDisplay.text = $"+{customEvent.Gain}";
                break;
        }

        int sequences = LocalizationManager.Instance.GetTotalSequences(EventData.LocalizationKey);
        if (customEvent.IsOrderedSequence && sequences > 1)
        {
            CurrentSequenceNumber = 0;
            EventText.text = $"{LocalizationManager.Instance.GetText(customEvent.LocalizationKey, CurrentSequenceNumber)}";
            YesNoGO.SetActive(false);
            OKGO.SetActive(false);
            IconsGO.SetActive(false);
            NextGO.SetActive(true);
        }
        else
        {
            EventText.text = $"{LocalizationManager.Instance.GetText(customEvent.LocalizationKey)}";
        }

        if(customEvent.EventGroup == EventGroup.THANKYOU)
        {
            StartCoroutine(HeartsAnimation());
        }
    }

    IEnumerator HeartsAnimation()
    {
        Color color = Hearts.color;
        color.a = 1f;
        Hearts.color = color;
        HeartsPlus.color = color;

        yield return new WaitForSeconds(1.5f);
        var originalPosition = Hearts.transform.position;
        while (color.a - 0 > 0.01f)
        {
            color.a = Mathf.Lerp(color.a, 0, Time.deltaTime * 2);

            Hearts.color = color;
            HeartsPlus.color = color;
            Hearts.transform.position += new Vector3(0, 0.01f, 0);
            yield return null;
        }

        Hearts.transform.position = originalPosition;
        color.a = 0f;

        Hearts.color = color;
        HeartsPlus.color = color;
    }

    public void Yes()
    {
        GameClock clock = GameManager.Instance.GameClock;
        Player player = GameManager.Instance.Player;
        var moddedEnergy = player.ModifyEnergyConsumption(amount: (int)EventData.Cost);
        if (player.EnergyDepleted(moddedEnergy)) return;

        switch (EventData.Id)
        {
            case EventType.SPIRITUAL_RETREAT:
            case EventType.PRAYER_REQUEST:
                for (int i = 0; i < EventData.Cost; i++)
                {
                    clock.Tick();
                    player.ConsumeEnergy(1);
                }
                GameManager.Instance.MissionManager.UpdateFaithPoints((int)EventData.Gain);
                break;

            case EventType.TOWN_HELP:
                for (int i = 0; i < EventData.Cost; i++)
                {
                    clock.Tick();
                    player.ConsumeEnergy(1);
                }
                GameManager.Instance.MissionManager.UpdateCharityPoints((int)EventData.Gain, null);
                break;
        }
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
            IconsGO.SetActive(EventData.EventPopupType == EventPopupType.YESNO);
            YesNoGO.SetActive(EventData.EventPopupType == EventPopupType.YESNO);
            OKGO.SetActive(EventData.EventPopupType == EventPopupType.OK);
            NextGO.SetActive(false);
        }

        EventText.text = $"{LocalizationManager.Instance.GetText(EventData.LocalizationKey, CurrentSequenceNumber)}";
    }
}