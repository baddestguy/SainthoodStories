using Assets.Xbox;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CustomEventPopup : MonoBehaviour
{
    public static bool IsDisplaying;

    private bool PointerDown;
    public float ButtonTimer;
    private float ButtonTimerTarget;
    public GameObject ChargeFx;
    public GameObject ButtonPressFx;

    public GameObject YesNoGO;
    public GameObject OKGO;
    public GameObject NextGO;
    public GameObject SkipGO;
    public CustomEventData EventData;
    public TextMeshProUGUI EventText;

    public GameObject StoryBG;
    public GameObject EventBG;
    public Text StoryEventText;
    public Image StoryImage;

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
    public CameraControls CameraControls;

    public void Setup(CustomEventData customEvent)
    {
        IsDisplaying = true;
        ToolTipManager.Instance.ShowToolTip("");
        transform.DOJump(transform.position, 30f, 1, 1f);
        EventData = customEvent;
        YesNoGO.SetActive(customEvent.EventPopupType == EventPopupType.YESNO);
        IconsGO.SetActive(customEvent.EventPopupType == EventPopupType.YESNO);
        OKGO.SetActive(customEvent.EventPopupType == EventPopupType.OK);
        SkipGO.SetActive(customEvent.EventGroup == EventGroup.STORY);
        CameraControls = GetCameraControl();

        StoryImage.gameObject.SetActive(true);
        StoryImage.sprite = Resources.Load<Sprite>(customEvent.ImagePath);
        if (StoryImage.sprite == null) StoryImage.gameObject.SetActive(false);

        switch (customEvent.EventPopupType)
        {
            case EventPopupType.YESNO:
                GameClock clock = GameManager.Instance.GameClock;
                Player player = GameManager.Instance.Player;

                GameClock c = new GameClock(clock.Time);
                c.AddTicks(Mathf.Abs((int)customEvent.Cost));
                TimeDisplay.text = $"{(int)c.Time}:{(c.Time % 1 == 0 ? "00" : "30")}";

                var moddedEnergy = player.ModifyEnergyConsumption(amount: EventData.EnergyCost);
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
            StoryEventText.text = "";
            StoryEventText.DOText(text, text.Length / 30f).SetEase(Ease.Linear);
            YesNoGO.SetActive(false);
            OKGO.SetActive(false);
            IconsGO.SetActive(false);
            NextGO.SetActive(true);
        }
        else
        {
            var text = LocalizationManager.Instance.GetText(customEvent.LocalizationKey);
            StoryEventText.text = "";
            StoryEventText.DOText(text, text.Length / 30f).SetEase(Ease.Linear);
            NextGO.SetActive(false);
        }

        if (customEvent.EventGroup == EventGroup.THANKYOU || customEvent.EventGroup == EventGroup.ENDWEEK)
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
        
        FindObjectOfType<GamepadCursor>().SnapToLocation(OKGO.transform.position);

        ButtonTimerTarget = 1f;
        SoundManager.Instance.PlayOneShotSfx("DialogOpen_SFX");
        GameplayControllerHandler.Instance.SetCurrentCustomEventPopup(this);
    }

    public void Yes()
    {
        if (DOTween.IsTweening(StoryEventText, true))
        {
            DOTween.Complete(StoryEventText);
            return;
        }
        InteractableHouse.HouseTriggeredEvent = CustomEventType.NONE;
        GameClock clock = GameManager.Instance.GameClock;
        Player player = GameManager.Instance.Player;
        var moddedEnergy = player.ModifyEnergyConsumption(amount: EventData.EnergyCost);
        if (player.CanUseEnergy(moddedEnergy)) return;

        ExteriorCamera.Instance.GetComponent<CameraControls>().SetZoomTarget(3f);
        CameraControls?.SetZoomTarget(6f);
        ChargeFx.SetActive(false);
        ButtonPressFx.SetActive(true);

        player.ConsumeEnergy(EventData.EnergyCost);
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

        player.CurrentBuilding.ClearHazard();
        player.CurrentBuilding.BuildRelationship(ThankYouType.IMMEDIATE_ASSISTANCE);
        if (player.CurrentBuilding.MyObjective?.Event == BuildingEventType.SPECIAL_EVENT || player.CurrentBuilding.MyObjective?.Event == BuildingEventType.SPECIAL_EVENT_URGENT)
        {
            player.CurrentBuilding.CurrentMissionCompleteToday = true;
            player.CurrentBuilding.CurrentMissionId++;
            player.CurrentBuilding.MyObjective = null;
        }

        var timeCost = Mathf.Abs(EventData.Cost);
        for (int i = 0; i < timeCost; i++)
        {
            clock.Tick();
        }

        EventsManager.Instance.EventInProgress = false;
        gameObject.SetActive(false);
        SoundManager.Instance.PlayOneShotSfx("ActionButton_SFX", timeToDie: 5f);
    }

    public void No()
    {
        if (DOTween.IsTweening(StoryEventText, true))
        {
            DOTween.Complete(StoryEventText);
            return;
        }

        InteractableHouse.HouseTriggeredEvent = CustomEventType.NONE;
        Player player = GameManager.Instance.Player;
        if (EventData.RewardType == CustomEventRewardType.FP)
            player.CurrentBuilding.UpdateFaithPoints(-(int)EventData.RejectionCost, 0);
        else
            player.CurrentBuilding.UpdateCharityPoints(-(int)EventData.RejectionCost, 0);

        EventsManager.Instance.EventInProgress = false;
        gameObject.SetActive(false);
        SoundManager.Instance.PlayOneShotSfx("Button_SFX");
        SoundManager.Instance.PlayOneShotSfx("FailedDeadline_SFX");
    }

    public void OK()
    {
        if (DOTween.IsTweening(StoryEventText, true))
        {
            DOTween.Complete(StoryEventText);
            return;
        }
        EventsManager.Instance.EventInProgress = false;
        gameObject.SetActive(false);
        SoundManager.Instance.PlayOneShotSfx("Button_SFX");
    }

    public void Skip()
    {
        if (DOTween.IsTweening(StoryEventText, true))
        {
            DOTween.Complete(StoryEventText);
        }

        OK();
    }

    public void Continue()
    {
        
        if (DOTween.IsTweening(StoryEventText, true))
        {
            DOTween.Complete(StoryEventText);
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

            if (GameSettings.Instance.IsUsingController)
            {
                if(EventData.EventPopupType == EventPopupType.YESNO) YesNoGO.GetComponentsInChildren<TooltipMouseOver>()[0].HandleControllerHover();
                if(EventData.EventPopupType == EventPopupType.OK) OKGO.GetComponent<TooltipMouseOver>().HandleControllerHover();
            }
        }

        var text = LocalizationManager.Instance.GetText(EventData.LocalizationKey, CurrentSequenceNumber);
        StoryEventText.text = "";
        StoryEventText.DOText(text, text.Length / 30f).SetEase(Ease.Linear);
        SoundManager.Instance.PlayOneShotSfx("Button_SFX");
    }

    public void OnPointerDown()
    {
        if (DOTween.IsTweening(StoryEventText, true))
        {
            DOTween.Complete(StoryEventText);
            return;
        }
        Player player = GameManager.Instance.Player;
        var moddedEnergy = player.ModifyEnergyConsumption(amount: EventData.EnergyCost);
        if (player.CanUseEnergy(moddedEnergy) || player.CurrentBuilding.BuildingState == BuildingState.RUBBLE) return;

        PointerDown = true;
        ChargeFx.SetActive(true);
        ExteriorCamera.Instance.GetComponent<CameraControls>().SetZoomTarget(2.5f);
        CameraControls?.SetZoomTarget(5.5f);
        SoundManager.Instance.PlayOneShotSfx("Charge_SFX");
    }

    public void OnPointerUp()
    {
        PointerDown = false;
        ChargeFx.SetActive(false);
        ExteriorCamera.Instance.GetComponent<CameraControls>().SetZoomTarget(3f);
        CameraControls?.SetZoomTarget(6f);
        SoundManager.Instance.StopOneShotSfx("Charge_SFX");
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

        if (GameSettings.Instance.IsUsingController && EventData.Id == CustomEventType.ENDGAME_DEMO)
        {
            var pressedButton = GamePadController.GetButton();
            if (pressedButton.Button == GamePadButton.South && pressedButton.Control.wasPressedThisFrame)
            {
                int sequences = LocalizationManager.Instance.GetTotalSequences(EventData.LocalizationKey);

                if (CurrentSequenceNumber >= sequences || sequences - CurrentSequenceNumber == 1)
                {
                    OK();
                }
                else
                {
                    Continue();
                }
            }
        }
    }

    public CameraControls GetCameraControl()
    {
        InteractableHouse house = null;
        if (GameManager.Instance.CurrentHouse == null) return null;

        switch (GameManager.Instance.CurrentHouse.GetType().Name)
        {
            case "InteractableChurch":
                house = FindObjectOfType<InteractableChurch>();
                break;
            case "InteractableHospital":
                house = FindObjectOfType<InteractableHospital>();
                break;
            case "InteractableKitchen":
                house = FindObjectOfType<InteractableKitchen>();
                break;
            case "InteractableOrphanage":
                house = FindObjectOfType<InteractableOrphanage>();
                break;
            case "InteractableShelter":
                house = FindObjectOfType<InteractableShelter>();
                break;
            case "InteractableSchool":
                house = FindObjectOfType<InteractableSchool>();
                break;
            case "InteractableClothesBank":
                house = FindObjectOfType<InteractableClothesBank>();
                break;
        }

        return house ==  null || house.InteriorCam == null ? null : house.InteriorCam.GetComponent<CameraControls>();
    }

    public void OnOveride()
    {
        EventsManager.Instance.EventInProgress = false;
        gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        IsDisplaying = false;
        ToolTipManager.Instance.ShowToolTip("");
        TooltipMouseOver.IsHovering = false;
        Hearts.gameObject.SetActive(false);
        Coins.gameObject.SetActive(false);
        EventSystem.current?.SetSelectedGameObject(null);
        GamepadCursor.CursorSpeed = 2000f;
        GameplayControllerHandler.Instance.SetCurrentCustomEventPopup(null);
    }
}