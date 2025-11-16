using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class PopUI : MonoBehaviour
{
    private Transform CamTransform;
    public Action<string> Callback;
    private bool LockUI;

    private bool PointerDown;
    public float ButtonTimer;
    private float ButtonTimerTarget;
    private string ButtonName;

    public GameObject ChargeFx;
    public GameObject ButtonPressFx;

    public Slider[] ProgressBars;
    public Slider ProgressBar;

    public List<ActionButton> Buttons;
    public CameraControls CameraControls;

    private InteractableHouse MyHouse;
    private GameObject CurrentVfx;
    private GameObject CriticalCircleFX;
    public static int CriticalHitCount = 0;

    public TextMeshProUGUI RPDisplay;
    public Image GlowImage;
    public List<Image> GlowImages = new List<Image>();

    void Awake()
    {
        CamTransform = ExteriorCamera.Instance.Camera.transform;
        InteractableHouse.OnActionProgress += UpdateProgressBar;
    }

    public virtual void Init(Action<string> callback, string sprite, int items, GameClock deadline, InteractableHouse house = null, CameraControls cameraControls = null)
    {
        Callback = callback;

        MyHouse = house;
        CameraControls = cameraControls;
        if (RPDisplay != null && MyHouse != null)
        {
            RPDisplay.text = $"{MyHouse?.RelationshipPoints ?? 0}";
            transform.Find("Heart").GetComponent<TooltipMouseOver>().Loc_Key = $"{LocalizationManager.Instance.GetText("Tooltip_RP")}\n\n<b>Next Milestone: {MyHouse.GetNextRPMilestone()} RP</b>";
        }

        Buttons = gameObject.GetComponentsInChildren<ActionButton>().ToList();
        if (!GameSettings.Instance.FTUE || TutorialManager.Instance.CurrentTutorialStep >= 16)
        {
            foreach (var b in Buttons)
            {
                b.RefreshButton(house?.CanDoAction(b.ButtonName) ?? false);
                b.SetTimer(house?.SetButtonTimer(b.ButtonName) ?? 1f);
            }
        }
        else
        {
            foreach (var b in Buttons)
            {
                b.SetTimer(house?.SetButtonTimer(b.ButtonName) ?? 1f);
            }
        }

#if PLATFORM_MOBILE
        if (name.Contains("Construct"))
        {
            //exit
            Buttons[0].transform.localPosition = new Vector3(3.4f, Buttons[0].transform.localPosition.y, Buttons[0].transform.localPosition.z);
            var btn0Panel = Buttons[0].GetComponent<TooltipMouseOver>().InfoPanel;
            btn0Panel.transform.localPosition = new Vector3(2.5f, 1.6f, btn0Panel.transform.localPosition.z);
            btn0Panel.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);

            var btn1Panel = Buttons[1].GetComponent<TooltipMouseOver>().InfoPanel;
            btn1Panel.transform.localPosition = new Vector3(3.84f, btn1Panel.transform.localPosition.y, btn1Panel.transform.localPosition.z);
            btn1Panel.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);

            //pray
            Buttons[2].transform.localPosition = new Vector3(-2.2f, Buttons[2].transform.localPosition.y, Buttons[2].transform.localPosition.z);
            var btn2Panel = Buttons[2].GetComponent<TooltipMouseOver>().InfoPanel;
            btn2Panel.transform.localPosition = new Vector3(1.58f, btn2Panel.transform.localPosition.y, btn2Panel.transform.localPosition.z);
            btn2Panel.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);

        }
