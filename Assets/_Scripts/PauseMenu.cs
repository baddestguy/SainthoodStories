using System;
using System.Collections;
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

    public GameObject PauseSettings;
    public GameObject GraphicsSettings;
    public GameObject SoundSettings;
    public Toggle TutorialEnabled;

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
        if (TutorialManager.Instance != null && TutorialEnabled != null)
        {
            TutorialEnabled.SetIsOnWithoutNotify(!TutorialManager.Instance.SkipTutorial);
        }

    }

    public void Activate()
    {
        active = !active;
        mainPanel.SetActive(active);
    }

    public void TogglePause()
    {
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
