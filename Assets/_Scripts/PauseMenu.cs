using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{

    private static PauseMenu instance;
    public TextMeshProUGUI headerText;
    public GameObject codexPanel;
    public GameObject settingsPannel;
    public GameObject saintsPanel;
    public GameObject mainPanel;
    public GameObject PauseToggleObj;

    public GameObject PauseSettings;
    public GameObject GraphicsSettings;
    public GameObject SoundSettings;
    public Toggle TutorialEnabled;
    public Toggle ShowGridToggle;
    public ToggleGroup MenuToggleGroup;

    [HideInInspector] public bool active;
    public static PauseMenu Instance
    {
        get
        {
            return instance;
        }
    }

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {

    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) || (Gamepad.current != null && Gamepad.current.startButton.wasPressedThisFrame))
        {
            Activate();
        }
        else if (active && (Gamepad.current != null && Gamepad.current.buttonEast.wasPressedThisFrame))
        {
            Activate();
        }
    }

    public void Activate()
    {
        if (UI.Instance.WeekBeginCrossFade) return;
        active = !active;
        mainPanel.SetActive(active);

        if (active)
        {
            TutorialManager.Instance.SkipTutorial = !GameSettings.Instance.TutorialToggle;

            if (GameSettings.Instance != null && TutorialEnabled != null)
            {
                TutorialEnabled.SetIsOnWithoutNotify(GameSettings.Instance.TutorialToggle);
            }

            var graphicsToggleTransform = MenuToggleGroup.transform.Find("Graphics");
            var soundToggleTransform = MenuToggleGroup.transform.Find("SoundTab");
            if (GameSettings.Instance.IsXboxMode)
            {
                //We don't allow changes to graphics settings when running in xbox mode.
                graphicsToggleTransform.gameObject.SetActive(false);
                soundToggleTransform.localPosition = new Vector3(graphicsToggleTransform.localPosition.x, soundToggleTransform.localPosition.y);
            }

            if (!GameManager.Instance.InGameSession)
            {
                PauseToggleObj.SetActive(false);


                if (GameSettings.Instance.IsXboxMode)
                {
                    ToggleSound();
                    var soundToggle = soundToggleTransform.GetComponent<Toggle>();
                    soundToggle.isOn = true;
                }
                else
                {
                    ToggleGraphics();
                    var graphicsToggle = graphicsToggleTransform.GetComponent<Toggle>();
                    graphicsToggle.isOn = true;
                }
            }
            else
            {
                PauseToggleObj.SetActive(true);
                TogglePause();
                var pauseToggle = MenuToggleGroup.transform.Find("PauseTab").GetComponent<Toggle>();
                pauseToggle.isOn = true;
            }

            ShowGridToggle.SetIsOnWithoutNotify(GameSettings.Instance.ShowGrid);

            if (GameManager.Instance.InGameSession)
                TutorialEnabled.transform.parent.gameObject.SetActive(false);
            else
                TutorialEnabled.transform.parent.gameObject.SetActive(true);
        }
    }

    public void TogglePause()
    {
        if (!GameManager.Instance.InGameSession) return;

        CloseAll();
        PauseSettings.SetActive(true);
        SoundManager.Instance.PlayOneShotSfx("Button_SFX");
    }

    public void ToggleGraphics()
    {
        CloseAll();
        GraphicsSettings.SetActive(true);
        SoundManager.Instance.PlayOneShotSfx("Button_SFX");
    }

    public void ToggleSound()
    {
        CloseAll();
        SoundSettings.SetActive(true);
        SoundManager.Instance.PlayOneShotSfx("Button_SFX");
    }

    public void ToggleTutorial()
    {
        TutorialManager.Instance.SkipTutorial = !TutorialManager.Instance.SkipTutorial;
    }

    public void ToggleGrid()
    {
        GameSettings.Instance.ToggleGrid();
    }

    public void SetHeaderText(string value)
    {
        headerText.text = value;
    }

    public void OnCodexBtnClicked()
    {
        CloseAll();
        SetHeaderText("Codex");
        codexPanel.SetActive(true);

    }

    public void OnResume()
    {
        Activate();
        SoundManager.Instance.PlayOneShotSfx("Button_SFX");
    }

    public void OnSaintBtnClicked()
    {
        SetHeaderText("Saints");
        CloseAll();
    }

    public void OnSettingsBtnClicked()
    {
        CloseAll();
        SetHeaderText("Settings");
        settingsPannel.SetActive(true);

    }

    public void CloseAll()
    {
        codexPanel.SetActive(false);
        settingsPannel.SetActive(false);
        saintsPanel.SetActive(false);
        PauseSettings.SetActive(false);
        GraphicsSettings.SetActive(false);
        SoundSettings.SetActive(false);
    }

    public void OnExitToMenuBtnClicked()
    {
        //maybe do check before quit
        SoundManager.Instance.PlayOneShotSfx("Button_SFX");
        Activate();
        GameManager.Instance.ClearData();
        StartCoroutine(ScheduleCallback(() => {
            SoundManager.Instance.EndAllTracks();
            GameManager.Instance.LoadScene("MainMenu", LoadSceneMode.Single);
        }, 1));
    }

    public void OnExitToDesktopClicked()
    {
        //maybe do check before quit
        SoundManager.Instance.PlayOneShotSfx("Button_SFX");
        StartCoroutine(ScheduleCallback(() => {
            Application.Quit();
        }, 1));
    }

    private void OnDisable()
    {
        GameSettings.Instance.Save();
    }

    private IEnumerator ScheduleCallback(Action callback, float delay)
    {
        yield return new WaitForSeconds(delay);
        callback?.Invoke();
    }
}
