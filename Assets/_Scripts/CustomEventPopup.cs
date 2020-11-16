using DG.Tweening;
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
    public Image Coins;

    private int CurrentSequenceNumber;

    public DOTween Dotween;

    public void Setup(CustomEventData customEvent)
    {
        transform.DOJump(transform.position, 30f, 1, 1f);
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
                c.AddTicks(Mathf.Abs((int)customEvent.Cost));
                TimeDisplay.text = $"{(int)c.Time}:{(c.Time % 1 == 0 ? "00" : "30")}";

                var moddedEnergy = player.ModifyEnergyConsumption(amount: (int)customEvent.Cost);
                EnergyDisplay.text = moddedEnergy == 0 ? "0" : moddedEnergy > 0 ? $"-{moddedEnergy}" : $"+{-moddedEnergy}";

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
            var text = LocalizationManager.Instance.GetText(customEvent.LocalizationKey, CurrentSequenceNumber);
            EventText.text = "";
            EventText.DOText(text, text.Length / 30f).SetEase(Ease.Linear);
            YesNoGO.SetActive(false);
            OKGO.SetActive(false);
            IconsGO.SetActive(false);
            NextGO.SetActive(true);
        }
        else
        {
            var text = LocalizationManager.Instance.GetText(customEvent.LocalizationKey);
            EventText.text = "";
            EventText.DOText(text, text.Length / 30f).SetEase(Ease.Linear);
        }

        if(customEvent.EventGroup == EventGroup.THANKYOU)
        {
            if(customEvent.RewardType == CustomEventRewardType.CP)
            {
                Hearts.gameObject.SetActive(true);
            }
            else if(customEvent.RewardType == CustomEventRewardType.COIN)
            {
                Coins.gameObject.SetActive(true);
            }
        }
        
        ButtonTimerTarget = 1f;
        SoundManager.Instance.PlayOneShotSfx("DialogOpen");
    }   

    public void Yes()
    {
        if (DOTween.IsTweening(EventText, true))
        {
            DOTween.Complete(EventText);
            return;
        }
        GameClock clock = GameManager.Instance.GameClock;
        Player player = GameManager.Instance.Player;
        var moddedEnergy = player.ModifyEnergyConsumption(amount: (int)EventData.Cost);
        if (player.EnergyDepleted(moddedEnergy)) return;

        Camera.main.GetComponent<CameraControls>().SetZoomTarget(3f);
        ChargeFx.SetActive(false);
        ButtonPressFx.SetActive(true);

        player.ConsumeEnergy((int)EventData.Cost);
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

        var timeCost = Mathf.Abs(EventData.Cost);
        for (int i = 0; i < timeCost; i++)
        {
            clock.Tick();
        }

        EventsManager.Instance.EventInProgress = false;
        gameObject.SetActive(false);
        SoundManager.Instance.PlayOneShotSfx("ActionButton", 0.5f, 5f);
        SaveDataManager.Instance.SaveGame();
    }

    public void No()
    {
        if (DOTween.IsTweening(EventText, true))
        {
            DOTween.Complete(EventText);
            return;
        }
        Player player = GameManager.Instance.Player;

        if(EventData.RewardType == CustomEventRewardType.FP)
            player.CurrentBuilding.UpdateFaithPoints(-(int)EventData.RejectionCost, 0);
        else
            player.CurrentBuilding.UpdateCharityPoints(-(int)EventData.RejectionCost, 0);

        EventsManager.Instance.EventInProgress = false;
        gameObject.SetActive(false);
        SoundManager.Instance.PlayOneShotSfx("Button");
        SaveDataManager.Instance.SaveGame();
    }

    public void OK()
    {
        if (DOTween.IsTweening(EventText, true))
        {
            DOTween.Complete(EventText);
            return;
        }
        EventsManager.Instance.EventInProgress = false;
        gameObject.SetActive(false);
        SoundManager.Instance.PlayOneShotSfx("Button");
    }

    public void Continue()
    {
        if (DOTween.IsTweening(EventText, true))
        {
            DOTween.Complete(EventText);
            return;
        }

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

        var text = LocalizationManager.Instance.GetText(EventData.LocalizationKey, CurrentSequenceNumber);
        EventText.text = "";
        EventText.DOText(text, text.Length / 30f).SetEase(Ease.Linear);
        SoundManager.Instance.PlayOneShotSfx("Button");
    }

    public void OnPointerDown()
    {
        if (DOTween.IsTweening(EventText, true))
        {
            DOTween.Complete(EventText);
            return;
        }
        Player player = GameManager.Instance.Player;
        var moddedEnergy = player.ModifyEnergyConsumption(amount: (int)EventData.Cost);
        if (player.EnergyDepleted(moddedEnergy)) return;

        PointerDown = true;
        ChargeFx.SetActive(true);
        Vector3 fxpos = UICam.Instance.Camera.ScreenToWorldPoint(Input.mousePosition);
        ChargeFx.transform.position = new Vector3(fxpos.x, ChargeFx.transform.position.y, ChargeFx.transform.position.z);
        Camera.main.GetComponent<CameraControls>().SetZoomTarget(2.5f);
        SoundManager.Instance.PlayOneShotSfx("Charge");
    }

    public void OnPointerUp()
    {
        PointerDown = false;
        ChargeFx.SetActive(false);
        Camera.main.GetComponent<CameraControls>().SetZoomTarget(3f);
        SoundManager.Instance.StopOneShotSfx("Charge");
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

    private void OnDisable()
    {
        ToolTipManager.Instance.ShowToolTip("");
        TooltipMouseOver.IsHovering = false;
        Hearts.gameObject.SetActive(false);
        Coins.gameObject.SetActive(false);
    }
}