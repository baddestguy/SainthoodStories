using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SavedDataUiHandler : MonoBehaviour
{
    public GameObject MainMenuPanel;
    public GameObject loadUiItemPanel;
    public Transform contentHolder;
    public GameObject laodDataUiitemPrefab;

    private List<GameObject> UIs = new List<GameObject>();

    private void Start()
    {
        SaveDataManager.Instance.savedDataUiHandler = this;
    }

    public void Pupulate(SaveObject[] data, Action<SaveObject> callback)
    {
        MainMenuPanel.SetActive(false);
        loadUiItemPanel.SetActive(true);
        UIs = new List<GameObject>();
        for (int i = 0; i < data.Length; i++)
        {
            GameObject ui = Instantiate(laodDataUiitemPrefab, contentHolder);
            SaveObject saveObject = data[i];
            ui.GetComponentInChildren<Button>().onClick.AddListener(
                delegate {
                    callback?.Invoke(saveObject); 
                    print((Days)saveObject.Day);
            });
            GetTextObject(ui.transform).text = ((Days)saveObject.Day).ToString();
            UIs.Add(ui);
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
        MainMenuPanel.SetActive(true);
        loadUiItemPanel.SetActive(false);
    }

    private void Close()
    {

    }

    
}
