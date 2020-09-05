using System;
using System.Collections;
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

    private bool PointerDown;
    public float ButtonTimer;
    private float ButtonTimerTarget;
    private string ButtonName;

    public GameObject ChargeFx;
    public GameObject ButtonPressFx;

    void Start()
    {
        CamTransform = Camera.main.transform;
    }

    public virtual void Init(Action<string> callback, string sprite, int items, GameClock deadline, float timer = 1f)
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
            BuildingIcon.transform.localPosition = new Vector3(0, 1, 0);
        }
        else
        {
            ItemsRequiredDisplay.gameObject.SetActive(true);
            ClockIcon.gameObject.SetActive(true);
            BuildingIcon.transform.localPosition = new Vector3(0, 2, 0);
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
    }

    public void OnClick(string button)
    {
        if (LockUI)
        {
            //Play disabled sound
            return;
        }

        ChargeFx.SetActive(false);
        ButtonPressFx.SetActive(true);
        ButtonPressFx.transform.position = ChargeFx.transform.position;
        SoundManager.Instance.PlayOneShotSfx("Button");
        Callback?.Invoke(button);
        Camera.main.GetComponent<CameraControls>().SetZoomTarget(3f);
    }

    public void OnPointerDown(string button)
    {
        PointerDown = true;
        ButtonName = button;
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

    private IEnumerator ActionPauseCycle()
    {
        LockUI = true;
        Player.LockMovement = true;
        var buttons = gameObject.GetComponentsInChildren<Button>();
        for(int i = 0; i < buttons.Length; i++)
        {
            buttons[i].interactable = false;
        }

        yield return new WaitForSeconds(1);
        
        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[i].interactable = true;
        }

  //      yield return new WaitForSeconds(0.5f);

        if (!MissionManager.MissionOver)
        {
            LockUI = false;
            Player.LockMovement = false;
        }
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
    }
}
