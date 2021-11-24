using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{

    private static PauseMenu instance;
    public TextMeshProUGUI headerText;
    public GameObject codexPanel;
    public GameObject settingsPannel;
    public GameObject saintsPanel;
    public GameObject mainPanel;
    public Camera testCam;
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
       // Activate(false);
    }

    public void Activate()
    {
        active = !active;
        mainPanel.SetActive(active);
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

    public void OnSaintBtnClicked()
    {
        SetHeaderText("Saints");
        CloseAll();
        //SceneManager.LoadScene("SaintsShowcase_Day");
        //saintsPanel.SetActive(true);
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
    }

    public void OnExitToMenuBtnClicked()
    {
        //maybe do check before quit
        StartCoroutine(ScheduleCallback(() => {
            SoundManager.Instance.EndAllTracks();
            GameManager.Instance.LoadScene("MainMenu", LoadSceneMode.Single);
        }, 1));
    }

    public void OnExitToDesktopClicked()
    {
        //maybe do check before quit
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
