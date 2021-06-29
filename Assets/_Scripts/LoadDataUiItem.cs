using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoadDataUiItem : MonoBehaviour
{

    public Button button;
    public TextMeshProUGUI day;
    public TextMeshProUGUI FP;
    public TextMeshProUGUI CP;

    public void SetUp(SaveObject data, Action<SaveObject> callBack) {

        print(data.Day);
        FP.text = data.FP.ToString();
        CP.text = data.CP.ToString();
        day.text = ((Days)data.Day).ToString();

        button.onClick.AddListener(delegate { callBack?.Invoke(data); });
    }
}