using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopUI : MonoBehaviour
{
    private Transform CamTransform;
    public Action<string> Callback;
    private bool LockUI;

    public Image BuildingIcon;
    public Image ClockIcon;
    public TextMeshProUGUI ItemsRequiredDisplay;
    public TextMeshProUGUI DeadlineDisplay;
    public TextMeshProUGUI IsCurrentlyOpen;

    private bool PointerDown;
    public float ButtonTimer;
    private float ButtonTimerTarget;
    private string ButtonName;

    public GameObject ChargeFx;
    public GameObject ButtonPressFx;

    public Slider ProgressBar;

    private List<ActionButton> Buttons;

    void Start()
    {
        CamTransform = Camera.main.transform;
        InteractableHouse.OnActionProgress += UpdateProgressBar;
    }

    public virtual void Init(Action<string> callback, string sprite, int items, GameClock deadline, InteractableHouse house = null, float timer = 1f)
    {
        Callback = callback;

        BuildingIcon.sprite = Resources.Load<Sprite>($"Icons/{sprite}");
        ItemsRequiredDisplay.text = $"{items}";
        DeadlineDisplay.text = $"{(int)deadline.Time}:{(deadline.Time % 1 == 0 ? "00" : "30")}";
        ButtonTimerTarget = timer;

        if (items <= 0)
        {
            ItemsRequiredDisplay.gameObject.SetActive(false);
            ClockIcon.gameObject.SetActive(false);
            BuildingIcon.transform.localPosition = new Vector3(0, 1.5f, 0);
            if (house != null)
            {
                IsCurrentlyOpen.text = house.BuildingState == BuildingState.RUBBLE ? LocalizationManager.Instance.GetText("UI_ConstructionSite") : house.DuringOpenHours() ? LocalizationManager.Instance.GetText("UI_Open") : LocalizationManager.Instance.GetText("UI_Closed");
            }
            else
            {
                IsCurrentlyOpen.text = "";
            }
        }
        else
        {
            ItemsRequiredDisplay.gameObject.SetActive(true);
            ClockIcon.gameObject.SetActive(true);
            BuildingIcon.transform.localPosition = new Vector3(0, 2, 0);
            IsCurrentlyOpen.text = "";
        }

        GameClock clock = GameManager.Instance.GameClock;
        if (clock.TimeDifference(deadline) <= 1.5)
        {
            DeadlineDisplay.color = Color.red;
            ClockIcon.color = Color.red;
        }
        else
        {
            DeadlineDisplay.color = Color.white;
            ClockIcon.color = Color.white;
        }

        Buttons = gameObject.GetComponentsInChildren<ActionButton>().ToList();
        if (!GameSettings.Instance.FTUE || TutorialManager.Instance.CurrentTutorialStep >= 20)
        {
            foreach (var b in Buttons)
            {
                b.RefreshButton(house?.CanDoAction(b.ButtonName) ?? false);
            }
        }
    }

    public void OnClick(string button)
    {
        if (LockUI)
        {
            SoundManager.Instance.PlayOneShotSfx("Button");
            return;
        }

        var myButton = Buttons.Where(b => b.ButtonName == button).FirstOrDefault();
        if (GameSettings.Instance.FTUE)
        {
            if (!TutorialManager.Instance.CheckTutorialButton(button))
            {
                SoundManager.Instance.PlayOneShotSfx("Button");
                return;
            }
        }
        else
        {
            if (!myButton.Enabled)
            {
                SoundManager.Instance.PlayOneShotSfx("Button");
                return;
            }
        }

        Vector3 fxpos = UICam.Instance.Camera.ScreenToWorldPoint(Input.mousePosition);
        ChargeFx.transform.position = myButton.transform.position - new Vector3(0, -0.1f, 0.1f);
        ChargeFx.SetActive(false);
        ButtonPressFx.SetActive(true);
        ButtonPressFx.transform.position = ChargeFx.transform.position;
        SoundManager.Instance.PlayOneShotSfx("ActionButton", 0.5f, 5f);
        Camera.main.GetComponent<CameraControls>().SetZoomTarget(3f);

        if (GameSettings.Instance.FTUE)
        {
            TutorialManager.Instance.NextTutorialStep();
            BroadcastMessage("RefreshTutorialButton", SendMessageOptions.DontRequireReceiver);
        }
        Callback?.Invoke(button);
        myButton.SendMessage("ShowToolTip", SendMessageOptions.DontRequireReceiver);
    }

    public void OnPointerDown(string button)
    {
        var myButton = Buttons.Where(b => b.ButtonName == button).FirstOrDefault();

        if (GameSettings.Instance.FTUE)
        {
            if (!TutorialManager.Instance.CheckTutorialButton(button))
            {
                SoundManager.Instance.PlayOneShotSfx("Button");
                return;
            }
        }
        else
        {
            if (!myButton.Enabled)
            {
               SoundManager.Instance.PlayOneShotSfx("Button");
                return;
            }
        }

        PointerDown = true;
        ButtonName = button;
        ChargeFx.SetActive(true);
        Vector3 fxpos = UICam.Instance.Camera.ScreenToWorldPoint(Input.mousePosition);
        ChargeFx.transform.position = myButton.transform.position - new Vector3(0, -0.1f, 0.1f);
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
        transform.forward = CamTransform.forward;

        if (PointerDown)
        {
            //Play VFX
            ButtonTimer += Time.deltaTime;
            if(ButtonTimer >= ButtonTimerTarget)
            {
                PointerDown = false;
                ButtonTimer = 0f;
                OnClick(ButtonName);
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

    private void UpdateProgressBar(float progress)
    {
        if (ProgressBar == null) return;

        ProgressBar.gameObject.SetActive(true);
        ProgressBar.value = progress;

        if(progress > 0.75f)
        {
            ProgressBar.gameObject.SetActive(false);
        }
    }


    private void OnDisable()
    {
        var buttons = gameObject.GetComponentsInChildren<Button>();
        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[i].interactable = true;
        }
        LockUI = false;
        Player.LockMovement = false;
        ChargeFx.SetActive(false);
        ButtonPressFx.SetActive(false);
        
        if (ProgressBar == null) return;
        ProgressBar.gameObject.SetActive(false);
    }
}
