using System;
using System.Collections;
using Assets._Scripts.Xbox;
using TMPro;
using UnityEngine;
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
    [HideInInspector] public ActivePauseTab ActiveTab;
    public static PauseMenu Instance
    {
        get
        {
            return instance;
        }
    }

    public enum ActivePauseTab
    {
        Pause,
        Graphics,
        Sound
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
        if (!GameManager.Instance.PlayerHasLoggedIn) return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Activate();
        }
    }

    public void Activate()
    {
        if (UI.Instance.WeekBeginCrossFade || EventsManager.Instance.EventInProgress) return;
        active = !active;
        mainPanel.SetActive(active);

        if (active)
        {
            TutorialManager.Instance.SkipTutorial = !GameSettings.Instance.TutorialToggle;

            if (GameSettings.Instance != null && TutorialEnabled != null)
            {
                TutorialEnabled.SetIsOnWithoutNotify(GameSettings.Instance.TutorialToggle);
            }

            PauseMenuControllerHandler.Instance.Activate();

            if (!GameManager.Instance.InGameSession)
            {
                PauseToggleObj.SetActive(false);

                ToggleGraphics();
            }
            else
            {
                PauseToggleObj.SetActive(true);
                TogglePause();
            }

            ShowGridToggle.SetIsOnWithoutNotify(GameSettings.Instance.ShowGrid);

            //if (GameManager.Instance.InGameSession)
            //    TutorialEnabled.transform.parent.gameObject.SetActive(false);
            //else
            //    TutorialEnabled.transform.parent.gameObject.SetActive(true);
        }
        else
        {
            GameSettings.Instance.Save();
            if (GameSettings.Instance.IsUsingController)
            {
                PauseMenuControllerHandler.Instance.Deactivate();
            }
        }
    }

    public void TogglePause()
    {
        if (!GameManager.Instance.InGameSession) return;

        CloseAll();
        PauseSettings.SetActive(true);
        var pauseToggle = MenuToggleGroup.transform.Find("PauseTab").GetComponent<Toggle>();
        pauseToggle.SetIsOnWithoutNotify(true);
        ActiveTab = ActivePauseTab.Pause;
        SoundManager.Instance.PlayOneShotSfx("Button_SFX");
    }

    public void ToggleGraphics()
    {
        CloseAll();
        GraphicsSettings.SetActive(true);
        var graphicsToggle = MenuToggleGroup.transform.Find("Graphics").GetComponent<Toggle>();
        graphicsToggle.SetIsOnWithoutNotify(true);
        ActiveTab = ActivePauseTab.Graphics;
        SoundManager.Instance.PlayOneShotSfx("Button_SFX");
    }

    public void ToggleSound()
    {
        CloseAll();
        SoundSettings.SetActive(true);
        var graphicsToggle = MenuToggleGroup.transform.Find("SoundTab").GetComponent<Toggle>();
        graphicsToggle.SetIsOnWithoutNotify(true);
        ActiveTab = ActivePauseTab.Sound;
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
        StartCoroutine(ScheduleCallback(() =>
        {
            SoundManager.Instance.EndAllTracks();
            GameManager.Instance.LoadScene("MainMenu", LoadSceneMode.Single);
        }, 1));
    }

    public void OnExitToDesktopClicked()
    {
        //maybe do check before quit
        SoundManager.Instance.PlayOneShotSfx("Button_SFX");
        StartCoroutine(ScheduleCallback(() =>
        {
            Application.Quit();
        }, 1));
    }

    private void OnDisable()
    {
    }

    private IEnumerator ScheduleCallback(Action callback, float delay)
    {
        yield return new WaitForSeconds(delay);
        callback?.Invoke();
    }
}
