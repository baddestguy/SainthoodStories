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

    public static PauseMenu Instance
    {
        get
        {
            if(instance == null)
            {
                Scene scene = SceneManager.GetSceneByName("PauseMenu");
                if (scene.isLoaded)
                {
                    instance = FindObjectOfType<PauseMenu>();
                    return instance;
                }
                else
                {
                    SceneManager.LoadScene(scene.name, LoadSceneMode.Additive);
                    instance = FindObjectOfType<PauseMenu>();
                    return instance;
                }
            }
            else
            {
                return instance;
            }
        }
    }


    void Start()
    {
        Activate(false);
    }

    public void Activate(bool vlaue)
    {
        mainPanel.SetActive(vlaue);
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
       
    }

    public void OnExitToDesktopClicked()
    {

    }

}