#endif

        var canvas = GetComponent<Canvas>();
        if (canvas != null)
        {
            canvas.worldCamera = ExteriorCamera.Instance.UICamera;
        }
    }

    public void OnClick(string button)
    {
        if (Player.LockMovement) return;
        if (LockUI)
        {
            SoundManager.Instance.PlayOneShotSfx("Button_SFX");
            return;
        }

        var myButton = Buttons.Where(b => b.ButtonName == button).FirstOrDefault();

#if PLATFORM_MOBILE
        if(!PointerDown)
        {
            var tt = myButton.GetComponent<TooltipMouseOver>();
            if (tt != null && !tt.TurnOnInfoPanel)
            {
                foreach (var b in Buttons)
                {
                    b.GetComponent<TooltipMouseOver>().TurnOnInfoPanel = false;
                    b.GetComponent<TooltipMouseOver>().HideToolTip();
                }
                tt.TurnOnInfoPanel = true;
                tt.ShowToolTip();
                return;
            }

            tt.TurnOnInfoPanel = false;
        }
#endif

        if (GameSettings.Instance.FTUE)
        {
            if (!TutorialManager.Instance.CheckTutorialButton(button))
            {
                SoundManager.Instance.PlayOneShotSfx("Button_SFX");
                return;
            }
        }
        else
        {
            if (!myButton.Enabled)
            {
                SoundManager.Instance.PlayOneShotSfx("Button_SFX");
                return;
            }
        }

        myButton.Wiggle();
        Vector3 fxpos = UICam.Instance.Camera.ScreenToWorldPoint(Input.mousePosition);
        if (MyHouse != null && MyHouse.BuildingState == BuildingState.RUBBLE)
        {
            ChargeFx.transform.position = myButton.transform.position;
        }
        else
        {
            ChargeFx.transform.position = myButton.transform.position + new Vector3(0, 0, -5f);
        }
        ChargeFx.SetActive(false);
        //  if (myButton.ButtonName == "EXIT" || myButton.ButtonName == "ENTER")
        SoundManager.Instance.PlayOneShotSfx("Button_SFX");
        //else

        CameraControls?.SetZoomTarget(Constants.INTERIOR_ZOOM_IN_TARGET);

        if (myButton.ButtonName == "EXIT" || myButton.ButtonName == "ENTER" || myButton.ButtonName == "WORLD")
        {
            myButton.SendMessage("HideToolTip", SendMessageOptions.DontRequireReceiver);
        }

        Callback?.Invoke(button);

        foreach(var glow in GlowImages)
        {
            if(glow.transform.parent == myButton.transform)
            {
                FlashIconOnCompletedAction(glow);
            }
        }

        if (RPDisplay != null && MyHouse != null)
        {
            RPDisplay.text = $"{MyHouse?.RelationshipPoints ?? 0}";
            transform.Find("Heart").GetComponent<TooltipMouseOver>().Loc_Key = $"{LocalizationManager.Instance.GetText("Tooltip_RP")}\n\n<b>Next Milestone: {MyHouse.GetNextRPMilestone()} RP</b>";
        }

#if !PLATFORM_MOBILE
        if (myButton.ButtonName != "EXIT" && myButton.ButtonName != "WORLD")
            myButton.SendMessage("ShowToolTip", SendMessageOptions.DontRequireReceiver);
#endif
    }

    public void OnPointerDown(string button)
    {
        if (Player.LockMovement) return;

        var myButton = Buttons.FirstOrDefault(b => b.ButtonName == button);

#if PLATFORM_MOBILE
        var tt = myButton.GetComponent<TooltipMouseOver>();
        if(tt != null && !tt.TurnOnInfoPanel)
        {
            foreach(var b in Buttons)
            {
                b.GetComponent<TooltipMouseOver>().TurnOnInfoPanel = false;
                b.GetComponent<TooltipMouseOver>().HideToolTip();
            }
            tt.TurnOnInfoPanel = true;
            tt.ShowToolTip();
            return;
        }

        tt.TurnOnInfoPanel = false;
#endif

        if (GameSettings.Instance.FTUE)
        {
            if (!TutorialManager.Instance.CheckTutorialButton(button))
            {
                SoundManager.Instance.PlayOneShotSfx("Button_SFX");
                return;
            }
        }
        else
        {
            if (!myButton.Enabled)
            {
                Player player = GameManager.Instance.Player;
                if (player.EnergyDepleted())
                {
                    UI.Instance.ErrorFlash("Energy");
                    return;
                }

                SoundManager.Instance.PlayOneShotSfx("Button_SFX");
                return;
            }
        }

        PointerDown = true;
        ButtonName = button;
        ButtonTimerTarget = myButton.Timer;
        ChargeFx.SetActive(true);
        Vector3 fxpos = UICam.Instance.Camera.ScreenToWorldPoint(Input.mousePosition);
        if(MyHouse != null && MyHouse.BuildingState == BuildingState.RUBBLE)
        {
            MyHouse.PlaySpecialChargeVfx(ButtonName);
            ChargeFx.transform.position = myButton.transform.position;
        }
        else
        {
            MyHouse.PlaySpecialChargeVfx(ButtonName);
            ChargeFx.transform.position = myButton.transform.position + new Vector3(0,0, -5f);
        }
        ExteriorCamera.Instance.GetComponent<CameraControls>().SetZoomTarget(Constants.EXTERIOR_POINTER_DOWN_ZOOM_IN_TARGET);
        CameraControls?.SetZoomTarget(Constants.POINTER_DOWN_ZOOM_IN_TARGET);

        SoundManager.Instance.PlayOneShotSfx("Charge_SFX", timeToDie: myButton.Timer);
        StartCoroutine("CriticalCircle", myButton);
    }

    private IEnumerator CriticalCircle(ActionButton myButton)
    {
        if (CriticalCircleFX == null)
        {
            CriticalCircleFX = Instantiate(Resources.Load<GameObject>("UI/CriticalCircle"));
        }
        CriticalCircleFX.SetActive(true);
        CriticalCircleFX.transform.SetParent(ChargeFx.transform, true);
        CriticalCircleFX.transform.localScale = new Vector3(0.0075f, 0.0075f, 0.0075f);
        CriticalCircleFX.transform.localPosition = Vector3.zero;
        CriticalCircleFX.transform.localEulerAngles = Vector3.zero;

        var critCircle = CriticalCircleFX.transform.GetChild(0);
        critCircle.localScale = new Vector3(5, 5, 5);
        critCircle.DOScale(new Vector3(0.3f, 0.3f, 0.3f), myButton.Timer);

        yield return new WaitForSeconds(myButton.Timer);

        //Play FX
        CriticalCircleFX.SetActive(false);
        critCircle.localScale = new Vector3(5, 5, 5);
    }


    public void OnPointerUp()
    {
        if (Player.LockMovement) return;
   
        StopCoroutine("CriticalCircle");
        if (CriticalCircleFX != null)
        {
            CriticalCircleFX.SetActive(false);
            DOTween.Kill(CriticalCircleFX.transform.GetChild(0).transform);
        }

        if (PointerDown)
        {
            var critCircleScaleX = CriticalCircleFX.transform.GetChild(0).transform.localScale.x;
            if (critCircleScaleX > 0.65f && critCircleScaleX < 0.8f)
            {
                //CRITICAL HIT!
                CameraControls?.MyCamera.DOShakeRotation(1f, 1f);
                Debug.LogWarning("CRITICAL HIT!");
                ButtonTimer = 0f;
                if (CriticalHitCount > -1)
                {
                    CriticalHitCount++;
                }
                OnClick(ButtonName);
                PointerDown = false;
                ButtonPressFx.SetActive(true);
                ButtonPressFx.transform.position = ChargeFx.transform.position;
                SoundManager.Instance.PlayOneShotSfx("ActionButton_SFX", timeToDie: 5f);
            }
            else if (critCircleScaleX < 0.65f || (critCircleScaleX > 0.8f && critCircleScaleX < 2.2f))
            {
                //Regular HIT!
                Debug.LogWarning("Regular HIT!");
                //  if(CriticalHitCount >= 1) SoundManager.Instance.PlayOneShotSfx("Crit_Bad");
                ButtonTimer = 0f;
                CriticalHitCount = -1;
                OnClick(ButtonName);
                PointerDown = false;
            }
        }

        PointerDown = false;
        ChargeFx.SetActive(false);
        ExteriorCamera.Instance.GetComponent<CameraControls>().SetZoomTarget(Constants.EXTERIOR_ZOOM_IN_TARGET);
        CameraControls?.SetZoomTarget(Constants.INTERIOR_ZOOM_IN_TARGET);
        SoundManager.Instance.StopOneShotSfx("Charge");
    }

    void Update()
    {
        if (PointerDown)
        {
            //Play VFX
            ButtonTimer += Time.deltaTime;
            if (ButtonTimer >= ButtonTimerTarget)
            {
                Debug.LogWarning("LEft Lingering Regular HIT!");
                if (CriticalHitCount >= 1) SoundManager.Instance.PlayOneShotSfx("Crit_Bad");
                CriticalHitCount = -1;
                ButtonTimer = 0f;
                OnClick(ButtonName);
                PointerDown = false;
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

        if (MyHouse?.HasResetActionProgress() ?? false)
        {
            CriticalHitCount = 0;
        }
    }

    private void UpdateProgressBar(float progress, InteractableHouse house, int progressBar = 0)
    {
        if (ProgressBars.Length < progressBar + 1) return;
        ProgressBar = ProgressBars[progressBar];
        if (ProgressBar == null || MyHouse != house) return;

        ProgressBar.gameObject.SetActive(true);
        ProgressBar.DOValue(progress, 0.5f);

        if (CriticalHitCount > 0)
        {
            ProgressBar.fillRect.GetComponent<Image>().color = Color.yellow;
        }
        else
        {
            ProgressBar.fillRect.GetComponent<Image>().color = Color.green;
        }

        if (progress > 0.99f)
        {
            StartCoroutine(DisableProgressBar());
            if (CriticalHitCount > 0)
            {
                CriticalSequence();
            }
            CriticalHitCount = 0;
        }
        else if (progress == 0)
        {
            ProgressBar.gameObject.SetActive(false);
        }
        else
        {
            if (CriticalHitCount > 0)
            {
                //    SoundManager.Instance.PlayOneShotSfx("Crit_Good");
            }
        }
    }

    private IEnumerator DisableProgressBar()
    {
        yield return new WaitForSeconds(1.5f);
        foreach (var p in ProgressBars)
        {
            p.gameObject.SetActive(false);
        }
    }

    public void PlayVFX(string vfxName, float offset = 5f)
    {
        CurrentVfx = transform.Find(vfxName)?.gameObject;
        if (CurrentVfx == null) return;

        CurrentVfx.SetActive(false);
        CurrentVfx.SetActive(true);
        var myButton = Buttons.FirstOrDefault(b => b.ButtonName == ButtonName);
        CurrentVfx.transform.localPosition = new Vector3(myButton.transform.localPosition.x, myButton.transform.localPosition.y, myButton.transform.localPosition.z-offset);
    }

    public void CriticalSequence()
    {
        var house = MyHouse as InteractableChurch;
        if (house != null && (house.LotHProgress == 2 || house.MassProgress == 2))
        {
            SoundManager.Instance.PlayOneShotSfx("MassBegin_SFX", timeToDie: 4);
        }
        else
        {
            SoundManager.Instance.PlayOneShotSfx("Cheer_SFX", 1f, 5f);
        }
    }

    public void FlashIconOnCompletedAction(Image glow)
    {
        if (glow == null) return;

        glow.DOKill(true);
        glow.color = new Color(glow.color.r, glow.color.g, glow.color.b, 1f);
        glow.transform.localScale = Vector3.one;
        glow.transform.DOScale(new Vector3(3f, 3f, 3f), 0.75f);
        glow.DOFade(0, 1f);
    }

    private void OnEnable()
    {
        UpdateProgressBars();
    }

    private void UpdateProgressBars()
    {
        for (int i = 0; i < ProgressBars.Length; i++)
        {
            if (ProgressBars[i])
            {
                if (ProgressBars[i].value < 1)
                    UpdateProgressBar(ProgressBars[i].value, MyHouse, i);

                if (MyHouse != null && MyHouse.HasResetActionProgress())
                {
                    UpdateProgressBar(0, MyHouse, i);
                }
            }
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
        if (CurrentVfx != null) CurrentVfx.SetActive(false);

        if (ProgressBar == null) return;
        ProgressBar.gameObject.SetActive(false);
    }
}
