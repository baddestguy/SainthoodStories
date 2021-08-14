using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SavedDataUiHandler : MonoBehaviour
{

    public static SavedDataUiHandler instance;

    //public GameObject MainMenuPanel;
    public GameObject loadUiItemPanel;
    public Transform contentHolder;
    public GameObject laodDataUiitemPrefab;
    public Button backButton;
    public TextMeshProUGUI backBtnText;

    private List<GameObject> UIs = new List<GameObject>();

    public Action<bool> OnUiOpen;


    private void Awake()
    {
        instance = this;
    }

    public void Pupulate(SaveObject[] data, Action<SaveObject> callback, bool ingameLoading)
    {
        //MainMenuPanel.SetActive(false);
        OnUiOpen?.Invoke(true);
        try
        {
            UI.Instance.PanelToActivateOnLoadUiEvent.SetActive(false);
        }
        catch { }
        loadUiItemPanel.SetActive(true);
        UIs = new List<GameObject>();
        if (ingameLoading)
        {
            backButton.onClick.RemoveAllListeners();
            backButton.onClick.AddListener(() => callback?.Invoke(null));
            backBtnText.text = "New Game";
        }
        else
        {
            backButton.onClick.RemoveAllListeners();
            backButton.onClick.AddListener(Back);
            backBtnText.text = "Back";
        }


        for (int i = 0; i < data.Length; i++)
        {
            LoadDataUiItem ui = Instantiate(laodDataUiitemPrefab, contentHolder).GetComponent<LoadDataUiItem>();
            SaveObject saveObject = data[i];
            print(saveObject == null);
            ui.SetUp(saveObject, callback);
            //ui.GetComponentInChildren<Button>().onClick.AddListener(
            //    delegate {
            //        callback?.Invoke(saveObject); 
            //        print((Days)saveObject.Day);
            //});
            //GetTextObject(ui.transform).text = ((Days)saveObject.Day).ToString();
            UIs.Add(ui.gameObject);
        }

    }


    private TextMeshProUGUI GetTextObject(Transform target)
    {
        int count = target.childCount;
        for (int i = 0; i < count; i++)
        {
            Transform child = target.GetChild(i);
            TextMeshProUGUI gui = child.GetComponent<TextMeshProUGUI>();
            if (gui != null)
                return gui;
        }
        return null;
    }

    public void Back()
    {
        for (int i = 0; i < UIs.Count; i++)
        {
            Destroy(UIs[i]);
        }
        //MainMenuPanel.SetActive(true);
        try
        {
            UI.Instance.PanelToActivateOnLoadUiEvent.SetActive(true);
        }
        catch { }
        Close();
    }

    public void Close()
    {
        OnUiOpen?.Invoke(false);
        loadUiItemPanel.SetActive(false);
    }


}
