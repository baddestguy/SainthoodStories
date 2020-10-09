using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CustomEventPopup : MonoBehaviour
{
    private bool PointerDown;
    public float ButtonTimer;
    private float ButtonTimerTarget;
    public GameObject ChargeFx;
    public GameObject ButtonPressFx;

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
    public GameObject CoinIcon;

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

                FPIcon.SetActive(customEvent.RewardType == CustomEventRewardType.FP);
                CPIcon.SetActive(customEvent.RewardType == CustomEventRewardType.CP);
                CoinIcon.SetActive(customEvent.RewardType == CustomEventRewardType.COIN);
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
        else
        {
            Color color = Hearts.color;
            color.a = 0f;
            Hearts.color = color;
            HeartsPlus.color = color;
        }
        
        ButtonTimerTarget = 1f;
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

        ChargeFx.SetActive(false);
        ButtonPressFx.SetActive(true);

        player.ConsumeEnergy(moddedEnergy);
        switch (EventData.RewardType) {
            case CustomEventRewardType.FP:
                player.CurrentBuilding.UpdateFaithPoints((int)EventData.Gain, -moddedEnergy);
                break;

            case CustomEventRewardType.CP:
                player.CurrentBuilding.UpdateCharityPoints((int)EventData.Gain, moddedEnergy);
                break;

            case CustomEventRewardType.COIN:
                TreasuryManager.Instance.DonateMoney((int)EventData.Gain);
                break;
        }

        for (int i = 0; i < EventData.Cost; i++)
        {
            clock.Tick();
        }

        EventsManager.Instance.EventInProgress = false;
        gameObject.SetActive(false);
    }

    public void No()
    {
        Player player = GameManager.Instance.Player;
        player.CurrentBuilding.UpdateCharityPoints(-(int)EventData.RejectionCost, 0);

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

    public void OnPointerDown()
    {
        Player player = GameManager.Instance.Player;
        var moddedEnergy = player.ModifyEnergyConsumption(amount: (int)EventData.Cost);
        if (player.EnergyDepleted(moddedEnergy)) return;

        PointerDown = true;
        ChargeFx.SetActive(true);
        Vector3 fxpos = UICam.Instance.Camera.ScreenToWorldPoint(Input.mousePosition);
        ChargeFx.transform.position = new Vector3(fxpos.x, ChargeFx.transform.position.y, ChargeFx.transform.position.z);
        Camera.main.GetComponent<CameraControls>().SetZoomTarget(2.5f);
    }

    public void OnPointerUp()
    {
        PointerDown = false;
        ChargeFx.SetActive(false);
        Camera.main.GetComponent<CameraControls>().SetZoomTarget(3f);
    }

    void Update()
    {
        if (PointerDown)
        {
            ButtonTimer += Time.deltaTime;
            if (ButtonTimer >= ButtonTimerTarget)
            {
                PointerDown = false;
                ButtonTimer = 0f;
                Yes();
            }
        }
        else
        {
            ButtonTimer -= Time.deltaTime;
            if (ButtonTimer <= 0)
            {
                ButtonTimer = 0;
            }
        }
    }

}